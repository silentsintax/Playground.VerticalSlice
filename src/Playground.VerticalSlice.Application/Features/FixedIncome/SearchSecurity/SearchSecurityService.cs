using Microsoft.Extensions.Logging;
using Playground.VerticalSlice.Application.Shared.Monads;

namespace Playground.VerticalSlice.Application.Features.FixedIncome.SearchSecurity
{
    public interface ISearchSecurityService
    {
        Task<Result<SecurityDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<SecurityDto>> GetByIsinAsync(string isin, CancellationToken ct = default);
    }

    public class SearchSecurityService(ISearchSecurityRepository repository, ILogger<SearchSecurityService> logger) : ISearchSecurityService
    {
        public async Task<Result<SecurityDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var security = await repository.GetByIdAsync(id, ct);

                if(security.IsNone)
                    return Result<SecurityDto>.Failure(Error.NotFound($"Security with ID {id} not found"));

                return security.GetOrThrow();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while searching for security with ID {SecurityId}", id);
                return Result<SecurityDto>.Failure(Error.InternalServerError($"An error occurred while searching for security with ID {id}"));
            }
        }

        public async Task<Result<SecurityDto>> GetByIsinAsync(string isin, CancellationToken ct = default)
        {
            try
            {
                var security = await repository.GetByISINAsync(isin, ct);

                if (security.IsNone)
                    return Result<SecurityDto>.Failure(Error.NotFound($"Security with ISIN {isin} not found"));

                return security.GetOrThrow();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while searching for security with ISIN {isin}", isin);
                return Result<SecurityDto>.Failure(Error.InternalServerError($"An error occurred while searching for security with ISIN {isin}"));
            }
        }
    }
}
