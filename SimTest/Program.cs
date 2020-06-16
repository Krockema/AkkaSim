using AkkaSim;
using AkkaSim.Definitions;
using AkkaSim.Logging;
using SimTest.Domain;
using SimTest.MachineQueue;
using System;
using System.Collections.Generic;
using Akka.Dispatch.SysMsg;
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
           
            RunSimulation();

            Console.ReadLine();
        }

        private static void RunSimulation()
        {
            LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Warn);
            LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_AGENTS, LogLevel.Info);
            LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_AKKA, LogLevel.Warn);
            LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AKKA, LogLevel.Trace);
            //InternalLogger.LogToConsole = true;
            //InternalLogger.LogLevel = LogLevel.Trace;

            SimulationConfig simConfig = new SimulationConfig(debugAkka: false
                , debugAkkaSim: true
                , interruptInterval: 120
                , timeToAdvance: TimeSpan.FromSeconds(0));
            var sim = new Simulation(simConfig);
            var r = new Random();

            Console.ReadKey();

            var jobDistributor =
                sim.ActorSystem.ActorOf(MachineJobDistributor.Props(sim.ActorSystem.EventStream, sim.SimulationContext, 0),
                    "JobDistributor");

            // Tell all Machines
            for (int i = 0; i < 3; i++)
            {
                // Create a message
                var createMachines = new MachineJobDistributor.AddMachine(null, jobDistributor);
                sim.SimulationContext.Tell(createMachines, null);
            }

            for (int i = 0; i < 300; i++)
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
                var terminated = sim.RunAsync();
                new StateManager().Continuation(simConfig.Inbox, sim);
                terminated.Wait();
            }

            Console.WriteLine("Systen is shutdown!");
            Console.WriteLine("System Runtime " + sim.ActorSystem.Uptime);
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
