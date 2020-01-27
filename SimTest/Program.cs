using System;
using System.Collections.Generic;
using AkkaSim;
using SimTest.Domain;
using SimTest.MachineQueue;
using ImmutableObjectLib;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using AkkaSim.Definitions;
using Akka.Actor;
using Akka.Event;
using AkkaSim.Logging;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using LogLevel = NLog.LogLevel;

namespace SimTest
{
    class Program
    {
        public static void Main(string[] args)
        {
            Run();
        }
        private static void Run()
        {
            Console.WriteLine("Simulation world of Akka!");
           
            LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Warn);
            LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_AGENTS, LogLevel.Info);
            LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AKKA, LogLevel.Trace);
            //InternalLogger.LogToConsole = true;
            //InternalLogger.LogLevel = LogLevel.Trace;

            SimulationConfig simConfig = new SimulationConfig(debugAkka: false
                                                            , debugAkkaSim: true
                                                            , interruptInterval: 480);
            var sim = new Simulation(simConfig);
            var r = new Random();

            Console.ReadKey();

            var jobDistributor = sim.ActorSystem.ActorOf(MachineJobDistributor.Props(sim.ActorSystem.EventStream, sim.SimulationContext, 0), "JobDistributor");

            // Tell all Machines
            for (int i = 0; i < 3; i++)
            {
                // Create a message
                var createMachines = new MachineJobDistributor.AddMachine(null, jobDistributor);
                sim.SimulationContext.Tell(createMachines, null);
            }

            for (int i = 0; i < 3; i++)
            {
                var materialRequest = new MaterialRequest(CreateBOM(), new Dictionary<int, bool>(), 0, r.Next(50, 500), true);
                var request = new MachineJobDistributor.ProductionOrder(materialRequest, jobDistributor);
                sim.SimulationContext.Tell(request, null);
            }

            // example to monitor for FinishWork Messages.

            var monitor = sim.ActorSystem.ActorOf(props: Monitoring.WorkTimeMonitor
                                                                   .Props(time: 0),
                                                   name: "SimulationMonitor");

            if (sim.IsReady())
            {
                sim.RunAsync();
                new StateManager().Continuation(simConfig.Inbox, sim);
            }

            Console.WriteLine("Systen shutdown. . . ");
            Console.WriteLine("System Runtime " + sim.ActorSystem.Uptime);

            Console.ReadLine();
        }


        public static Material CreateBOM()
        {
            return new Material
            {
                Id = 1
                , Name = "Table"
                , AssemblyDuration = 5
                , Quantity = 1
                , Materials = new List<Material> { new Material { Id = 2, Name = "Leg", AssemblyDuration = 3, ParrentMaterialID = 1, Quantity = 4, IsReady = true } }
            };
        }
    }
}
