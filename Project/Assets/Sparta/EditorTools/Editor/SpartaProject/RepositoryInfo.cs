namespace SpartaTools.Editor.SpartaProject
{
    public class RepositoryInfo
    {
        public string Commit { get; private set; }

        public string User { get; private set; }

        public string Branch { get; private set; }

        public RepositoryInfo() : this("No commit", "No branch", "No user")
        {
        }

        public RepositoryInfo(string commit, string branch, string user)
        {
            Commit = commit;
            Branch = branch;
            User = user;
        }
    }
}
