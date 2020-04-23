using Akka.Actor;
using AkkaSim.Definitions;

namespace TestAkkaSim.Moc
{
    public class Work : SimulationMessage
    {
        public Work(object message, IActorRef target) : base(message, target)
        {

        }

    }
}
