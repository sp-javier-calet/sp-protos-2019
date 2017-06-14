namespace BehaviorDesigner.Runtime
{
    public interface IReadParser<T>
    {
        /**
         * called when the whole object has to be read
         */
        T Parse(IReader reader);
    }
}