using Baubit.Validation;
using FluentResults;

namespace Baubit.Configuration.Validation
{
    public abstract class AValidator<TConfiguration> : IValidator<TConfiguration>
    {
        public abstract Result Run(TConfiguration validatable);
    }
}
