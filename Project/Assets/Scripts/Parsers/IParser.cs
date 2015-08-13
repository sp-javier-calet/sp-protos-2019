using SocialPoint.Attributes;

public interface IParser<T>
{
    T Parse(Attr data);
}
