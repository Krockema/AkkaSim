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

namespace SimTest
{
    class Program
    {
        public static void Main(string[] args)
        {
            Run();
            //ThreeWaysToCloneImutables();

        }


        private static void Run()
        {
            Console.WriteLine("Simulation world of Akka!");

            SimulationConfig simConfig = new SimulationConfig(false, 480);
            var sim = new Simulation(simConfig);
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

            for (int i = 0; i < 3000; i++)
            {
                var materialRequest = new MaterialRequest(CreateBOM(), new Dictionary<int, bool>(), 0, r.Next(50, 500), true);
                var request = new MachineJobDistributor.ProductionOrder(materialRequest, jobDistributor);
                sim.SimulationContext.Tell(request, null);
            }

            // example to monitor for FinishWork Messages.

            var monitor = sim.ActorSystem.ActorOf(props: Monitoring.WorkTimeMonitor
                                                                   .Props(time: 0),
                                                   name: "SimulationMonitor");
            var stw = new Stopwatch();
            stw.Start();

            if (sim.IsReady())
            {
                sim.RunAsync();
                //Continuation(simConfig.Inbox, sim);
            }

            

            Console.WriteLine("Systen shutdown. . . ");
            Console.WriteLine("System Runtime " + sim.ActorSystem.Uptime);

            Console.ReadLine();
        }

        private static void Continuation(Inbox inbox, Simulation sim)
        {

            var something = inbox.ReceiveAsync().Result;
            switch (something)
            {
                case SimulationMessage.SimulationState.Started:
                    Continuation(inbox, sim);
                    break;
                case SimulationMessage.SimulationState.Stopped:
                    sim.Continue();
                    Continuation(inbox, sim);
                    break;
                case SimulationMessage.SimulationState.Finished:
                    sim.ActorSystem.Terminate();
                    break;
                default:
                    break;
            }
        }



        public static Message MessageFactory(string target, string source, object obj)
        {
            return new Message(key: Guid.NewGuid(),
                                target: target,
                                @object: obj,
                                list: new List<TestItem>(),
                                priority: ImmutableObjectLib.Priority.Medium);
        }
        public static Message MessageFactory(string target, string source, object obj, long due)
        {
            return new Message(key: Guid.NewGuid(),
                                target: target,
                                @object: obj,
                                list: new List<TestItem>(),
                                priority: ImmutableObjectLib.Priority.Medium);
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
            var listof = new List<Message>();
            listof.Add(MessageFactory("t1", "s1", "abuse me"));
            Console.WriteLine("Id before Copy " + listof.First().Key);



            var msg = listof.First();
            msg = msg.UpdatePriority(ImmutableObjectLib.Priority.High);
            Console.WriteLine("Var -> Id after Copy  " + msg.Key + " Prio :" + msg.Priority);
            Console.WriteLine("List -> Id after Copy  " + listof.First().Key + " Prio :" + listof.First().Priority);

            Console.WriteLine("List -> With Test Item : " + listof.First().List.Count());
            var testItem = new TestItem(Guid.NewGuid(), "Blabla");
            listof.First().List.Add(testItem);
            Console.WriteLine("List -> With Test Item : " + listof.First().List.Count());



            var msg2 = new TestItem(Guid.NewGuid(), "Before Copy");
            Console.WriteLine(msg2.Text + " " + msg2.Key);
            msg2 = msg2.Update("After Copy");
            Console.WriteLine(msg2.Text + " " + msg2.Key);
            Console.ReadLine();
            // msg2 = new WorkItem(msg2, "test2");
            // Console.WriteLine("Id after Copy  " + msg2.Key);
            // 
            // msg2 = msg2.Clone().WorkItem = "";
            // Console.WriteLine("Id after Copy  " + msg2.Key);
        }

    }
}
