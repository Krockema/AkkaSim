using System;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaSim.Definitions;

namespace AkkaSim.SpecialActors
{
    /// <summary>
    /// A Time Monitor that regulates the Simulation Speed.
    /// </summary>
    public class HeartBeat :  ReceiveActor
    {
        private TimeSpan _timeToAdvance;
        public static Props Props(TimeSpan timeToAdvance)
        {
            return Akka.Actor.Props.Create(() => new HeartBeat(timeToAdvance));
        }
        public HeartBeat(TimeSpan timeToAdvance)
        {
            #region init
            _timeToAdvance = timeToAdvance;
            #endregion
            
            Receive<SimulationMessage.Command>(dl =>
                SendHeartBeat()
            );

            Receive<TimeSpan>(tta =>
                _timeToAdvance = tta
            );
        }

        private void SendHeartBeat()
        {
            Task.Delay(_timeToAdvance).Wait();
            Sender.Tell(SimulationMessage.Command.HeartBeat);
        }

    }
}

