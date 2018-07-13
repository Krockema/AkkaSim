using Akka.Actor;
using AkkaSim;
using SimTest.Domain;
using AkkaSim.Definitions;
using System.Collections.Generic;
using SimTest.Machine;
using System.Linq;
using System;
using Akka.Event;

namespace SimTest.MachineQueue
{
    partial class MachineJobDistributor : SimulationElement
    {
        private int MaterialCounter = 0;
        public Dictionary<IActorRef, bool> Machines { get; set; } = new Dictionary<IActorRef, bool>();

        public PriorityQueue<MaterialRequest> ReadyItems { get; set; } = new PriorityQueue<MaterialRequest>();

        public HashSet<MaterialRequest> WaitingItems { get; set; } = new HashSet<MaterialRequest>();

        public static Props Props(EventStream eventStream, IActorRef simulationContext, long time)
        {
            return Akka.Actor.Props.Create(() => new MachineJobDistributor(simulationContext, time));
        }
        public MachineJobDistributor(IActorRef simulationContext, long time) 
            : base(simulationContext, time)
        {
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case ProductionOrder m  : MaterialRequest(m); break;
                case Command.GetWork    : PushWork(); break;
                case AddMachine m       : CreateMachines(Machines.Count + 1, TimePeriod); break;
                case ProductionOrderFinished m: ProvideMaterial(m); break;
                default: new Exception("Message type could not be handled by SimulationElement"); break;
            }
        }

        /// <summary>
        /// solve Tree
        /// </summary>
        /// <param name="request"></param>
        private void MaterialRequest(object o)
        {
            var p = o as ProductionOrder;
            var request = p.Message as MaterialRequest;
            if (request.Material.Materials != null)
            {
                foreach (var child in request.Material.Materials)
                {
                    for (int i = 0; i < child.Quantity; i++)
                    {
                        var childRequest = new MaterialRequest(material: child,
                                                          childRequests: null,
                                                                parrent: request.Id,
                                                                    due: request.Due - request.Material.AssemblyDuration - child.AssemblyDuration,
                                                                 isHead: false);
                        request.ChildRequests.Add(childRequest.Id, false);
                        var po = new ProductionOrder(childRequest, Self);
                        _SimulationContext.Tell(po, Self);
                    }
                }
            }
            if (request.Material.IsReady)
                ReadyItems.Enqueue(request);
             else
                WaitingItems.Add(request);

            PushWork();
        }

        private void PushWork()
        {
            if (Machines.ContainsValue(true) && ReadyItems.Count() != 0)
            {
                var key = Machines.First(X => X.Value == true).Key;
                Machines.Remove(key);
                var m = new MachineAgent.Work(ReadyItems.Dequeue(), key);
                Machines.Add(key, false);
                _SimulationContext.Tell(m, Sender);
            };
        }

        private void CreateMachines(int machineNumber, long time)
        {
            Machines.Add(Context.ActorOf(MachineAgent.Props(_SimulationContext, time), "Maschine_" + machineNumber), true);
        }

        private void ProvideMaterial(object o)
        {
            var po = o as ProductionOrderFinished;
            var request = po.Message as MaterialRequest;
            if (request.Material.Name == "Table")
                MaterialCounter++;
            //Console.WriteLine("Time: " + TimePeriod + " Number " + MaterialCounter + " Finished: " + request.Material.Name);
            if (!request.IsHead)
            {
                var parrent = WaitingItems.Single(x => x.Id == request.Parrent);
                parrent.ChildRequests[request.Id] = true;
                
                // now check if item can be deployd to ReadyQueue
                if (parrent.ChildRequests.All(x => x.Value == true))
                {
                    WaitingItems.Remove(parrent);
                    ReadyItems.Enqueue(parrent);
                }
            }
            Machines.Remove(Sender);
            Machines.Add(Sender, true);

            PushWork();
        }

        protected override void Finish()
        {
            Console.WriteLine(Sender.Path + " has been Killed");
        }
    }
}
