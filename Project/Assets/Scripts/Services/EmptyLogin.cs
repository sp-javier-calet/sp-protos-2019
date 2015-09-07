using Zenject;
using SocialPoint.Login;
using SocialPoint.Hardware;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.Events;

class EmptyLogin : SocialPoint.Login.EmptyLogin
{   
    public EmptyLogin() : base(null)
    {
    }
}