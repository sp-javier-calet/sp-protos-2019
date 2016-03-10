using SocialPoint.ServerSync;
using SocialPoint.AppEvents;

class MessageCenter : SocialPoint.ServerMessaging.MessageCenter
{
    public MessageCenter(ICommandQueue commandQueue, CommandReceiver commandReceiver, IAppEvents appEvents) : base(commandQueue, commandReceiver, appEvents)
    {
        
    }
}


