public class ModelException : System.Exception
{
    public ModelError Error { get; private set; }

    public ModelException(ModelError error) : base(error.ToString())
    {
        Error = error;
    }
}