// -----------------------------------------------------------------------
// <copyright file="CounterLogger.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server.Diagnostics
{
    using ExitGames.Logging;

    public class CounterLogger
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The print counter.
        /// </summary>
        public static void PrintCounter()
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    "peers: {0}, r-ev: {1}, ur-ev: {2}, r-op: {3}, ur-op: {4}, res: {5}, r-rtt: {6}, ur-rtt: {7}, rtt: {8}, var: {9}, flsh-rtt: {10}, flsh-ev {11}, flsh-op {12}",
                    Counters.ConnectedClients.GetNextValue(),
                    Counters.ReliableEventsReceived.GetNextValue(),
                    Counters.UnreliableEventsReceived.GetNextValue(),
                    Counters.ReliableOperationsSent.GetNextValue(),
                    Counters.UnreliableOperationsSent.GetNextValue(),
                    Counters.ReceivedOperationResponse.GetNextValue(),
                    Counters.ReliableEventRoundTripTime.GetNextValue(),
                    Counters.UnreliableEventRoundTripTime.GetNextValue(),
                    Counters.RoundTripTime.GetNextValue(),
                    Counters.RoundTripTimeVariance.GetNextValue(),
                    Counters.FlushEventRoundTripTime.GetNextValue(),
                    Counters.FlushEventsReceived.GetNextValue(),
                    Counters.FlushOperationsSent.GetNextValue());
            }
            else
            {
                // reset average counters
                Counters.ConnectedClients.GetNextValue();
                Counters.ReliableEventsReceived.GetNextValue();
                Counters.UnreliableEventsReceived.GetNextValue();
                Counters.ReliableOperationsSent.GetNextValue();
                Counters.UnreliableOperationsSent.GetNextValue();
                Counters.ReceivedOperationResponse.GetNextValue();
                Counters.ReliableEventRoundTripTime.GetNextValue();
                Counters.UnreliableEventRoundTripTime.GetNextValue();
                Counters.RoundTripTime.GetNextValue();
                Counters.RoundTripTimeVariance.GetNextValue();
                Counters.FlushEventRoundTripTime.GetNextValue();
                Counters.FlushEventsReceived.GetNextValue();
                Counters.FlushOperationsSent.GetNextValue();
            }
        }
    }
}
