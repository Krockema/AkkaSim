using Akka.Actor;
using AkkaSim.Interfaces;
using System;


namespace AkkaSim.Definitions
{
    /// <summary>
    /// Data Stuckture 
    /// </summary>
    public class SimulationMessage: IComparable<ISimulationMessage>, ISimulationElement, ISimulationMessage
    {
        /// <summary>
        /// Generic Message identifier
        /// </summary>
        public Guid Key { get; }
        /// <summary>
        /// !- Imutable -! Message Object
        /// </summary>
        public object Message { get; }
        /// <summary>
        /// Target Actor to whom the Simulation Messageshall be forwarded.
        /// </summary>
        public IActorRef Target { get; }
        /// <summary>
        /// Priority to Order msg
        /// </summary>
        public Priority Priority { get; }
        /// <summary>
        /// For simple and fast internal instructions
        /// </summary>
        internal enum Command
        {
            Start,
            Stop,
            Done,
            Finish
        }

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

        /// <summary>
        /// Agent Intern  finish current Actor and tell parrents
        /// </summary>
        internal class Finish : SimulationMessage
        {
            public Finish(IActorRef parrent) 
                : base(target: parrent, message: null) { }

        };

        /// <summary>
        /// Message to Advance the local clock time of each registred SimulationElement.
        /// </summary>
        internal class AdvanceTo
        {
            public long TimePeriod { get; }
            public AdvanceTo(long time)
            {
                TimePeriod = time;
            }
        }

        /// <summary>
        /// A Wrapper for messages to pop after given delay
        /// </summary>
        public class Schedule
        {
            /// <summary>
            /// Amount of Timesteps the message should be delayed
            /// </summary>
            public long Delay { get; }
            public ISimulationMessage Message { get; }
            internal Schedule(long delay, ISimulationMessage message)
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
        protected SimulationMessage(object message, IActorRef target, Priority priority = Priority.Medium)
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
        protected SimulationMessage(object message, ActorSelection targetSelection, Priority priority = Priority.Medium)
        {
            Key = Guid.NewGuid();
            Message = message;
            TargetSelection = targetSelection;
            Priority = priority;
        }
    }

}
