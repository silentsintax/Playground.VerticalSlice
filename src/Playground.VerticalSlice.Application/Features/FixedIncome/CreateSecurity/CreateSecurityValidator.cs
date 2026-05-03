using FluentValidation;
using Playground.VerticalSlice.Application.Shared.Enums;

namespace Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity
{
    public class FixedIncomeSecurityValidator : AbstractValidator<CreateSecurityRequest>
    {
        public FixedIncomeSecurityValidator()
        {
            RuleSet("Identification", IdentificationRules);
            RuleSet("Issuer", IssuerRules);
            RuleSet("Financial", FinancialRules);
            RuleSet("Yield", YieldRules);
            RuleSet("Dates", DateRules);
            RuleSet("Tax", TaxRules);
            RuleSet("Liquidity", LiquidityRules);

            // Default: all rules run together
            Include(new FixedIncomeIdentificationValidator());
            Include(new FixedIncomeIssuerValidator());
            Include(new FixedIncomeFinancialValidator());
            Include(new FixedIncomeYieldValidator());
            Include(new FixedIncomeDateValidator());
            Include(new FixedIncomeTaxValidator());
            Include(new FixedIncomeLiquidityValidator());

            // Cross-field rules
            CrossFieldRules();
        }

        private void IdentificationRules() => Include(new FixedIncomeIdentificationValidator());
        private void IssuerRules() => Include(new FixedIncomeIssuerValidator());
        private void FinancialRules() => Include(new FixedIncomeFinancialValidator());
        private void YieldRules() => Include(new FixedIncomeYieldValidator());
        private void DateRules() => Include(new FixedIncomeDateValidator());
        private void TaxRules() => Include(new FixedIncomeTaxValidator());
        private void LiquidityRules() => Include(new FixedIncomeLiquidityValidator());

        private void CrossFieldRules()
        {
            // IPCA/IGPM must have a real spread defined
            RuleFor(x => x.Spread)
                .NotNull()
                .GreaterThanOrEqualTo(0)
                .When(x => x.Indexer == IndexerType.IPCA || x.Indexer == IndexerType.IGPM)
                .WithMessage("IPCA/IGPM securities must define a real spread (e.g. IPCA + 6.5%).");

            // Pre-fixado must not have an indexer spread
            RuleFor(x => x.Spread)
                .Null()
                .When(x => x.Indexer == IndexerType.PreFixado)
                .WithMessage("Pre-fixado securities should not have an indexer spread.");

            // IR-exempt types enforcement
            RuleFor(x => x.IsIRExempt)
                .Equal(true)
                .When(x => IRExemptTypes.Contains(x.Type))
                .WithMessage(x => $"{x.Type} is IR-exempt for individuals — IsIRExempt must be true.");

            RuleFor(x => x.IsIRExempt)
                .Equal(false)
                .When(x => !IRExemptTypes.Contains(x.Type))
                .WithMessage(x => $"{x.Type} is NOT IR-exempt — IsIRExempt must be false.");

            // FGC only covers specific types
            RuleFor(x => x.FGCGuaranteeLimit)
                .LessThanOrEqualTo(250_000)
                .When(x => x.FGCGuaranteeLimit.HasValue && FGCCoveredTypes.Contains(x.Type))
                .WithMessage("FGC coverage limit is R$ 250.000 per CPF/CNPJ per institution.");

            RuleFor(x => x.FGCGuaranteeLimit)
                .Null()
                .When(x => !FGCCoveredTypes.Contains(x.Type))
                .WithMessage(x => $"{x.Type} is not covered by FGC. Remove FGCGuaranteeLimit.");

            // Grace period must be before maturity
            RuleFor(x => x.GracePeriodEnd)
                .LessThan(x => x.MaturityDate)
                .When(x => x.GracePeriodEnd.HasValue)
                .WithMessage("Grace period (carência) must end before the maturity date.");

            // Minimum investment must not exceed face value
            RuleFor(x => x.MinimumInvestment)
                .LessThanOrEqualTo(x => x.FaceValue)
                .When(x => x.FaceValue > 0)
                .WithMessage("Minimum investment cannot exceed the face value.");

            // CVM registration required for public offers
            RuleFor(x => x.IsPubliclyOffered)
                .Equal(true)
                .When(x => x.Type == SecurityType.Debenture || x.Type == SecurityType.CRI || x.Type == SecurityType.CRA)
                .WithMessage("Debentures, CRIs and CRAs require public offering registration (CVM).");

            // Tesouro Direto must be Pre, SELIC or IPCA
            RuleFor(x => x.Indexer)
                .Must(indexer => TesouroAllowedIndexers.Contains(indexer))
                .When(x => TesouroTypes.Contains(x.Type))
                .WithMessage("Tesouro Direto securities must use SELIC, IPCA or PreFixado as indexer.");
        }

