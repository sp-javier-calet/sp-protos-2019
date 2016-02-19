using System;

namespace SocialPoint.Console
{
    public class ConsoleException : Exception
    {
        public ConsoleException(string what): base(what)
        {
        }
    }
}

