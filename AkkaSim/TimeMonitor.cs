using Akka.Actor;
using System;
using static AkkaSim.Definitions.SimulationMessage;

namespace Master40.SimulationCore.Reporting
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
            
            Receive<AdvanceTo>(dl => 
                report(dl.TimePeriod)
            );
        }

        protected sealed override void PreStart()
        {
            Context.System.EventStream.Subscribe(Self, typeof(AdvanceTo));
            base.PreStart();
        }

        protected sealed override void PostStop()
        {
            //_SimulationContext.Tell(Command.DeRegistration, Self);
            Context.System.EventStream.Unsubscribe(Self, typeof(AdvanceTo));
            base.PostStop();
        }
    }
}

