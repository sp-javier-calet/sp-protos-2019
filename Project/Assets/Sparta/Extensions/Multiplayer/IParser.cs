using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface IParser<T>
    {
        /**
         * called when the whole object has to be read
         */
        T Parse(IReader reader);

        /**
         * called when only the changes of the object have to be read
         * should read dirty bits first
         */
        T Parse(T oldObj, IReader reader);
    }        
}