        private static readonly SecurityType[] IRExemptTypes =
        [
            SecurityType.LCI, 
            SecurityType.LCA,
            SecurityType.CRI, 
            SecurityType.CRA
        ];

        private static readonly SecurityType[] FGCCoveredTypes =
        [
            SecurityType.CDB, 
            SecurityType.LCI,
            SecurityType.LCA, 
            SecurityType.LC, 
            SecurityType.LF
        ];

        private static readonly SecurityType[] TesouroTypes =
        [
            SecurityType.TesouroPrefixado,
            SecurityType.TesouroSelic,
            SecurityType.TesouroIPCA
        ];

        private static readonly IndexerType[] TesouroAllowedIndexers =
        [
            IndexerType.PreFixado,
            IndexerType.SELIC,
            IndexerType.IPCA
        ];
    }

    public class FixedIncomeLiquidityValidator : AbstractValidator<CreateSecurityRequest>
    {
        public FixedIncomeLiquidityValidator()
        {
            RuleFor(x => x.Liquidity)
                .IsInEnum()
                .WithMessage("Invalid liquidity type.");

            RuleFor(x => x.EarlyRedemptionPenalty)
                .InclusiveBetween(0, 100)
                .When(x => x.EarlyRedemptionPenalty.HasValue)
                .WithMessage("Early redemption penalty must be between 0% and 100%.");

            RuleFor(x => x.EarlyRedemptionPenalty)
                .NotNull()
                .When(x => x.AllowsEarlyRedemption)
                .WithMessage("A penalty rate must be defined when early redemption is allowed.");

            RuleFor(x => x.AllowsEarlyRedemption)
                .Equal(false)
                .When(x => x.Liquidity == LiquidityType.AtMaturity)
                .WithMessage("A security with AtMaturity liquidity cannot allow early redemption.");
        }
    }

    public class FixedIncomeTaxValidator : AbstractValidator<CreateSecurityRequest>
    {
        public FixedIncomeTaxValidator()
        {
            RuleFor(x => x.IsIOFExempt)
                .Equal(true)
                .When(x => (x.MaturityDate.ToDateTime(TimeOnly.MinValue) -
                            x.IssueDate.ToDateTime(TimeOnly.MinValue)).Days >= 30)
                .WithMessage("Securities with term ≥ 30 days are IOF-exempt.");

            RuleFor(x => x.GuaranteeType)
                .NotEmpty()
                .When(x => x.Type == SecurityType.CRI || x.Type == SecurityType.CRA)
                .WithMessage("CRI and CRA must specify a guarantee/collateral type.");
        }
    }

    public class FixedIncomeDateValidator : AbstractValidator<CreateSecurityRequest>
    {
        public FixedIncomeDateValidator()
        {
            RuleFor(x => x.IssueDate)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Issue date cannot be in the future.");

            RuleFor(x => x.MaturityDate)
                .GreaterThan(x => x.IssueDate)
                .WithMessage("Maturity date must be after issue date.");

            RuleFor(x => x.MaturityDate)
                .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("An active security must not have a past maturity date.");

            RuleFor(x => x.DurationDays)
                .GreaterThan(0)
                .WithMessage("Duration must be a positive number of business days.");

            // Minimum terms per BACEN regulation
            RuleFor(x => x.MaturityDate)
                .Must((sec, maturity) => (maturity.ToDateTime(TimeOnly.MinValue) -
                                         sec.IssueDate.ToDateTime(TimeOnly.MinValue)).Days >= 90)
                .When(x => x.Type == SecurityType.LCI || x.Type == SecurityType.LCA)
                .WithMessage("LCI and LCA require a minimum term of 90 days (BACEN rules).");

            RuleFor(x => x.MaturityDate)
                .Must((sec, maturity) => (maturity.ToDateTime(TimeOnly.MinValue) -
                                         sec.IssueDate.ToDateTime(TimeOnly.MinValue)).Days >= 360)
                .When(x => x.Type == SecurityType.LF)
                .WithMessage("LF (Letra Financeira) requires a minimum term of 24 months.");
        }
    }

