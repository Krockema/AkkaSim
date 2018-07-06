using Akka.Actor;
using AkkaSim.Interfaces;
using AkkaSim.Definitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimTest.MachineQueue
{
    partial class MachineJobDistributor
    {
        public enum Command
        {
            GetWork
        }
        public class ProductionOrder : SimulationMessage
        {
            public ProductionOrder(object message, IActorRef target) : base(message, target)
            {
            }
        }

        public class ProductionOrderFinished : SimulationMessage
        {
            public ProductionOrderFinished(object message, IActorRef target) : base(message, target)
            {
            }
        }

        public class AddMachine : SimulationMessage
        {
            public AddMachine(object message, IActorRef target) : base(message, target)
            {
            }
        }
    }
}
