using Akka.Actor;
using System.Threading.Tasks;
using static AkkaSim.Public.SimulationMessage;

namespace AkkaSim
{
    public class Simulation
    {
        public ActorSystem ActorSystem { get; }
        public IActorRef SimulationContext { get; }
        public Simulation()
        {
            ActorSystem  = ActorSystem.Create("SimulationSystem");
            SimulationContext = ActorSystem.ActorOf<SimulationContext>("SimulationContext");
        }

        public Task Run()
        {
            return Task.Run(() =>
            {
                SimulationContext.Tell(Command.Start);
            });
        }
    }
}
