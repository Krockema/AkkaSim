using System;
using System.Collections.Generic;
using AkkaSim;
using SimTest.Domain;
using SimTest.MachineQueue;
using SimTest.Monitoring;
using ImmutableObjectLib;
using AkkaSim.Definitions;

namespace SimTest
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Simulation world of Akka!");

            var sim = new Simulation(false);
            var r = new Random();


            var msg = MessageFactory("t1", "s1", "abuse me");
            msg = msg.UpdateDue(10);


            var jobDistributor = sim.ActorSystem.ActorOf(MachineJobDistributor.Props(sim.ActorSystem.EventStream, sim.SimulationContext, 0), "JobDistributor");
            // Create a message
            var createMachines = new MachineJobDistributor.AddMachine(null, jobDistributor);
            // Tell all Machines
            for (int i = 0; i < 200; i++)
            {
                sim.SimulationContext.Tell(createMachines, null);
            }
            
            for (int i = 0; i < 20000; i++)
            {
                var materialRequest = new MaterialRequest(CreateBOM(), new Dictionary<int, bool>(), 0 , r.Next(50, 500), true);
                var request = new MachineJobDistributor.ProductionOrder(materialRequest, jobDistributor);
                sim.SimulationContext.Tell(request, null);
            }

            // example to monitor for FinishWork Messages.

            // var monitor = sim.ActorSystem.ActorOf(props: WorkTimeMonitor
            //                                              .Props(eventStream: sim.ActorSystem.EventStream,
            //                                                            time: 0), 
            //                                       name: "SimulationMonitor");
            
            sim.Run();
            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        public static Message MessageFactory(string target, string source, object obj)
        {
            return new Message(key: Guid.NewGuid(),
                                target: target,
                                source: source,
                                @object: obj,
                                due: 0,
                                priority: 1);
        }
        public static Message MessageFactory(string target, string source, object obj, long due)
        {
            return new Message(key: Guid.NewGuid(),
                                target: target,
                                source: source,
                                @object: obj,
                                due: due,
                                priority: 1);
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
