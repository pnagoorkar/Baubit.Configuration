using System;

namespace Baubit.Configuration.Traceability
{
    public class EnvironmentVariableNotFound : Exception
    {
        public string EnvVariable { get; private set; }
        public EnvironmentVariableNotFound(string envVariable)
        {
            EnvVariable = envVariable;
        }
    }
}
