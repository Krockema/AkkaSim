using Akka.Actor;
using AkkaSim.Definitions;

namespace TestAkkaSim.Moc
{
    public class Ping : SimulationMessage
    {
        public const string Name = "PING";
        public Ping(object message, IActorRef target) : base(message, target)
        {

        }

    }
}
