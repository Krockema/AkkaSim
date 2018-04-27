using Akka.Actor;
using AkkaSim.Internals;
using System;

namespace AkkaSim.Public
{
    public interface ISimulationMessage
    {
        Guid Key { get; }
        object Message { get; }
        IActorRef Target { get; }
        ActorSelection TargetSelection { get; }
        Priority Priority { get; }
        int CompareTo(ISimulationMessage other);
    }
}