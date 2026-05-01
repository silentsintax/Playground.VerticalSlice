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
                if (request is null || string.IsNullOrEmpty(request.securityName))
                    return Result<CreateSecurityResponse>.Failure(Error.Validation("Informe o nome do papel"));

                var security = await repository.CreateSecurity();

                if (security.isFailure)
                    return Result<CreateSecurityResponse>.Failure(Error.Validation("Failed to create security."));

                return new CreateSecurityResponse(security.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating a security with name {SecurityName}", request.securityName);

                return Result<CreateSecurityResponse>.Failure(Error.InternalServerError("An unexpected error occurred while creating the security."));
            }
        }
    }
}
