using Akka.Actor;
using Akka.Event;
using System;
using static AkkaSim.Definitions.SimulationMessage;

namespace AkkaSim
{
    public class SimulationMonitor : UntypedActor, ILogReceive
    {
        protected long _Time;
        private EventStream _EventStream;
        private Type _Channel;
        public SimulationMonitor(EventStream eventStream, long time, Type channel)
        {
            _Time = time;
            _EventStream = eventStream;
            _Channel = channel;
        }

        protected override void PreStart()
        {
            _EventStream.Subscribe(Self, _Channel);
            _EventStream.Subscribe(Self, typeof(AdvanceTo));
            base.PreStart();
        }

        public static Props Props(EventStream eventStream, long time, Type channel)
        {
            return Akka.Actor.Props.Create(() => new SimulationMonitor(eventStream, time, channel));
        }

        protected override void PostStop()
        {
            _EventStream.Unsubscribe(Self, _Channel);
            base.PostStop();
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case AdvanceTo m: _Time = m.TimePeriod;
                    break;
                default:
                    EventHandle(message);
                    break;
            }
        }
        
        protected virtual void EventHandle(object o)
        {
            Console.WriteLine($"Letter captured: { o.ToString() }, sender: { Sender }");
        }
    }
}
