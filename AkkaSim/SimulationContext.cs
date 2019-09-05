using Akka.Actor;
using AkkaSim.Definitions;
using AkkaSim.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AkkaSim.Definitions.SimulationMessage;

namespace AkkaSim
{
    public class SimulationContext : ReceiveActor
    {
        // for Normal Mode
        private Dictionary<long, long> _FeaturedInstructions = new Dictionary<long, long>();
        private long _CurrentInstructions = 0;

        // for Debugging Mode
        public Dictionary<Guid, ISimulationMessage> _InstructionStore = new Dictionary<Guid, ISimulationMessage>();
        public Dictionary<long, Dictionary<Guid, ISimulationMessage>> _FeatureStore = new Dictionary<long, Dictionary<Guid, ISimulationMessage>>();

        private SimulationConfig _SimulationConfig { get; }

        /// <summary>
        /// Contains the next interval time where the simulation will stop.
        /// </summary>
        private long _nextInterupt { get; set; } = 0;

        /// <summary>
        /// Set to true when the Simulation hase no events in Queue or has recieved a SimulationState.Finish message
        /// </summary>
        private bool _IsComplete { get; set; } = false;
        /// <summary>
        /// Set to false when the Simulation recieved a Command.Stop, time will no longer advance.
        /// </summary>
        private bool _IsRunning { get; set; } = false;

        /// <summary>
        /// Contains the current simulation Time
        /// </summary>
        private long TimePeriod { get; set; }

        /// <summary>
        /// Probe Constructor for Simulation context
        /// </summary>
        /// <returns>IActorRef of the SimulationContext</returns>
        public static Props Props(SimulationConfig config)
        {
            return Akka.Actor.Props.Create(() => new SimulationContext(config));
        }

        public SimulationContext(SimulationConfig config)
        {
            #region init

            _SimulationConfig = config;

            #endregion init

            if (config.Debug == true)
            {
                Become(DebugMode);
            } else
            {
                Become(NormalMode);
            }

        }

        /// <summary>
        /// Debuging Mode Stores the Messages and enables Message Tracking for the Simulation Run.
        /// </summary>
        private void DebugMode()
        {

            Receive<Command>(s => s == Command.Start, s =>
            {
                if (!_InstructionStore.Any())
                //if (_CurrentInstructions == 0)
                {
                    _IsRunning = true;
                    _nextInterupt = _nextInterupt + _SimulationConfig.InteruptInterval;
                    _SimulationConfig.Inbox.Receiver.Tell(SimulationState.Started);
                    
                    Advance_Debug();
                }
                else
                {
                    // Not Ready Yet, Try Again
                    Context.Self.Tell(s);
                }
            });

            // Determine when The initialisation is Done.
            Receive<Command>(s => s == Command.IsReady, s =>
            {
                if (_InstructionStore.Any())
                //if (_CurrentInstructions == 0)
                {
                    Sender.Tell(Command.IsReady, ActorRefs.NoSender);
                }
                else
                {
                    // Not Ready Yet, Try Again
                    Context.Self.Forward(s);
                }
            });

            // Determine when The Simulation is Done.
            Receive<SimulationState>(s => s == SimulationState.Finished, s =>
            {
                if (_InstructionStore.Any())
                {
                    _SimulationConfig.Inbox.Receiver.Tell(SimulationState.Finished);
                    _IsComplete = true;
                }
                else
                {
                    // Not Ready Yet, Try Again
                    Context.Self.Forward(s);
                }
            });

            Receive<Done>(c =>
            {
                if (!_InstructionStore.Remove(((ISimulationMessage)c.Message).Key)) throw new Exception("Failed to remove message from Instruction store");
                System.Diagnostics.Debug.WriteLine("-- Done(" + _InstructionStore.Count() + ") ", "AKKA");
                Advance_Debug();
            });

            Receive<Command>(c => c == Command.Stop, c =>
            {
                // Console.WriteLine("-- Resume simulation -- !");
                System.Diagnostics.Debug.WriteLine("STOP", "AKKA");
                _IsRunning = false;
            });

            Receive<Shutdown>(c =>
            {
                if (_InstructionStore.Any() && _FeatureStore.Any())
                //if (_CurrentInstructions == 0 && _FeaturedInstructions.Count() == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Simulation Finished...", "AKKA");
                    CoordinatedShutdown.Get(Context.System).Run(CoordinatedShutdown.ClrExitReason.Instance);
                }
            });

            Receive<Schedule>(m =>
            {
                LogInterceptor(m.Message);
                var sheduleAt = m.Delay + TimePeriod;
                if (_FeatureStore.TryGetValue(sheduleAt, out Dictionary<Guid, ISimulationMessage> value))
                {
                    value.Add(m.Message.Key, m.Message);
                }
                else
                { _FeatureStore.Add(sheduleAt, new Dictionary<Guid, ISimulationMessage> { { m.Message.Key, m.Message } }); }

                m.Message.Target.Forward(m);
            });

            Receive<ISimulationMessage>(m =>
            {
                LogInterceptor(m);
                if (m.Target != ActorRefs.NoSender)
                {
                    m.Target.Forward(m);

                }
                else if (m.TargetSelection != null)
                {
                    m.TargetSelection.Tell(m);
                }
                else
                {
                    // ping back
                    Sender.Tell(m);
                }
                //_CurrentInstructions++;
                _InstructionStore.Add(m.Key, m);
                System.Diagnostics.Debug.WriteLine(" DO ++ (" + _InstructionStore.Count() + ")" + m.GetType().ToString(), "AKKA");
            });
        }

        /// <summary>
        /// Enables Message logging for explicit Messages
        /// </summary>
        /// <param name="message"></param>
        private void LogInterceptor(ISimulationMessage message)
        {
            if (message.LogThis)
            {
                Context.System.EventStream.Publish(message);
            }
        }

