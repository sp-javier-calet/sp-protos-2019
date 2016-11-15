using SocialPoint.Lockstep;

namespace Examples.Lockstep
{
    public static class CommandType
    {
        public static byte Click = 1;

        public static void Setup(Model model, LockstepCommandFactory factory, LockstepClient client)
        {
            factory.Register<ClickCommand>(CommandType.Click);
            if(client != null)
            {                
                client.RegisterCommandLogic<ClickCommand>(new ClickCommandLogic(model));
            }
        }
    }


}