using Akka.Actor;
using AkkaSim.Internals;
using AkkaSim.Public;
using System;
using System.Collections.Generic;
using static AkkaSim.Public.SimulationMessage;

namespace AkkaSim
{
    public abstract class SimulationElement : ReceiveActor, ISimulationElement
    {
        /// <summary>
        /// Referencing the Global Simulation Context. (Head Actor)
        /// </summary>
        protected IActorRef _SimulationContext { get; }
        
        /// <summary>
        /// Guid of the current element
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        /// Current Time
        /// </summary>
        protected long TimePeriod { get; private set; }

        /// <summary>
        /// Store for featured messages
        /// </summary>
        private Dictionary<long, PriorityQueue<SimulationMessage>> _messageStash = new Dictionary<long, PriorityQueue<SimulationMessage>>();

        /// <summary>
        /// Register the simulation element at the Simulation
        /// </summary>
        protected sealed override void PreStart()
        {
            _SimulationContext.Tell(Command.Registration, Self);
            base.PreStart();
        }

        public SimulationElement(IActorRef simulationContext, long time)
        {
            TimePeriod = time;
            _SimulationContext = simulationContext;

            Receive<Command>(f => f == Command.Finish, f => {
                Finish();
                _SimulationContext.Tell(Command.Done, null);
            });

            Receive<Schedule>(message => ScheduleMessages(message.Delay, (SimulationMessage)message.Message));

            Receive<AdvanceTo>(m => AdvanceTo(m.TimePeriod));

            // any Message that is not handled internaly
            ReceiveAny(m => MapMessageToMethod(m));
        }

        /// <summary>
        /// Deregister the Actor from Context and Tell parrent Elements that his work is done.
        /// </summary>
        protected sealed override void PostStop()
        {
            _SimulationContext.Tell(Command.DeRegistration, Self);

            // tell parrents
            var p = Context.Parent;
            if (!(p == _SimulationContext))
                _SimulationContext.Tell(new Finish(p), Self);

            base.PostStop();
        }

        private void Terminate() {
            
            // check if all childs Finished
            var childs = Context.GetChildren();
            foreach (var child in childs)
                if (child.Path != Sender.Path) return;

            // geratefully shutdown
            Context.Stop(Self);
        }

        private void AdvanceTo(long time) {
            //Console.WriteLine(Self.Path + " Advancing time to " + time);
            TimePeriod = time;
            ReleaseMessagesForThisTimeperiod();
        }

        private void MapMessageToMethod(object message)
        {
            Do(message);
            _SimulationContext.Tell(Command.Done, null);
        }

        private void ScheduleMessages(long delay, SimulationMessage message)
        {
            var atTime = delay + TimePeriod;
            if (!_messageStash.TryGetValue(atTime, out PriorityQueue<SimulationMessage> stash))
            {
                stash = new PriorityQueue<SimulationMessage>();
                _messageStash.Add(atTime, stash);
            }
            stash.Enqueue(message);
        }

        private void ReleaseMessagesForThisTimeperiod()
        {
            var thereWasWork = _messageStash.TryGetValue(TimePeriod, out PriorityQueue<SimulationMessage> stash);
            // One by one.
            while (stash != null && stash.Count() != 0)
                MapMessageToMethod(stash.Dequeue());
            // Free up Space
            if (thereWasWork)
                _messageStash.Remove(TimePeriod);
        }
        
        /// <summary>
        /// Free for implementing your own behave on messages 
        /// </summary>
        /// <param name="process"></param>
        protected virtual void Do(object process)
        {

        }

        /// <summary>
        /// Anything that has to be done before shutdown, default it will terminate the Actor if the Actor hase no more childs.
        /// </summary>
        /// <param name="process"></param>
        protected virtual void Finish()
        {
            Terminate();
        }
    }
}