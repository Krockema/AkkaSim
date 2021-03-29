using Akka.Actor;
using AkkaSim.Definitions;
using AkkaSim.Interfaces;
using AkkaSim.Logging;
using NLog;
using System;
using System.Collections.Generic;
using AkkaSim.Definitions.Instructions;
using AkkaSim.SpecialActors;
using static AkkaSim.Definitions.SimulationMessage;

namespace AkkaSim
{
    public class SimulationContext : ReceiveActor
    {
        // for Normal Mode
        private IFeatureInstructions _featuredInstructions;
        private ICurrentInstructions _currentInstructions;
        
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
        private TimeSpan Time { get; }
        private DateTime DateTime { get; }
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
            _featuredInstructions = InstructionStoreFactory.CreateFeatureStore(config.DebugAkkaSim);
            _currentInstructions = InstructionStoreFactory.CreateCurrent(config.DebugAkkaSim);
            Heart = Context.ActorOf(HeartBeat.Props(config.TimeToAdvance));
            #endregion init



            Become(NormalMode);
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
                if (_currentInstructions.Count() == 0)
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
                if (_currentInstructions.Count() == 0)
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
                if (_currentInstructions.Count() == 0)
                {
                    _SimulationConfig.Inbox.Receiver.Tell(SimulationState.Finished);
                    _IsComplete = true;
                    _logger.Log(LogLevel.Trace ,"Simulation Finished...");
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
                if (!_currentInstructions.Remove(msg: msg.Key))
                    throw new Exception("Failed to remove message from Instruction store");
                Advance();
            });

            Receive<Command>(c => c == Command.Stop, c =>
            {
                // Console.WriteLine("-- Resume simulation -- !");
                _logger.Info("Command Stop --Done {arg1} Stop", new object[] { _currentInstructions.Count() });
                _IsRunning = false;
            });

            Receive<Shutdown>(c =>
            {
                if (_currentInstructions.Count() == 0 && _featuredInstructions.Count() == 0)
                {
                    CoordinatedShutdown.Get(Context.System)
                                       .Run(CoordinatedShutdown.ClrExitReason.Instance);
                }
            });

            Receive<Schedule>(m =>
            {
                LogInterceptor(m.Message);

                var scheduleAt = m.Delay + TimePeriod;

                if (_featuredInstructions.TryGetValue(scheduleAt, out ICurrentInstructions instructions))
                    instructions.Add(m.Message.Key, m.Message);
                else
                {
                    instructions = InstructionStoreFactory.CreateCurrent(_SimulationConfig.DebugAkkaSim);
                    instructions.Add(m.Message.Key, m.Message);
                    _featuredInstructions.Add(scheduleAt, instructions);
                }
                
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
                _currentInstructions.Add(m.Key, m);
                _logger.Log(LogLevel.Trace ,"Time[{arg1}] | {arg2} | DO ++ | Instructions: {arg2} | Type: {arg3} | Sender: {arg4} | Target: {arg5}"
                    , new object[] { TimePeriod, m.Key , _currentInstructions.Count(), m.GetType().ToString(), Sender.Path.Name, m.Target.Path.Name });
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
            _currentInstructions.WaitForDiastole(true);
        }

        /// <summary>
        /// End of an Heart Beat cycle for normal 
        /// </summary>
        private void Diastole()
        {
            _currentInstructions.WaitForDiastole(false);
            Advance();
            Systole();
        }

        private void Advance()
        {
            if (_currentInstructions.Count() == 0 && _IsRunning && !_IsComplete)
            {
                if (_featuredInstructions.Count() != 0)
                {
                    Advance(_featuredInstructions.Next());
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

            if (_featuredInstructions.TryGetValue(TimePeriod, out _currentInstructions))
            {
                _featuredInstructions.Remove(TimePeriod);
                // global Tick
                var tick = new AdvanceTo(TimePeriod);
                Context.System.EventStream.Publish(tick);
            }
        }
    }
}
