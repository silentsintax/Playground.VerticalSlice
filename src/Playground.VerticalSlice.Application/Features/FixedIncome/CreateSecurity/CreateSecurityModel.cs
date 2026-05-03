using Playground.VerticalSlice.Application.Shared.Enums;

namespace Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity
{
    // === IDENTIFICATION ===
    public record CreateSecurityRequest(
        int Id,
        string ISIN,
        string? CETIP,
        string Name,
        SecurityType Type,

        // === ISSUER ===
        string IssuerName,
        string IssuerCNPJ,
        string? IssuerRating,

        // === FINANCIAL TERMS ===
        decimal FaceValue,
        decimal UnitPrice,
        decimal MinimumInvestment,
        string Currency,

        // === YIELD / REMUNERATION ===
        IndexerType Indexer,
        decimal Rate,
        RateType RateType,
        decimal? Spread,
        InterestPaymentFrequency PaymentFrequency,

        // === DATES ===
        DateOnly IssueDate,
        DateOnly MaturityDate,
        DateOnly? GracePeriodEnd,
        int DurationDays,

        // === TAX & REGULATION ===
        bool IsIRExempt,
        bool IsIOFExempt,
        string? GuaranteeType,
        decimal? FGCGuaranteeLimit,
        bool IsPubliclyOffered,

        // === LIQUIDITY ===
        LiquidityType Liquidity,
        bool AllowsEarlyRedemption,
        decimal? EarlyRedemptionPenalty
    );

    public record CreateSecurityResponse(int id);

    public static class CreateSecurityMapping
    {
        internal static CreateSecuritEntity MapToEntity(CreateSecurityRequest request)
        {
            return new CreateSecuritEntity()
            {
                Id = request.Id,
                ISIN = request.ISIN,
                CETIP = request.CETIP,
                Name = request.Name,
                Type = request.Type,

                IssuerName = request.IssuerName,
                IssuerCNPJ = request.IssuerCNPJ,
                IssuerRating = request.IssuerRating,

                FaceValue = request.FaceValue,
                UnitPrice = request.UnitPrice,
                MinimumInvestment = request.MinimumInvestment,
                Currency = request.Currency,

                Indexer = request.Indexer,
                Rate = request.Rate,
                RateType = request.RateType,
                Spread = request.Spread,
                PaymentFrequency = request.PaymentFrequency,

                IssueDate = request.IssueDate,
                MaturityDate = request.MaturityDate,
                GracePeriodEnd = request.GracePeriodEnd,
                DurationDays = request.DurationDays,

                IsIRExempt = request.IsIRExempt,
                IsIOFExempt = request.IsIOFExempt,
                GuaranteeType = request.GuaranteeType,
                FGCGuaranteeLimit = request.FGCGuaranteeLimit,
                IsPubliclyOffered = request.IsPubliclyOffered,

                Liquidity = request.Liquidity,
                AllowsEarlyRedemption = request.AllowsEarlyRedemption,
                EarlyRedemptionPenalty = request.EarlyRedemptionPenalty,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
