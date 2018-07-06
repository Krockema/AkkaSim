using Akka.Actor;
using AkkaSim.Interfaces;
using System;

namespace AkkaSim.Definitions
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