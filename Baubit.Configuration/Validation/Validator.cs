using Baubit.Validation;
using FluentResults;

namespace Baubit.Configuration.Validation
{
    public abstract class Validator<TConfiguration> : IValidator<TConfiguration>
    {
        public abstract Result Run(TConfiguration validatable);
    }
}
