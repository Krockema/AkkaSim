using System;
using System.Collections.Generic;
using AkkaSim;
using SimTest.Domain;
using SimTest.MachineQueue;

namespace SimTest
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Simulation world of Akka!");

            var sim = new Simulation();
            var r = new Random();

            var jobDistributor = sim.ActorSystem.ActorOf(MachineJobDistributor.Props(sim.SimulationContext, 0), "JobDistributor");
            var createMachines = new MachineJobDistributor.AddMachine(null, jobDistributor);
            for (int i = 0; i < 10; i++)
            {
                sim.SimulationContext.Tell(createMachines, null);
            }
            
            for (int i = 0; i < 20; i++)
            {
                var materialRequest = new MaterialRequest(CreateBOM(), new Dictionary<int, bool>(),0 , r.Next(50, 500), true);
                var request = new MachineJobDistributor.ProductionOrder(materialRequest, jobDistributor);
                sim.SimulationContext.Tell(request, null);
            }
       

            sim.Run();

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
