using Akka.Actor;
using System;
using System.Collections.Generic;
using static AkkaSim.Definitions.SimulationMessage;

namespace AkkaSim
{
    public class SimulationMonitor : UntypedActor, ILogReceive
    {
        protected long _Time;
        private List<Type> _Channels;
        public SimulationMonitor(long time, List<Type> channels)
        {
            _Time = time;
            _Channels = channels;
                       
        }

        protected override void PreStart()
        {
            _Channels.ForEach(channel => Context.System.EventStream.Subscribe(Self, channel));
            Context.System.EventStream.Subscribe(Self, typeof(AdvanceTo));

            base.PreStart();
        }

        public static Props Props(long time, List<Type> channels)
        {
            return Akka.Actor.Props.Create(() => new SimulationMonitor(time, channels));
        }

        protected override void PostStop()
        {
            _Channels.ForEach(channel => Context.System.EventStream.Unsubscribe(Self, channel));
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
