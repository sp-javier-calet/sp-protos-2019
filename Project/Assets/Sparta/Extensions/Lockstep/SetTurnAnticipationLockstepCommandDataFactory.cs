using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public class SetTurnAnticipationLockstepCommandDataFactory : IGenericFactory<TurnTypeTuple, LockstepCommandData>
    {
        ClientLockstepController _clientLockstep;

        public SetTurnAnticipationLockstepCommandDataFactory(ClientLockstepController clientLockstep)
        {
            _clientLockstep = clientLockstep;
        }

        public bool SupportsModel(TurnTypeTuple model)
        {
            return model.Type == LockstepCommandDataTypes.SetTurnAnticipation;
        }

        public LockstepCommandData Create(TurnTypeTuple model)
        {
            return new SetTurnAnticipationLockstepCommandData(model.Turn, _clientLockstep);
        }
    }
}