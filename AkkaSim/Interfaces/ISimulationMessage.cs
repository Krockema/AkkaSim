using Akka.Actor;
using AkkaSim.Definitions;

namespace AkkaSim.Interfaces
{
    public interface ISimulationMessage
    {
        object Message { get; }
        IActorRef Target { get; }
        ActorSelection TargetSelection { get; }
        Priority Priority { get; }
        int CompareTo(ISimulationMessage other);
    }
}