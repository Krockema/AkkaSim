using System;
using Akka.Actor;
using AkkaSim;
using AkkaSim.Interfaces;
using SimTest.Machine;

namespace TestAkkaSim.Moc
{
    public class ActingObject : SimulationElement
    {
        public ActingObject(IActorRef simulationContext, long time) : base(simulationContext, time)
        {
        }

        protected override void Do(object process)
        {
            switch (process)
            {
                case Work w: Schedule(long.Parse(w.Message.ToString()), new Work("Done", Sender));
                    break;
                default:
                    throw new Exception("No Message Handler implemented for " + process.GetType().Name);
            }
        }
    }
}
