namespace BehaviorDesigner.Runtime
{
    public interface IWriteSerializer<T>
    {
        /**
         * should serialize the complete newObject
         */
        void Serialize(T newObj, IWriter writer);
    }
}