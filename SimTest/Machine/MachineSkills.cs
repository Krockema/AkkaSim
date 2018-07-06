using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using AkkaSim.Definitions;
using SimTest.Domain;

namespace SimTest.Machine
{
    partial class MachineAgent
    {
        public enum Command
        {
            Ready
        }

        public class Work : SimulationMessage
        {
            public Work(object message, IActorRef target) : base(message, target)
            { }
        }
    
        public class FinishWork : SimulationMessage
        {
            public FinishWork(object Message, IActorRef target) : base(Message, target)
            { }
        }
    }
}
