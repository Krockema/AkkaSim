using Akka.Actor;
using Akka.Util.Internal;
using AkkaSim.Internals;
using AkkaSim.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using static AkkaSim.Public.SimulationMessage;

namespace AkkaSim
{
    public class SimulationContext : ReceiveActor, IWithUnboundedStash
    {
        private Dictionary<long, long> _FeaturedInstructions = new Dictionary<long, long>();

        private long _CurrentInstructions = 0;

        public HashSet<IActorRef> _allSimulants = new HashSet<IActorRef>();

        private bool _IsRunning = true;

        private bool _IsComplete = false;

        public IStash Stash { get; set; }

        private long TimePeriod { get; set; }

        public SimulationContext()
        {
            Receive<Command>(s => s == Command.Start, s =>
            {
                //Console.WriteLine("-- Starting Simulation -- !");
                Stash.UnstashAll();

                BecomeStacked(() =>
                {
                    Receive<Command>(c => c == Command.Done, c =>
                    {
                        _CurrentInstructions--;
                        Advance();
                    });

                    Receive<Command>(c => c == Command.Advance, c => Advance());

                    Receive<Command>(c => c == Command.Registration, c => _allSimulants.Add(Sender));

                    Receive<Command>(c => c == Command.DeRegistration, c => _allSimulants.Remove(Sender));

                    Receive<Schedule>(m =>
                    {
                        var sheduleAt = m.Delay + TimePeriod;
                        if (_FeaturedInstructions.Any(x => x.Key == sheduleAt))
                            _FeaturedInstructions[sheduleAt] = _FeaturedInstructions[sheduleAt] + 1;
                        else
                            { _FeaturedInstructions.Add(sheduleAt, 1); }

                        m.Message.Target.Forward(m);
                    });

                    Receive<ISimulationMessage>(m =>
                    {
                        if (m.Target != null)
                            m.Target.Forward(m);
                        else if (m.TargetSelection != null)
                            m.TargetSelection.Tell(m);
                        else
                            Sender.Tell(m);

                        _CurrentInstructions++;
                    });

                    Receive<Command>(c => c == Command.Stop, c =>
                    {
                        // Console.WriteLine("-- Resume simulation -- !");
                        Context.UnbecomeStacked();
                    });

                    ReceiveAny(_ => Stash.Stash());
                });
            });

            ReceiveAny(_ => Stash.Stash());
        }

        /// <summary>
        /// Constructor for Simulation context
        /// </summary>
        /// <returns>IActorRef of the SimulationContext</returns>
        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new SimulationContext());
        }


        private void Advance()
        {
            if (_IsRunning && !_IsComplete && _CurrentInstructions == 0)
            {
                if (_FeaturedInstructions.Count != 0)
                {
                    Advance(_FeaturedInstructions.Min(x => x.Key));
                } else {
                    _IsComplete = true;
                    Console.WriteLine("Simulation Finished...");
                    Context.Stop(Self);
                }
            }
        }

        private void Advance(long to)
        {
            if (_CurrentInstructions != 0) return;
            // advance time
            if (TimePeriod >= to)
                 throw new Exception("Time cant be undone.");
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
                _allSimulants.ForEach(x => x.Tell(tick));
            }
        }

        /// <summary>
        /// No Iedea if that will work out, even it is required.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IActorRef CreateChildNode<T>() where T : ActorBase, ISimulationElement , new()
        {
            return Context.CreateActor(() => (T)Activator.CreateInstance(typeof(T), new object[] { Context, TimePeriod }));
        }


    }
}
