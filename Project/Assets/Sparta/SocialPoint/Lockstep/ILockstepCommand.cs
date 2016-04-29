using System;

namespace SocialPoint.Lockstep
{
    public interface ILockstepCommand : IEquatable<ILockstepCommand>
    {
        /// Execution turn of the command
        int Turn { get; }

        // Amount of retries applied to the current command
        int Retries { get; }

        // Called when the command is applied. The second parameter is the result of apply the command
        // (Commands can't be applied always)
        event Action<ILockstepCommand,bool> Applied;

        // Called when the command is not applied on time and the amount of retries is higher than the maximum allowed
        event Action<ILockstepCommand> Discarded;

        // Applies the command
        bool Apply();

        // Discards the command
        void Discard();

        // Called when the command is not processed and tries to be added as pending again
        bool Retry(int turn);

        // byte type of the command used when serializing it
        byte LockstepCommandDataType { get; }
    }

}