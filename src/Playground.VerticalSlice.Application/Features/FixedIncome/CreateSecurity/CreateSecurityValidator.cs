using FluentValidation;

namespace Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity
{
    internal class CreateSecurityValidator : AbstractValidator<CreateSecurityRequest>
    {
        public CreateSecurityValidator()
        {
            RuleFor(x => x.isin)
               .NotEmpty()
               .Length(12)
               .Matches(@"^BR[A-Z0-9]{10}$")
               .WithMessage("ISIN must follow Brazilian format: BR + 10 alphanumeric chars (e.g. BRCDBCDB0001).");

            RuleFor(x => x.securityName)
               .NotEmpty()
               .MaximumLength(50)
               .WithMessage("SecurityName must be informed with max lenght of 50 chars.");
        }
    }
}
