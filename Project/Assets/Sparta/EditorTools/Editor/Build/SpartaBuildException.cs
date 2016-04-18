using System;

namespace SpartaTools.Editor.Build
{
    public class SpartaBuildException : Exception
    {
        public SpartaBuildException(string message)
            : base(message)
        {
        }
    }

    public class EmptyModuleException : SpartaBuildException
    {
        public EmptyModuleException(string message)
            : base(message)
        {
        }
    }

    public class CompilerErrorException : SpartaBuildException
    {
        public CompilerErrorException(string message)
            : base(message)
        {
        }
    }

    public class DependencyNotFoundException : SpartaBuildException
    {
        public DependencyNotFoundException(string message)
            : base(message)
        {
        }
    }

    public class CompilerConfigurationException : SpartaBuildException
    {
        public CompilerConfigurationException(string message)
            : base(message)
        {
        }
    }
}