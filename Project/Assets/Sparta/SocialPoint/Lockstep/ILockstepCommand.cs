using System;

namespace SocialPoint.Lockstep
{
    public interface ILockstepCommand
    {
        int Turn { get; }

        int Retries { get; }

        event Action<ILockstepCommand,bool> Applied;
    
        event Action<ILockstepCommand> Discarded;

        bool Apply();

        void Discard();

        bool Retry(int turn);

        byte LockstepCommandDataType { get; }
    }
}