using SocialPoint.Utils;

namespace SocialPoint.Lockstep.Network
{
    public class NetworkSetTurnAnticipationLockstepCommandDataFactory : IGenericFactory<TurnTypeTuple, NetworkLockstepCommandData>
    {
        ClientLockstepController _clientLockstep;

        public NetworkSetTurnAnticipationLockstepCommandDataFactory(ClientLockstepController clientLockstep)
        {
            _clientLockstep = clientLockstep;
        }

        public bool SupportsModel(TurnTypeTuple model)
        {
            return model.Type == NetworkLockstepCommandDataTypes.SetTurnAnticipation;
        }

        public NetworkLockstepCommandData Create(TurnTypeTuple model)
        {
            return new NetworkSetTurnAnticipationLockstepCommandData(model.Turn, _clientLockstep);
        }
    }
}