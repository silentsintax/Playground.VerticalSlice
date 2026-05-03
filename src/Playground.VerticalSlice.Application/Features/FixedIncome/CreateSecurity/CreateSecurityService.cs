using Microsoft.Extensions.Logging;
using Playground.VerticalSlice.Application.Shared.Monads;

namespace Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity
{
    public interface ICreateSecurityService
    {
        Task<Result<CreateSecurityResponse>> Create(CreateSecurityRequest request, CancellationToken ct);
    }

    public class CreateSecurityService(
        ICreateSecurityRepository repository,
        ILogger<CreateSecurityService> logger) : ICreateSecurityService
    {
        public async Task<Result<CreateSecurityResponse>> Create(CreateSecurityRequest request, CancellationToken ct)
        {
            try
            {
                var validationResult = new FixedIncomeSecurityValidator().Validate(request);

                if (!validationResult.IsValid)
                    return Result<CreateSecurityResponse>.Failure(Error.Validation(validationResult.Errors.Select(e => e.ErrorMessage).ToArray()));
                
                var security = await repository.CreateSecurity(CreateSecurityMapping.MapToEntity(request), ct);

                if (security.isFailure)
                    return Result<CreateSecurityResponse>.Failure(Error.Validation("Failed to create security."));

                return new CreateSecurityResponse(security.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating a security with name {SecurityName}", request.Name);

                return Result<CreateSecurityResponse>.Failure(Error.InternalServerError("An unexpected error occurred while creating the security."));
            }
        }
    }
}
