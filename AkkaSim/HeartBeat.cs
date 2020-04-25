using System;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaSim.Definitions;

namespace AkkaSim
{
    /// <summary>
    /// A Time Monitor that does perform an injected Action on TimeAdvance event.
    /// </summary>
    public class HeartBeat :  ReceiveActor
    {
        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new HeartBeat());
        }
        public HeartBeat()
        {
            #region init
            #endregion
            
            Receive<SimulationMessage.Command>(dl =>
                SendHeartBeat()
            );
        }

        private void SendHeartBeat()
        {
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            Sender.Tell(SimulationMessage.Command.HeartBeat);
        }

    }
}

