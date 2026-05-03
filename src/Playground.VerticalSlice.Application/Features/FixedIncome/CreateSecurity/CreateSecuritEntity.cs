using Playground.VerticalSlice.Application.Shared.Enums;

namespace Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity
{
    public class CreateSecuritEntity
    {
        // === IDENTIFICATION ===
        public int Id { get; set; }
        public string ISIN { get; set; }           // International code (e.g., BRCDBCDB0001)
        public string? CETIP { get; set; }         // CETIP/B3 registration code
        public string Name { get; set; }           // Commercial name
        public SecurityType Type { get; set; }     // CDB, LCI, LCA, CRI, CRA, Debenture, Tesouro...

        // === ISSUER ===
        public string IssuerName { get; set; }
        public string IssuerCNPJ { get; set; }
        public string? IssuerRating { get; set; }  // Fitch, Moody's, S&P rating

        // === FINANCIAL TERMS ===
        public decimal FaceValue { get; set; }           // Valor nominal (e.g., R$ 1.000)
        public decimal UnitPrice { get; set; }           // PU (Preço Unitário)
        public decimal MinimumInvestment { get; set; }   // Aplicação mínima
        public string Currency { get; set; } = "BRL";

        // === YIELD / REMUNERATION ===
        public IndexerType Indexer { get; set; }         // CDI, IPCA, SELIC, IGPM, PRE (fixed)
        public decimal Rate { get; set; }                // e.g., 12.5 (% a.a.) or 110 (% do CDI)
        public RateType RateType { get; set; }           // Percentage, Spread, Fixed
        public decimal? Spread { get; set; }             // Spread over indexer (basis points)
        public InterestPaymentFrequency PaymentFrequency { get; set; } // Monthly, Semiannual, AtMaturity

        // === DATES ===
        public DateOnly IssueDate { get; set; }          // Data de emissão
        public DateOnly MaturityDate { get; set; }       // Data de vencimento
        public DateOnly? GracePeriodEnd { get; set; }    // Carência (earliest redemption date)
        public int DurationDays { get; set; }            // Prazo em dias úteis (D.U.)

        // === TAX & REGULATION ===
        public bool IsIRExempt { get; set; }             // LCI, LCA, CRI, CRA are IR-exempt
        public bool IsIOFExempt { get; set; }
        public string? GuaranteeType { get; set; }       // FGC coverage, real guarantee, etc.
        public decimal? FGCGuaranteeLimit { get; set; }  // R$ 250.000 per CPF/CNPJ per institution
        public bool IsPubliclyOffered { get; set; }      // CVM registration required

        // === LIQUIDITY ===
        public LiquidityType Liquidity { get; set; }     // D+0, D+1, AtMaturity, Secondary Market
        public bool AllowsEarlyRedemption { get; set; }
        public decimal? EarlyRedemptionPenalty { get; set; }

        // === STATUS & AUDIT ===
        public SecurityStatus Status { get; set; }       // Active, Matured, Cancelled
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