        /// <summary>
        /// Does not track Simulation Messages, only the amount that has to be processed
        /// </summary>
        private void NormalMode()
        {

            Receive<Command>(s => s == Command.Start, s =>
            {
                if (_CurrentInstructions == 0)
                {
                    _IsRunning = true;
                    _nextInterupt = _nextInterupt + _SimulationConfig.InteruptInterval;
                    _SimulationConfig.Inbox.Receiver.Tell(SimulationState.Started);
                    Advance();
                }
                else
                {
                    // Not Ready Yet, Try Again
                    Context.Self.Tell(s);
                }

            });

            // Determine when The Simulation is Done.
            Receive<Command>(s => s == Command.IsReady, s =>
            {
                //if (_InstructionStore.Count() == 0)
                if (_CurrentInstructions == 0)
                {
                    Sender.Tell(Command.IsReady, ActorRefs.NoSender);
                }
                else
                {
                    // Not Ready Yet, Try Again
                    Context.Self.Forward(s);
                }
            });

            // Determine when The Simulation is Done.
            Receive<SimulationState>(s => s == SimulationState.Finished, s =>
            {
                //if (_InstructionStore.Count() == 0)
                if (_CurrentInstructions == 0)
                {
                    _SimulationConfig.Inbox.Receiver.Tell(SimulationState.Finished);
                    _IsComplete = true;
                }
                else
                {
                    // Not Ready Yet, Try Again
                    Context.Self.Forward(s);
                }
            });

            Receive<Done>(c =>
            {
                //Console.WriteLine("--");
                _CurrentInstructions--;
                Advance();
            });

            Receive<Command>(c => c == Command.Stop, c =>
            {
                // Console.WriteLine("-- Resume simulation -- !");
                _IsRunning = false;
            });

            Receive<Shutdown>(c =>
            {
                if (_CurrentInstructions == 0 && _FeaturedInstructions.Count() == 0)
                {
                    CoordinatedShutdown.Get(Context.System)
                                       .Run(CoordinatedShutdown.ClrExitReason.Instance);
                }
            });

            Receive<Schedule>(m =>
            {
                LogInterceptor(m.Message);

                var sheduleAt = m.Delay + TimePeriod;

                if (_FeaturedInstructions.TryGetValue(sheduleAt, out long value))
                    _FeaturedInstructions[sheduleAt] = _FeaturedInstructions[sheduleAt] + 1;
                else
                    { _FeaturedInstructions.Add(sheduleAt, 1); }
                
                m.Message.Target.Forward(m);
            });

            Receive<ISimulationMessage>(m =>
            {
                LogInterceptor(m);

                if (m.Target != ActorRefs.NoSender)
                {
                    m.Target.Forward(m);

                }
                else if (m.TargetSelection != null)
                {
                    m.TargetSelection.Tell(m);
                }
                else
                {
                    Sender.Tell(m);
                }
                //Console.WriteLine("++");
                _CurrentInstructions++;
            });
        }

        
        private void Advance()
        {
            if (_IsRunning && !_IsComplete && _CurrentInstructions == 0)
            {
                if (_FeaturedInstructions.Count != 0)
                {
                    Advance(_FeaturedInstructions.Min(x => x.Key));
                }
                else
                {
                    _IsComplete = true;
                    _SimulationConfig.Inbox.Receiver.Tell(SimulationState.Finished);
                }
            }
        }

        private void Advance_Debug()
        {
            if (_IsRunning && !_IsComplete && _InstructionStore.Count() == 0)
            {
                if (_FeatureStore.Count != 0)
                {
                    Advance_Debug(_FeatureStore.Min(x => x.Key));
                } else {
                    _IsComplete = true;
                    _SimulationConfig.Inbox.Receiver.Tell(SimulationState.Finished);
                }
            }
        }


        private void Advance_Debug(long to)
        {
            if (TimePeriod >= to)
                throw new Exception("Time cant be undone.");
            TimePeriod = to;
            // get current Tasks
            MoveFeaturesToCurrentTimeSpan_Debug();

        }

        private void Advance(long to)
        {
            // advance time
            if (TimePeriod >= to)
                 throw new Exception("Time cant be undone.");
            // stop sim at Interval
            if (TimePeriod >= _nextInterupt)
            {
                _IsRunning = false;
                _SimulationConfig.Inbox.Receiver.Tell(SimulationState.Stopped);
                return;
            }
            TimePeriod = to;
            // get current Tasks
            MoveFeaturesToCurrentTimeSpan();
            
        }

        private void MoveFeaturesToCurrentTimeSpan_Debug()
        {
            if (_FeatureStore.TryGetValue(TimePeriod, out _InstructionStore))
            {
                // stop sim at Interval
                if (TimePeriod >= _nextInterupt)
                {
                    _IsRunning = false;
                    _SimulationConfig.Inbox.Receiver.Tell(SimulationState.Stopped);
                }

                _FeatureStore.Remove(TimePeriod);
                // global Tick
                var tick = new AdvanceTo(TimePeriod);
                Context.System.EventStream.Publish(tick);
                System.Diagnostics.Debug.WriteLine("Move To: " + TimePeriod + " open " + _InstructionStore.Count(), "AKKA");
            }
        }

        private void MoveFeaturesToCurrentTimeSpan()
        {

            if (_FeaturedInstructions.TryGetValue(TimePeriod, out _CurrentInstructions))
            {
                _FeaturedInstructions.Remove(TimePeriod);
                // global Tick
                var tick = new AdvanceTo(TimePeriod);
                Context.System.EventStream.Publish(tick);
            }
        }
    }
}
