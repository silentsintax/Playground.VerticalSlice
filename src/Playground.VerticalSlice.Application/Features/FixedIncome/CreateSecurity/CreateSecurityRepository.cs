using Dapper;
using Microsoft.Data.SqlClient;
using Playground.VerticalSlice.Application.Shared.Monads;
using System.Data;

namespace Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity
{
    public interface ICreateSecurityRepository 
    {
        Task<Result<int>> CreateSecurity(CreateSecuritEntity security, CancellationToken ct);
    }

    public sealed class CreateSecurityRepository : ICreateSecurityRepository
    {
        private readonly string _connectionString;

        public CreateSecurityRepository(string connectionString)
            => _connectionString = connectionString;
        private SqlConnection CreateConnection() => new(_connectionString);

        public async Task<Result<int>> CreateSecurity(CreateSecuritEntity security, CancellationToken ct)
        {
            const string sql = """
            INSERT INTO FixedIncomeSecurities (
                ISIN, CETIP, Name, Type,
                IssuerName, IssuerCNPJ, IssuerRating,
                FaceValue, UnitPrice, MinimumInvestment, Currency,
                Indexer, Rate, RateType, Spread, PaymentFrequency,
                IssueDate, MaturityDate, GracePeriodEnd, DurationDays,
                IsIRExempt, IsIOFExempt, GuaranteeType, FGCGuaranteeLimit, IsPubliclyOffered,
                Liquidity, AllowsEarlyRedemption, EarlyRedemptionPenalty,
                Status, CreatedAt, UpdatedAt
            )
            VALUES (
                @ISIN, @CETIP, @Name, @Type,
                @IssuerName, @IssuerCNPJ, @IssuerRating,
                @FaceValue, @UnitPrice, @MinimumInvestment, @Currency,
                @Indexer, @Rate, @RateType, @Spread, @PaymentFrequency,
                @IssueDate, @MaturityDate, @GracePeriodEnd, @DurationDays,
                @IsIRExempt, @IsIOFExempt, @GuaranteeType, @FGCGuaranteeLimit, @IsPubliclyOffered,
                @Liquidity, @AllowsEarlyRedemption, @EarlyRedemptionPenalty,
                @Status, @CreatedAt, @UpdatedAt
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);  -- ← returns generated Id
            """;

            using var connection = CreateConnection();
            var generatedId = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, ToParameters(security), cancellationToken: ct));

            return generatedId;
        }

        
        private static DynamicParameters ToParameters(CreateSecuritEntity s)
        {
            var p = new DynamicParameters();

            p.Add("@ISIN", s.ISIN, DbType.String, size: 12);
            p.Add("@CETIP", s.CETIP, DbType.String, size: 12);
            p.Add("@Name", s.Name, DbType.String, size: 200);
            p.Add("@Type", s.Type.ToString(), DbType.String, size: 30);
            p.Add("@IssuerName", s.IssuerName, DbType.String, size: 300);
            p.Add("@IssuerCNPJ", s.IssuerCNPJ, DbType.String, size: 18);
            p.Add("@IssuerRating", s.IssuerRating, DbType.String, size: 10);
            p.Add("@FaceValue", s.FaceValue, DbType.Decimal);
            p.Add("@UnitPrice", s.UnitPrice, DbType.Decimal);
            p.Add("@MinimumInvestment", s.MinimumInvestment, DbType.Decimal);
            p.Add("@Currency", s.Currency, DbType.StringFixedLength, size: 3);
            p.Add("@Indexer", s.Indexer.ToString(), DbType.String, size: 20);
            p.Add("@Rate", s.Rate, DbType.Decimal);
            p.Add("@RateType", s.RateType.ToString(), DbType.String, size: 30);
            p.Add("@Spread", s.Spread, DbType.Decimal);
            p.Add("@PaymentFrequency", s.PaymentFrequency.ToString(), DbType.String, size: 20);
            p.Add("@IssueDate", s.IssueDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
            p.Add("@MaturityDate", s.MaturityDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
            p.Add("@GracePeriodEnd", s.GracePeriodEnd?.ToDateTime(TimeOnly.MinValue), DbType.Date);
            p.Add("@DurationDays", s.DurationDays, DbType.Int32);
            p.Add("@IsIRExempt", s.IsIRExempt, DbType.Boolean);
            p.Add("@IsIOFExempt", s.IsIOFExempt, DbType.Boolean);
            p.Add("@GuaranteeType", s.GuaranteeType, DbType.String, size: 100);
            p.Add("@FGCGuaranteeLimit", s.FGCGuaranteeLimit, DbType.Decimal);
            p.Add("@IsPubliclyOffered", s.IsPubliclyOffered, DbType.Boolean);
            p.Add("@Liquidity", s.Liquidity.ToString(), DbType.String, size: 20);
            p.Add("@AllowsEarlyRedemption", s.AllowsEarlyRedemption, DbType.Boolean);
            p.Add("@EarlyRedemptionPenalty", s.EarlyRedemptionPenalty, DbType.Decimal);
            p.Add("@Status", s.Status.ToString(), DbType.String, size: 20);
            p.Add("@CreatedAt", s.CreatedAt, DbType.DateTime2);
            p.Add("@UpdatedAt", s.UpdatedAt, DbType.DateTime2);

            return p;
        }
    }
}
