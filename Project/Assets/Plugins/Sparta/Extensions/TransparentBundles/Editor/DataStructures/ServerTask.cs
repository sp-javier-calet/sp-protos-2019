namespace SocialPoint.TransparentBundles
{
    public class ServerTask
    {
        public BundleOperation Operation;
        public int Id;
        public string AuthorMail;

        public ServerTask(BundleOperation operation, int id, string authorMail)
        {
            Operation = operation;
            Id = id;
            AuthorMail = authorMail;
        }
    }
}
