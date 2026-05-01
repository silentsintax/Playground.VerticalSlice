using Playground.VerticalSlice.Application.Shared.Monads;

namespace Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity
{
    public interface ICreateSecurityRepository 
    {
        Task<Result<int>> CreateSecurity();
    }

    public class CreateSecurityRepository : ICreateSecurityRepository
    {
        public async Task<Result<int>> CreateSecurity()
        {
            await Task.Delay(100);
            
            int newSecurityId = 123;
            
            return Result<int>.Success(newSecurityId);
        }
    }
}
