using Akka.Actor;
using AkkaSim.Definitions;
using System;

namespace AkkaSim.Interfaces
{
    public interface ISimulationMessage
    {
        Guid Key { get; }
        object Message { get; }
        IActorRef Target { get; }
        ActorSelection TargetSelection { get; }
        Priority Priority { get; }
        bool LogThis { get;  }
        int CompareTo(ISimulationMessage other);
    }
}