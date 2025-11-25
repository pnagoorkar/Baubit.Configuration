using Baubit.Traceability.Errors;

namespace Baubit.Configuration.Traceability
{
    public class EnvVarNotFound : AError
    {
        public string EnvVariable { get; private set; }
        public EnvVarNotFound(string envVariable) : base(new System.Collections.Generic.List<FluentResults.IError>(), $"Environemnt variable: {envVariable} not found", default)
        {
            EnvVariable = envVariable;
        }
    }
}
