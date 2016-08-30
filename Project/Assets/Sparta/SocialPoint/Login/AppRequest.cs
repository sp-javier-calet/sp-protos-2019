using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Login
{
    public sealed class AppRequest
    {
        public List<User> Recipients { get; set; }

        public AttrDic Data { get; set; }

        public string Type { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public AppRequest()
        {
            Data = new AttrDic();
            Recipients = new List<User>();
        }

        public AppRequest(string type, AttrDic data = null) : this()
        {
            Type = type;
            if(data != null)
            {
                Data = data;
            }
        }

        public void AddRecipient(User user)
        {
            Recipients.Add(user);
        }
    }
}