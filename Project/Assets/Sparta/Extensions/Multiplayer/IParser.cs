using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface IParser<T>
    {
        T Parse(IReader reader);
        T Parse(T oldObj, IReader reader);
    }        
}