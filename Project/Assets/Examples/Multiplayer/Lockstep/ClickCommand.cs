using FixMath.NET;
using SocialPoint.FixedMath;
using SocialPoint.IO;
using SocialPoint.Lockstep;
using SocialPoint.NetworkModel;

namespace Examples.Multiplayer.Lockstep
{
    public class ClickCommand : ILockstepCommand
    {
        public ClickCommand()
        {
        }

        public ClickCommand(Fix64 x, Fix64 y, Fix64 z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Fix64 X { get; private set; }

        public Fix64 Y { get; private set; }

        public Fix64 Z { get; private set; }

        public object Clone()
        {
            return new ClickCommand(X, Y, Z);
        }

        public void Deserialize(IReader reader)
        {
            X = Fix64.FromRaw(reader.ReadInt64());
            Y = Fix64.FromRaw(reader.ReadInt64());
            Z = Fix64.FromRaw(reader.ReadInt64());
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(X.RawValue);
            writer.Write(Y.RawValue);
            writer.Write(Z.RawValue);
        }
    }

    public class ClickCommandLogic : ILockstepCommandLogic<ClickCommand>
    {
        readonly NetworkScene _scene;

        public ClickCommandLogic(NetworkScene scene)
        {
            _scene = scene;
        }

        public void Apply(ClickCommand cmd, byte playerNum)
        {
            var ngo = _scene.InstantiateObject(GameObjectType.Cube);
            ngo.Behaviours.Add<OwnerNetworkBehavior>().PlayerNumber = playerNum;
            ngo.Transform.Position = new Fix64Vector(cmd.X, cmd.Y, cmd.Z);
        }
    }
}