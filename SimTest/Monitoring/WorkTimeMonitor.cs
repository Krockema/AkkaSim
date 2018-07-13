using Akka.Actor;
using Akka.Event;
using AkkaSim;
using SimTest.Domain;
using SimTest.Machine;
using System;

namespace SimTest.Monitoring
{
    public class WorkTimeMonitor : SimulationMonitor
    {
        public WorkTimeMonitor(long time) 
            : base(time, typeof(MachineAgent.FinishWork))
        {
        }

        protected override void EventHandle(object o)
        {
            // base.EventHandle(o);
            var m = o as MachineAgent.FinishWork;
            var material = m.Message as MaterialRequest;
            Console.WriteLine("Finished: " + material.Material.Name + " on Time: " + _Time);
        }

        public static Props Props(long time)
        {
            return Akka.Actor.Props.Create(() => new WorkTimeMonitor(time));
        }
    }
}
