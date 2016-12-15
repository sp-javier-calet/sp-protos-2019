namespace SocialPoint.TransparentBundles
{
    public class Bundle
    {
        public int Id = -1;
        public string Name = "";
        public int Version = -1;
        public float Size = 0f;
        public bool IsLocal = false;
        public Asset Asset;

        public Bundle(string name, Asset asset)
        {
            Id = -1;
            Name = name;
            Asset = asset;
        }

        public Bundle(int id, string name, int version, float size, bool isLocal, Asset asset)
        {
            Id = id;
            Name = name;
            Version = version;
            Size = size;
            IsLocal = isLocal;
            Asset = asset;
        }
    }
}