using Playground.VerticalSlice.Application.Shared.Enums;

namespace Playground.VerticalSlice.Application.Features.FixedIncome.SearchSecurity
{
    public class SecurityDto
    {
        public int Id { get; set; }
        public string ISIN { get; set; }
        public string? CETIP { get; set; }
        public string Name { get; set; }
        public SecurityType Type { get; set; }

        public string IssuerName { get; set; }
        public string IssuerCNPJ { get; set; }
        public string? IssuerRating { get; set; }

        public decimal FaceValue { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal MinimumInvestment { get; set; }
        public string Currency { get; set; }

        public IndexerType Indexer { get; set; } 
        public decimal Rate { get; set; }
        public RateType RateType { get; set; }  
        public decimal? Spread { get; set; }       
        public InterestPaymentFrequency PaymentFrequency { get; set; }

        public DateOnly IssueDate { get; set; }        
        public DateOnly MaturityDate { get; set; }     
        public DateOnly? GracePeriodEnd { get; set; }  
        public int DurationDays { get; set; }          

        public bool IsIRExempt { get; set; }           
        public bool IsIOFExempt { get; set; }
        public string? GuaranteeType { get; set; }     
        public decimal? FGCGuaranteeLimit { get; set; }
        public bool IsPubliclyOffered { get; set; }    

        public LiquidityType Liquidity { get; set; } 
        public bool AllowsEarlyRedemption { get; set; }
        public decimal? EarlyRedemptionPenalty { get; set; }

        public SecurityStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
