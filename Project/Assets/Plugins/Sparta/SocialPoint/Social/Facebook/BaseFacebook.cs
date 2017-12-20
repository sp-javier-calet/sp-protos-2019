using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public abstract class BaseFacebook : IFacebook
    {
        public bool InitializedFriends {get; set;}

        public event FacebookStateChangeDelegate StateChangeEvent;

        protected void NotifyStateChanged()
        {
            if(StateChangeEvent != null)
            {
                StateChangeEvent();
            }
        }

        protected void DoPostOnWall(FacebookWallPost post, Error err, FacebookWallPostDelegate cbk)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                PostOnWallFinish(post, err, cbk);
                return;
            }

            var graph = new FacebookGraphQuery(post.To + "/feed", FacebookGraphQuery.MethodType.POST);
            graph.AddParam("picture", post.Picture.AbsoluteUri);
            graph.AddParam("link", post.Link.AbsoluteUri);
            graph.AddParam("name", post.Name);
            graph.AddParam("caption", post.Caption);
            graph.AddParam("message", post.Message);
            graph.AddParam("description", post.Description);
            graph.AddParam("actions", post.GetActionsJson());

            QueryGraph(graph, (query, err2) => PostOnWallGraphQueryFinish(query, post, err2, cbk));
        }

        protected void PostOnWallGraphQueryFinish(FacebookGraphQuery query, FacebookWallPost post, Error err, FacebookWallPostDelegate cbk)
        {
            post.PostId = query.Params.GetValue("post_id").ToString();
            PostOnWallFinish(post, err, cbk);
        }

        protected void PostOnWallFinish(FacebookWallPost post, Error err, FacebookWallPostDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(post, err);
            }
        }

        public string GetFriendsIds(FacebookFriendFilterDelegate includeFriend = null)
        {
            string[] friendsIdsFiltered = Friends.Where(user => includeFriend == null || includeFriend(user)).Select(user => user.UserId).ToArray();
            return string.Join(",", friendsIdsFiltered);
        }

        public void PostOnWall(FacebookWallPost post, FacebookWallPostDelegate cbk = null)
        {
            DoPostOnWall(post, null, cbk);
        }

        public void AskForPermission(string permission, FacebookPermissionsDelegate cbk = null)
        {
            AskForPermissions(new List<string>{ permission }, cbk);
        }

        public abstract void SendAppRequest(FacebookAppRequest req, FacebookAppRequestDelegate cbk = null);

        public abstract void PostOnWallWithDialog(FacebookWallPost pub, FacebookWallPostDelegate cbk = null);

        public abstract void QueryGraph(FacebookGraphQuery req, FacebookGraphQueryDelegate cbk);

        public abstract void Login(ErrorDelegate cbk, bool withUi = true);

        public abstract void Logout(ErrorDelegate cbk);

        public abstract void LoadPhoto(string userId, FacebookPhotoDelegate cbk);

        public abstract bool HasPermissions(IList<string> permissions);

        public abstract void AskForPermissions(List<string> permissions, FacebookPermissionsDelegate cbk = null);

        public abstract void RefreshFriends(ErrorDelegate cbk = null);

        public abstract bool IsConnected { get; }

        public abstract bool IsConnecting { get; }

        public abstract string AppId { get; set; }

        public abstract string ApiVersion { get; set; }

        public abstract FacebookUser User { get; }

        public abstract List<FacebookUser> Friends { get; }

        public abstract List<string> LoginPermissions { get; }
    }
}
