using SocialPoint.Lockstep;
using SocialPoint.Utils;
using FixMath.NET;

public class ClickCommand : ILockstepCommand
{
    public event System.Action<ILockstepCommand, bool> Applied;
    public event System.Action<ILockstepCommand> Discarded;

    public int Turn { get; private set; }

    public int Retries { get; private set; }

    Fix64 _x;
    Fix64 _y;
    Fix64 _z;
    LockstepModel _model;

    public ClickCommand(Fix64 x, Fix64 y, Fix64 z, int turn, LockstepModel model)
    {
        _x = x;
        _y = y;
        _z = z;
        Turn = turn;
        _model = model;
    }

    public bool Apply()
    {
        var result = _model.OnClick(_x, _y, _z);
        if(Applied != null)
        {
            Applied(this, result);
        }
        return result;
    }

    public void Discard()
    {
        if(Discarded != null)
        {
            Discarded(this);
        }
    }

    public bool Retry(int turn)
    {
        ++Retries;
        return true;
    }

    public byte LockstepCommandDataType
    {
        get
        {
            throw new System.NotImplementedException();
        }
    }

    public bool Equals(ILockstepCommand other)
    {
        return true;
    }
}