    public class FixedIncomeYieldValidator : AbstractValidator<CreateSecurityRequest>
    {
        public FixedIncomeYieldValidator()
        {
            RuleFor(x => x.Indexer)
                .IsInEnum()
                .WithMessage("Invalid indexer type.");

            RuleFor(x => x.RateType)
                .IsInEnum()
                .WithMessage("Invalid rate type.");

            // Rate range depends on type
            RuleFor(x => x.Rate)
                .GreaterThan(0)
                .WithMessage("Rate must be positive.");

            RuleFor(x => x.Rate)
                .InclusiveBetween(50, 200)
                .When(x => x.RateType == RateType.PercentageOfIndexer)
                .WithMessage("CDI% rate should be between 50% and 200% of CDI.");

            RuleFor(x => x.Rate)
                .InclusiveBetween(0, 30)
                .When(x => x.RateType == RateType.FixedRate)
                .WithMessage("Fixed annual rate (% a.a.) must be between 0% and 30%.");

            RuleFor(x => x.Spread)
                .InclusiveBetween(-500, 2000) // basis points
                .When(x => x.Spread.HasValue)
                .WithMessage("Spread must be between -500 and 2000 basis points.");

            RuleFor(x => x.PaymentFrequency)
                .IsInEnum()
                .WithMessage("Invalid payment frequency.");
        }
    }

    public class FixedIncomeFinancialValidator : AbstractValidator<CreateSecurityRequest>
    {
        public FixedIncomeFinancialValidator()
        {
            RuleFor(x => x.FaceValue)
                .GreaterThan(0)
                .WithMessage("Face value (valor nominal) must be positive.");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0)
                .WithMessage("Unit price (PU) must be positive.");

            RuleFor(x => x.UnitPrice)
                .LessThanOrEqualTo(x => x.FaceValue * 1.5m) // sanity ceiling
                .WithMessage("Unit price seems abnormally high compared to face value.");

            RuleFor(x => x.MinimumInvestment)
                .GreaterThan(0)
                .WithMessage("Minimum investment must be greater than zero.");
        }
    }

    public class FixedIncomeIssuerValidator : AbstractValidator<CreateSecurityRequest>
    {
        public FixedIncomeIssuerValidator()
        {
            RuleFor(x => x.IssuerName)
                .NotEmpty()
                .MaximumLength(300)
                .WithMessage("Issuer name is required.");

            RuleFor(x => x.IssuerCNPJ)
                .NotEmpty()
                .Must(BeValidCNPJ)
                .WithMessage("Issuer CNPJ is invalid. Expected format: XX.XXX.XXX/XXXX-XX.");

            RuleFor(x => x.IssuerRating)
                .Must(BeValidRating)
                .When(x => !string.IsNullOrEmpty(x.IssuerRating))
                .WithMessage("Invalid rating. Use S&P/Fitch (AAA to D) or Moody's (Aaa to C) scale.");
        }

        private bool BeValidCNPJ(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj)) return false;

            // Strip formatting
            cnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Trim();

            if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0])) return false;

            int[] weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

            int sum = cnpj.Take(12).Select((c, i) => (c - '0') * weights1[i]).Sum();
            int d1 = sum % 11 < 2 ? 0 : 11 - (sum % 11);

            sum = cnpj.Take(13).Select((c, i) => (c - '0') * weights2[i]).Sum();
            int d2 = sum % 11 < 2 ? 0 : 11 - (sum % 11);

            return cnpj[12] - '0' == d1 && cnpj[13] - '0' == d2;
        }

        private static readonly HashSet<string> ValidRatings =
        [
            // S&P / Fitch
            "AAA","AA+","AA","AA-","A+","A","A-",
            "BBB+","BBB","BBB-","BB+","BB","BB-",
            "B+","B","B-","CCC+","CCC","CCC-","CC","C","D",
            // Moody's
            "Aaa","Aa1","Aa2","Aa3","A1","A2","A3",
            "Baa1","Baa2","Baa3","Ba1","Ba2","Ba3",
            "B1","B2","B3","Caa1","Caa2","Caa3","Ca","C"
        ];

        private bool BeValidRating(string? rating)
            => rating == null || ValidRatings.Contains(rating);
    }

    public class FixedIncomeIdentificationValidator : AbstractValidator<CreateSecurityRequest>
    {
        public FixedIncomeIdentificationValidator()
        {
            RuleFor(x => x.ISIN)
                .NotEmpty()
                .Length(12)
                .Matches(@"^BR[A-Z0-9]{10}$")
                .WithMessage("ISIN must follow Brazilian format: BR + 10 alphanumeric chars (e.g. BRCDBCDB0001).");

            RuleFor(x => x.CETIP)
                .Matches(@"^[A-Z0-9]{12}$")
                .When(x => !string.IsNullOrEmpty(x.CETIP))
                .WithMessage("CETIP code must be 12 alphanumeric characters.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200)
                .WithMessage("Security name is required and must be under 200 characters.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid security type.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Equal("BRL")
                .WithMessage("Only BRL (Real) is supported for domestic fixed income.");
        }
    }
}
