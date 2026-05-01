using Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity;

namespace Playground.VerticalSlice.Api.Extensions
{
    public static class InternalServicesExtensions
    {
        public static IServiceCollection AddInternalServices(this IServiceCollection services)
        {
            services.AddScoped<ICreateSecurityService, CreateSecurityService>();
            services.AddScoped<ICreateSecurityRepository, CreateSecurityRepository>();
            return services;
        }
    }
}
