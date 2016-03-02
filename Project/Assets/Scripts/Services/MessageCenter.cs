using SocialPoint.ServerSync;

class MessageCenter : SocialPoint.ServerMessaging.MessageCenter
{
    public MessageCenter(ICommandQueue commandQueue, CommandReceiver commandReceiver) : base(commandQueue, commandReceiver)
    {
        
    }
}


