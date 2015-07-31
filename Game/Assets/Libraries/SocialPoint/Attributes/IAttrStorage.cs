
namespace SocialPoint.Attributes
{
    public interface IAttrStorage
    {
        Attr Load(string key);

        void Save(string key, Attr attr);

        void Remove(string key);

        bool Has(string key);
    }
}

