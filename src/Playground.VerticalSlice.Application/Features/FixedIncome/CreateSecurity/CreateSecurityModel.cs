namespace Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity
{
    public record CreateSecurityRequest(string securityName, string isin);
    public record CreateSecurityResponse(int id);

    public static class CreateSecurityMapping
    {
        internal static CreateSecuritEntity MapToEntity(CreateSecurityRequest request)
        {
            return new CreateSecuritEntity()
            {
                ISIN = request.isin,
                SecurityName = request.securityName,
            };
        }
    }
}
