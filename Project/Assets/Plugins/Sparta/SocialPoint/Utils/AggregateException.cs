using System;
using System.Collections.Generic;
using System.Text;

namespace SocialPoint.Utils
{
    public sealed class AggregateException : Exception
    {
        const string _desc = "Multiple Exceptions thrown:";
        const string _indent = "    ";
        const string _countPrefix = ". ";
        const string _descPrefix = ": ";

        StringBuilder _msg;
        StringBuilder _prefix;

        void AddException(Exception ex, int lvl=0)
        {
            if(_prefix == null)
            {
                _prefix = new StringBuilder();
            }
            else
            {
                _prefix.Length = 0;
            }
            for(var i = 0; i < lvl; i++)
            {
                _prefix.Append(_indent);
            }
            _msg.Append(_prefix)
                .Append(ex.GetType().Name)
                .Append(_descPrefix)
                .AppendLine(ex.Message);
            var stack = ex.StackTrace;
            if(!string.IsNullOrEmpty(stack))
            {
                stack = stack.Replace(Environment.NewLine, Environment.NewLine+_prefix.ToString());
                _msg.Append(_prefix).AppendLine(stack);
            }
            if(ex.InnerException != null)
            {
                AddException(ex.InnerException, lvl+1);
            }
        }

        public Exception[] Exceptions { get; private set; }

        public override string Message
        {
            get
            {
                if(_msg == null)
                {
                    _msg = new StringBuilder();
                }
                else
                {
                    _msg.Length = 0;
                }
                _msg.AppendLine(_desc);
                var count = 1;
                for(var i=0; i<Exceptions.Length; i++)
                {
                    var ex = Exceptions[i];
                    _msg.Append(count++).Append(_countPrefix);
                    AddException(ex);
                }
                return _msg.ToString();
            }
        }

        public AggregateException(Exception[] exceptions)
        {
            Exceptions = exceptions;
        }

        public static bool ThrowOnTrigger = true;

        static AggregateException()
        {
            #if UNITY_5_3_OR_NEWER
            if(UnityEngine.Application.isPlaying)
            {
                ThrowOnTrigger = false;
            }
            #endif
        }

        public static void Trigger(List<Exception> exceptions)
        {
            Trigger(exceptions.ToArray());
        }

        public static void Trigger(Exception[] exceptions)
        {
            if(exceptions == null || exceptions.Length == 0)
            {
                return;
            }

            if(ThrowOnTrigger)
            {
                throw new AggregateException(exceptions);
            }
            else
            {
                for(var i = 0; i < exceptions.Length; i++)
                {
                    UnityEngine.Debug.LogException(exceptions[i]);
                }
            }
        }
    }
}
