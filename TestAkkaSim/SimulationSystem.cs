using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaSim;
using AkkaSim.Definitions;
using Xunit;
using Akka.TestKit.Xunit2;
using TestAkkaSim.Moc;

namespace TestAkkaSim
{
    public class SimulationSystem : TestKit
    {
        public static IEnumerable<object[]> GetTestData()
        {
            yield return new object[]{ SimulationCreator(debugMode: false, 0) };
            yield return new object[]{ SimulationCreator(debugMode: true, 0) };
        }

        public static IEnumerable<object[]> GetDelayTestData()
        {
            yield return new object[] { SimulationCreator(debugMode: false, 10) };
        }


        public static Simulation SimulationCreator(bool debugMode, long timeToAdvance)
        {
            SimulationConfig simConfig = new SimulationConfig(debugAkka: false
                , debugAkkaSim: debugMode
                , interruptInterval: 120
                , TimeSpan.FromMilliseconds(timeToAdvance));
            var sim = new Simulation(simConfig);
            // ActorMonitoringExtension.Monitors(sim.ActorSystem).IncrementActorCreated();
            // ActorMonitoringExtension.Monitors(sim.ActorSystem).IncrementMessagesReceived();
            return sim;
        }


        [Theory]
        [MemberData(nameof(GetTestData))]
        public void IsStarting(Simulation simulation)
        {
            Assert.True(simulation.IsReady());
            
            var task = simulation.RunAsync();
            Within(TimeSpan.FromSeconds(3), async () =>
            {
                await task;
                Assert.False(task.IsCompletedSuccessfully);
            });
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void MessageCurrentTime(Simulation simulation)
        {
            var ping = simulation.ActorSystem.ActorOf<PingBackAndForward>();
            var target = this.CreateTestProbe();

            var msg = new Ping(message: Ping.Name
                              , target: target);

            Within(TimeSpan.FromSeconds(3), () =>
            {
                simulation.SimulationContext.Tell(msg);

                target.FishForMessage(o => o.GetType() == typeof(Ping));

                Assert.Equal(TestActor, target.Sender);

            });
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void MessageScheduled(Simulation simulation)
        {
            var source = this.CreateTestProbe();
            var worker = simulation.ActorSystem.ActorOf(Props.Create(() 
                            => new ActingObject(simulation.SimulationContext, 0)));

            var msg = new Work(message: 10
                              , target: worker);

            Within(TimeSpan.FromSeconds(3), async () =>
            {
                simulation.SimulationContext.Tell(msg, source);
                await simulation.RunAsync();

                var work = source.FishForMessage(o => o.GetType() == typeof(Work)) as Work;
                
                Assert.Equal("Done", work.Message.ToString());
                Assert.Equal(TestActor, source.Sender);
            });
        }


        [Fact]
        public void IsStoppingAfterAtInterval()
        {
            Assert.True(true);
        }

        [Fact]
        public void IsRecurringAfterStop()
        {
            Assert.True(true);
        }

        [Fact]
        public void IsShutdown()
        {
            Assert.True(true);
        }
    }
}