﻿using Akka.Actor;
using AkkaSim.Definitions;
using AkkaSim.Interfaces;
using AkkaSim.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using AkkaSim.SpecialActors;
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
        private readonly Logger _logger = LogManager.GetLogger(TargetNames.LOG_AKKA);

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
        /// For Normal Mode it regulates Simulation Speed
        /// For DebugMode you can break at each Beat to check simulation System is not running empty, waiting or looping.
        /// </summary>
        private IActorRef Heart { get; set; }

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
            Heart = Context.ActorOf(HeartBeat.Props(config.TimeToAdvance));
            #endregion init

            if (config.DebugAkkaSim)
            {
                Become(DebugMode);
            } else
            {
                Become(NormalMode);
            }

        }

        #region Normal Behave
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
                    _nextInterupt += _SimulationConfig.InterruptInterval;
                    _SimulationConfig.Inbox.Receiver.Tell(SimulationState.Started);
                    Systole();
                    Advance();
                }
                else
                {
                    // Not Ready Yet, Try Again
                    Context.Self.Tell(s);
                }

            });

            Receive<Command>(s => s == Command.HeartBeat, s =>
            {
                Diastole();
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

                var scheduleAt = m.Delay + TimePeriod;

                if (_FeaturedInstructions.TryGetValue(scheduleAt, out long value))
                    _FeaturedInstructions[scheduleAt] = _FeaturedInstructions[scheduleAt] + 1;
                else
                { _FeaturedInstructions.Add(scheduleAt, 1); }
                
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

        /// <summary>
        /// Starts Heart Beat for normal mode that limits simulation Speed
        /// </summary>
        private void Systole()
        {
            if (_SimulationConfig.TimeToAdvance.Ticks == 0 || !_IsRunning) 
                return;
            Heart.Tell(Command.HeartBeat, Self);
            _CurrentInstructions++;
        }

        /// <summary>
        /// End of an Heart Beat cycle for normal 
        /// </summary>
        private void Diastole()
        {
            _CurrentInstructions--;
            Advance();
            Systole();
        }

        private void Advance()
        {
            if (_CurrentInstructions == 0 && _IsRunning && !_IsComplete)
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
        #endregion

        #region Debug
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
                    _nextInterupt = _nextInterupt + _SimulationConfig.InterruptInterval;
                    _SimulationConfig.Inbox.Receiver.Tell(SimulationState.Started);
                    Heart.Tell(Command.HeartBeat, Self);
                    Advance_Debug();
                }
                else
                {
                    // Not Ready Yet, Try Again
                    Context.Self.Tell(s);
                }
            });

            Receive<Command>(s => s == Command.HeartBeat,  s =>
            {
                Sender.Tell(Command.HeartBeat);
            });

            // Determine when The initialisation is Done.
            Receive<Command>(s => s == Command.IsReady, s =>
            {
                if (!_InstructionStore.Any())
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
                var msg = ((ISimulationMessage) c.Message);
                if (!_InstructionStore.Remove(msg.Key))
                    throw new Exception("Failed to remove message from Instruction store");
                //_logger.Log(LogLevel.Trace ,"| Time[{arg1}] | {arg2} | --Done | Messages Left {arg3} ", new object[] { TimePeriod, msg.Key, _InstructionStore.Count() });
                Advance_Debug();
            });

            Receive<Command>(c => c == Command.Stop, c =>
            {
                // Console.WriteLine("-- Resume simulation -- !");
                
                _logger.Info("Command Stop --Done {arg1} Stop", new object[] { _InstructionStore.Count() });
                _IsRunning = false;
            });

            Receive<Shutdown>(c =>
            {
                if (_InstructionStore.Any() && _FeatureStore.Any())
                //if (_CurrentInstructions == 0 && _FeaturedInstructions.Count() == 0)
                {
                    _logger.Log(LogLevel.Trace ,"Simulation Finished...");
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
                IActorRef target;
                if (m.Target != ActorRefs.NoSender)
                {
                    m.Target.Forward(m);
                    target = m.Target;

                }
                else if (m.TargetSelection != null)
                {
                    m.TargetSelection.Tell(m);
                    target = Sender;
                }
                else
                {
                    // ping back
                    Sender.Tell(m);
                    target = Sender;
                }
                //_CurrentInstructions++;
                _InstructionStore.Add(m.Key, m);
                _logger.Log(LogLevel.Trace ,"Time[{arg1}] | {arg2} | DO ++ | Instructions: {arg2} | Type: {arg3} | Sender: {arg4} | Target: {arg5}", new object[] { TimePeriod, m.Key , _InstructionStore.Count(), m.GetType().ToString(), Sender.Path.Name, target.Path.Name });
            });
        }

        private void Advance_Debug()
        {
            if (_IsRunning && !_IsComplete && _InstructionStore.Count == 0)
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
                _logger.Log( LogLevel.Debug, "Move To: {TimePeriod} | open: {arg}"
                    , new object[] { TimePeriod, _InstructionStore.Count() });
            }
        }
        #endregion

    }
}
