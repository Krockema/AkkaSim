using System;
using System.Collections.Generic;
using AkkaSim;
using SimTest.Domain;
using SimTest.MachineQueue;
using ImmutableObjectLib;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SimTest
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Simulation world of Akka!");

            var sim = new Simulation(false);
            var r = new Random();

            Console.ReadKey();

            var jobDistributor = sim.ActorSystem.ActorOf(MachineJobDistributor.Props(sim.ActorSystem.EventStream, sim.SimulationContext, 0), "JobDistributor");
            // Create a message
            var createMachines = new MachineJobDistributor.AddMachine(null, jobDistributor);
            // Tell all Machines
            for (int i = 0; i < 10; i++)
            {
                sim.SimulationContext.Tell(createMachines, null);
            }
            
            for (int i = 0; i < 200; i++)
            {
                var materialRequest = new MaterialRequest(CreateBOM(), new Dictionary<int, bool>(), 0 , r.Next(50, 500), true);
                var request = new MachineJobDistributor.ProductionOrder(materialRequest, jobDistributor);
                sim.SimulationContext.Tell(request, null);
            }

            // example to monitor for FinishWork Messages.

            var monitor = sim.ActorSystem.ActorOf(props: Monitoring.WorkTimeMonitor
                                                                   .Props(time: 0), 
                                                   name: "SimulationMonitor");
            var stw = new Stopwatch();
            stw.Start();


            sim.RunAsync().Wait();

            Console.WriteLine("System Runtime "+ sim.ActorSystem.Uptime);
            Console.WriteLine("Final Call Finisch Done Forever Together And So On");
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

        public static void ThreeWaysToCloneImutables()
        {

            var msg = MessageFactory("t1", "s1", "abuse me");
            Console.WriteLine("Id before Copy " + msg.Key);

            msg = msg.UpdateDue(10);
            Console.WriteLine("Id after Copy  " + msg.Key);

            var msg2 = new WorkItem("test");
            Console.WriteLine("Id before Copy " + msg2.Key);

            // msg2 = new WorkItem(msg2, "test2");
            // Console.WriteLine("Id after Copy  " + msg2.Key);
            // 
            // msg2 = msg2.Clone().WorkItem = "";
            // Console.WriteLine("Id after Copy  " + msg2.Key);
        }

    }
}
