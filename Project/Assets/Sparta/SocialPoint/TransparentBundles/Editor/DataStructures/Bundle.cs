namespace SocialPoint.TransparentBundles
{
    public class Bundle
    {
        public string Name = "";
        public float Size = 0f;
        public bool IsLocal = false;
        public Asset Asset;

        public Bundle(string name, Asset asset)
        {
            Name = name;
            Asset = asset;
        }

        public Bundle(string name, float size, bool isLocal, Asset asset)
        {
            Name = name;
            Size = size;
            IsLocal = isLocal;
            Asset = asset;
        }
    }
}