using Akka.Actor;
using Akka.Configuration;
using AkkaSim.Definitions;
using System.Threading.Tasks;
using static AkkaSim.Definitions.SimulationMessage;

namespace AkkaSim
{
    public class Simulation
    {

        public ActorSystem ActorSystem { get; }
        public IActorRef SimulationContext { get; }

        /// <summary>
        /// Prepare Simulation Environment
        /// </summary>
        /// <param name="debug">Enables AKKA-Global message Debugging</param>
        public Simulation(bool debug)
        {
            Config config = (debug) ? ConfigurationFactory.ParseString(GetConfiguration()) 
                         /* else */ : ConfigurationFactory.Load();

            ActorSystem  = ActorSystem.Create("SimulationSystem", config);
            SimulationContext = ActorSystem.ActorOf(Props.Create(() => new SimulationContext(ActorSystem.EventStream)),  "SimulationContext");
        }

        public void Run()
        {
            //return Task.Run(() =>
            //{
                SimulationContext.Tell(Command.Start);
            //});
        }

        /// <summary>
        /// should later read from app.config or dynamicly created
        /// </summary>
        /// <returns></returns>
        private string GetConfiguration()
        {
            return @"  akka {
                        stdout-loglevel = DEBUG
                        loglevel = DEBUG
                        log-config-on-start = on
                        actor {
                            debug {
                                receive = on
                                autoreceive = on
                                lifecycle = on
                                event-stream = on
                                unhandled = on
                                }
                            }";
        }
    }
}
