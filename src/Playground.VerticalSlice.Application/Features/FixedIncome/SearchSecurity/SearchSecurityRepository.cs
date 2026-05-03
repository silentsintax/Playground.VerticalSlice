using Dapper;
using Microsoft.Data.SqlClient;
using Playground.VerticalSlice.Application.Shared.Monads;

namespace Playground.VerticalSlice.Application.Features.FixedIncome.SearchSecurity
{
    public interface ISearchSecurityRepository
    {
        Task<Maybe<SecurityDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Maybe<SecurityDto>> GetByISINAsync(string isin, CancellationToken ct = default);
    }

    public class SearchSecurityRepository : ISearchSecurityRepository
    {
        private readonly string _connectionString;

        public SearchSecurityRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection CreateConnection() => new(_connectionString);

        public async Task<Maybe<SecurityDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = """
            SELECT * FROM FixedIncomeSecurities
            WHERE Id = @Id AND Status != 'Cancelled';
            """;

            using var connection = CreateConnection();

            var security = await connection.QuerySingleOrDefaultAsync<SecurityDto>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));

            return Maybe<SecurityDto>.FromNullable(security);
        }


        public async Task<Maybe<SecurityDto>> GetByISINAsync(string isin, CancellationToken ct = default)
        {
            const string sql = """
            SELECT * FROM FixedIncomeSecurities
            WHERE ISIN = @ISIN;
            """;

            using var connection = CreateConnection();
            var security = await connection.QuerySingleOrDefaultAsync<SecurityDto>(
                new CommandDefinition(sql, new { ISIN = isin }, cancellationToken: ct));

            return Maybe<SecurityDto>.FromNullable(security);
        }
    }
}
