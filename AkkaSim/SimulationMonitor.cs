using Akka.Actor;
using Akka.Event;
using System;
using static AkkaSim.Definitions.SimulationMessage;

namespace AkkaSim
{
    public class SimulationMonitor : UntypedActor, ILogReceive
    {
        protected long _Time;
        private Type _Channel;
        public SimulationMonitor(long time, Type channel)
        {
            _Time = time;
            _Channel = channel;
                       
        }

        protected override void PreStart()
        {
            Context.System.EventStream.Subscribe(Self, _Channel);
            Context.System.EventStream.Subscribe(Self, typeof(AdvanceTo));
            base.PreStart();
        }

        public static Props Props(long time, Type channel)
        {
            return Akka.Actor.Props.Create(() => new SimulationMonitor(time, channel));
        }

        protected override void PostStop()
        {
            Context.System.EventStream.Unsubscribe(Self, _Channel);
            Context.System.EventStream.Unsubscribe(Self, typeof(AdvanceTo));
            base.PostStop();
        }

        protected override void OnReceive(object message)
        {
            
            switch (message)
            {
                case AdvanceTo m: _Time = m.TimePeriod;
                    break;
                case Shutdown c: Shutdown();
                    break;
                default:
                    EventHandle(message);
                    break;
            }
        }
        
        protected virtual void EventHandle(object o)
        {
            System.Diagnostics.Debug.WriteLine($"Letter captured: { o.ToString() }, sender: { Sender }");
        }

        protected virtual void Shutdown()
        {
            Context.Stop(Self);
        }
    }
}
