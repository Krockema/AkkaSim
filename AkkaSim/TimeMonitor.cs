using System;
using Akka.Actor;
using AkkaSim.Definitions;

namespace AkkaSim
{
    /// <summary>
    /// A Time Monitor that does perform an injected Action on TimeAdvance event.
    /// </summary>
    public class TimeMonitor :  ReceiveActor
    {
        private Action<long> TimeReporter;
        public TimeMonitor(Action<long> report)
        {
            #region init
            TimeReporter = report;
            #endregion
            
            Receive<SimulationMessage.AdvanceTo>(dl =>
                TimeReporter(dl.TimePeriod)
            );
        }

        protected sealed override void PreStart()
        {
            Context.System.EventStream.Subscribe(Self, typeof(SimulationMessage.AdvanceTo));
            base.PreStart();
        }

        protected sealed override void PostStop()
        {
            //_SimulationContext.Tell(Command.DeRegistration, Self);
            Context.System.EventStream.Unsubscribe(Self, typeof(SimulationMessage.AdvanceTo));
            base.PostStop();
        }
    }
}

