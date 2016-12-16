namespace SocialPoint.TransparentBundles
{
    public class Bundle
    {
        public int Id = -1;
        public string Name = "";
        public float Size = 0f;
        public bool IsLocal = false;
        public Asset Asset;

        public Bundle(string name, Asset asset)
        {
            Id = -1;
            Name = name;
            Asset = asset;
        }

        public Bundle(int id, string name, float size, bool isLocal, Asset asset)
        {
            Id = id;
            Name = name;
            Size = size;
            IsLocal = isLocal;
            Asset = asset;
        }
    }
}