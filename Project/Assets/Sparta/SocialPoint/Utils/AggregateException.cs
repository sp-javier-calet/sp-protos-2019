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
                sb.Append(count++)
                    .Append(". ")
                    .Append(ex.GetType().Name)
                    .Append(": ")
                    .AppendLine(ex.Message);

                // InnerException stacktrace usually has more valuable data. Still, as it may be null sometimes, we should check.
                if(ex.InnerException != null)
                {
                    sb.AppendLine(ex.InnerException.StackTrace);
                }
                else
                {
                    sb.AppendLine(ex.StackTrace);
                }
                sb.AppendLine();
            }
            itr.Dispose();
            return sb.ToString();
        }

        public List<Exception> Exceptions { get; private set; }

        public AggregateException(IEnumerable<Exception> exceptions) : base(CreateMessage(exceptions))
        {
            Exceptions = new List<Exception>(exceptions);
        }
    }
}
