using SocialPoint.Lockstep;
using SocialPoint.IO;
using FixMath.NET;

public class ClickCommand : ILockstepCommand
{
    public Fix64 X{ get; private set; }
    public Fix64 Y{ get; private set; }
    public Fix64 Z{ get; private set; }

    public ClickCommand()
    {
    }

    public ClickCommand(Fix64 x, Fix64 y, Fix64 z)
    {
        X = x;
        Y = y;
        Z = z;
    }
        
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
        writer.Write((long)X.RawValue);
        writer.Write((long)Y.RawValue);
        writer.Write((long)Z.RawValue);
    }

}

public class ClickCommandLogic : ILockstepCommandLogic<ClickCommand>
{
    LockstepModel _model;

    public ClickCommandLogic(LockstepModel model)
    {
        _model = model;
    }

    public void Apply(ClickCommand cmd)
    {
        _model.OnClick(cmd.X, cmd.Y, cmd.Z);
    }
}
