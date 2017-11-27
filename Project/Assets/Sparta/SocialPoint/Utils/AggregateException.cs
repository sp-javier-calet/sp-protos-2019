using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPoint.Utils
{
    public sealed class AggregateException : Exception
    {
        static string CreateMessage(IEnumerable<Exception> exceptions)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Multiple Exceptions thrown:");
            var count = 1;
            var itr = exceptions.GetEnumerator();
            while(itr.MoveNext())
            {
                var ex = itr.Current;
                sb.Append(count++).Append(". ");
                AddException(sb, ex);
            }
            itr.Dispose();
            return sb.ToString();
        }

        static void AddException(StringBuilder sb, Exception ex, int lvl=0)
        {
            var prefix = new StringBuilder();
            for(var i = 0; i < lvl; i++)
            {
                prefix.Append("    ");
            }
            sb.Append(prefix)
                .Append(ex.GetType().Name)
                .Append(": ")
                .AppendLine(ex.Message);
            var stack = ex.StackTrace;
            if(!string.IsNullOrEmpty(stack))
            {
                stack = stack.Replace("\n", "\n"+prefix.ToString());
                sb.Append(prefix).AppendLine(stack);
            }
            if(ex.InnerException != null)
            {
                AddException(sb, ex.InnerException, lvl+1);
            }
        }

        public List<Exception> Exceptions { get; private set; }

        public AggregateException(IEnumerable<Exception> exceptions) : base(CreateMessage(exceptions))
        {
            Exceptions = new List<Exception>(exceptions);
        }
    }
}
