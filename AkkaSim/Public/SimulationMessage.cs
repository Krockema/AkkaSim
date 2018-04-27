using Akka.Actor;
using AkkaSim.Internals;
using System;


namespace AkkaSim.Public
{
    public class SimulationMessage: IComparable<ISimulationMessage>, ISimulationElement, ISimulationMessage
    {
        public enum Command
        {
            Start,
            Stop,
            Done,
            Advance,
            Finish,
            Registration,
            DeRegistration
        }

        public class Finish : SimulationMessage
        {
            public Finish(IActorRef parrent) 
                : base(target: parrent, message: null) { }

        };

        /// <summary>
        /// Message to Advance the local Clock time of each registred SimulationElement.
        /// </summary>
        public class AdvanceTo
        {
            public long TimePeriod { get; }
            public AdvanceTo(long time)
            {
                TimePeriod = time;
            }
        }

        /// <summary>
        /// Schedules a message to pop after delay
        /// </summary>
        public class Schedule
        {
            /// <summary>
            /// Amount of Timesteps the message should be delayed
            /// </summary>
            public long Delay { get; }
            public ISimulationMessage Message { get; }
            public Schedule(long delay, ISimulationMessage message)
            {
                Delay = delay;
                Message = message;
            }
        }

        /// <summary>
        /// General message envelope for all Messages in the System
        /// </summary>
        /// <param name="message"></param>
        /// <param name="target"></param>
        /// <param name="priority"></param>
        public SimulationMessage(object message, IActorRef target, Priority priority = Priority.Medium)
        {
            Key = Guid.NewGuid();
            Message = message;
            Target = target;
            Priority = priority;
        }

        /// <summary>
        /// Broadcast message by ActorSelection
        /// </summary>
        /// <param name="message"></param>
        /// <param name="targetSelection"></param>
        /// <param name="priority"></param>
        public SimulationMessage(object message, ActorSelection targetSelection, Priority priority = Priority.Medium)
        {
            Key = Guid.NewGuid();
            Message = message;
            TargetSelection = targetSelection;
            Priority = priority;
        }

        public Guid Key { get; }
        public object Message { get; }
        public IActorRef Target { get; }
        public Priority Priority { get; }

        /// <summary>
        /// Field to fill if Broadcast is Required.
        /// </summary>
        public ActorSelection TargetSelection { get; }
        /// <summary>
        /// Comparer for Priority Queue
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ISimulationMessage other)
        {
            if (this.Priority < other.Priority) return -1;
            else if (this.Priority > other.Priority) return 1;
            else return 0;
        }
    }

}
