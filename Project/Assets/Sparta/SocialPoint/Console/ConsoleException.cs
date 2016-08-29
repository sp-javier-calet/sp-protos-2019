using System;

namespace SocialPoint.Console
{
    public sealed class ConsoleException : Exception
    {
        public ConsoleException(string what): base(what)
        {
        }
    }
}

