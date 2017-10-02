using System;
using System.Text;

namespace SpartaTools.Editor.Utils
{
    public class Repository
    {
        const string Binary = "git";

        public static class Format
        {
            public const string Author = "%aN";
            public const string Mail = "%ae";
            public const string Date = "%ad";
            public const string Commit = "%H";
            public const string Subject = "%s";
            public const string Message = "%b";
            public const string NewLine = "%n";
        }

        #region Git queries

        public class Query
        {
            readonly string _path;
            readonly string _command;
            string _limitString = string.Empty;
            string _formatString = string.Empty;
            string _error = string.Empty;
            StringBuilder _options = new StringBuilder();
            StringBuilder _arguments = new StringBuilder();

            public bool HasError
            {
                get
                {
                    return !string.IsNullOrEmpty(_error);
                }
            }

            public string Error
            {
                get
                {
                    return _error;
                }
            }

            public Query(string path, string command)
            {
                _path = path;
                _command = command;
            }

            public Query WithLimit(int limit)
            {
                _limitString = string.Format("-n {0}", limit);
                return this;
            }

            public Query WithFormat(string format)
            {
                _formatString = string.Format("--pretty=\"{0}\"", format);
                return this;
            }

            public Query WithArg(string option)
            {
                _arguments.Append(" ").Append(option);
                return this;
            }

            public Query WithOption(string option)
            {
                _options.Append(" --").Append(option);
                return this;
            }

            public Query Since(DateTime date)
            {
                WithOption("since", date.ToString());
                return this;
            }

            public Query WithOption(string option, string arg)
            {
                _options.Append(" --").Append(option).Append(" \"").Append(arg).Append("\"");
                return this;
            }

            public string Exec()
            {
                string command = string.Format("{0} {1} {2} {3} {4}", _command, _arguments, _options, _formatString, _limitString);
                var result = NativeConsole.RunProcess(Binary, command, _path);

                _error = result.Error;
                return result.Output;
            }
        }

        #endregion

        readonly string _path;

        public Repository(string path)
        {
            _path = path;
        }

        public string ResetToCommit(string commit)
        {
            var log = new StringBuilder();
            log.AppendLine("Checkout");
            var checkOutResult = NativeConsole.RunProcess(Binary, string.Format("checkout {0}", commit), _path);
            log.Append(checkOutResult.Output);

            log.AppendLine("Reset");
            var resetResult = NativeConsole.RunProcess(Binary, "reset --hard", _path);

            log.Append(resetResult.Output);

            return log.ToString();
        }

        public string GetCommit()
        {
            return CreateLogQuery().WithFormat(Format.Commit).WithLimit(1).Exec().Trim();
        }

        public string GetBranch()
        {
            return CreateQuery("rev-parse").WithOption("abbrev-ref", "HEAD").Exec().Trim();
        }

        public string GetUser()
        {
            return CreateQuery("config").WithArg("user.email").Exec().Trim();
        }

        public Query CreateQuery(string command)
        {
            return new Query(_path, command);
        }

        public Query CreateLogQuery()
        {
            return new Query(_path, "log");
        }
    }
}
