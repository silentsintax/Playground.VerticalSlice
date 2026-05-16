using AutoFixture;
using FluentValidation.TestHelper;
using Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity;
using Playground.VerticalSlice.Application.Shared.Enums;

namespace Playground.VeticalSlice.UnitTests.Application.Features.FixedIncome.CreateSecurity
{
    public class CreateSecurityValidatorTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly FixedIncomeSecurityValidator _validator = new();

        #region Valid Request Tests

        [Fact]
        public void Validate_WithValidCDBRequest_PassesValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithValidLCIRequest_PassesValidation()
        {
            // Arrange
            var request = CreateValidLCIRequest();

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithValidTesouroDiretoRequest_PassesValidation()
        {
            // Arrange
            var request = CreateValidTesouroDiretoRequest();

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.True(result.IsValid);
        }

        #endregion

        #region Spread Validation Tests

        [Fact]
        public void Validate_WithIPCAIndexerWithoutSpread_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                Indexer = IndexerType.IPCA,
                Spread = null
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateSecurityRequest.Spread));
        }

        [Fact]
        public void Validate_WithIGPMIndexerWithoutSpread_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                Indexer = IndexerType.IGPM,
                Spread = null
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateSecurityRequest.Spread));
        }

        [Fact]
        public void Validate_WithPreFixadoIndexerAndSpread_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                Indexer = IndexerType.PreFixado,
                Spread = 2.5m
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateSecurityRequest.Spread));
        }

        [Fact]
        public void Validate_WithIPCAIndexerAndValidSpread_PassesValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                Indexer = IndexerType.IPCA,
                Spread = 2.5m
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.True(result.IsValid);
        }

        #endregion

        #region IR Exemption Tests

        [Fact]
        public void Validate_WithLCINotMarkedAsIRExempt_FailsValidation()
        {
            // Arrange
            var request = CreateValidLCIRequest();
            request = request with { IsIRExempt = false };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateSecurityRequest.IsIRExempt));
        }

        [Fact]
        public void Validate_WithLCANotMarkedAsIRExempt_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                Type = SecurityType.LCA,
                IsIRExempt = false
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithCRINotMarkedAsIRExempt_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                Type = SecurityType.CRI,
                IsIRExempt = false
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithCRANotMarkedAsIRExempt_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                Type = SecurityType.CRA,
                IsIRExempt = false
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithCDBMarkedAsIRExempt_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with { IsIRExempt = true };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        #endregion

        #region FGC Coverage Tests

        [Fact]
        public void Validate_WithCDBExceedingFGCLimit_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with { FGCGuaranteeLimit = 300000m };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithLCIExceedingFGCLimit_FailsValidation()
        {
            // Arrange
            var request = CreateValidLCIRequest();
            request = request with { FGCGuaranteeLimit = 300000m };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithNonFGCTypeHavingFGCLimit_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                Type = SecurityType.Debenture,
                IsIRExempt = false,
                FGCGuaranteeLimit = 100000m
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithCDBHavingValidFGCLimit_PassesValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with { FGCGuaranteeLimit = 250000m };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.True(result.IsValid);
        }

        #endregion

        #region Grace Period Tests

        [Fact]
        public void Validate_WithGracePeriodAfterMaturity_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                IssueDate = new DateOnly(2024, 1, 1),
                MaturityDate = new DateOnly(2025, 1, 1),
                GracePeriodEnd = new DateOnly(2025, 6, 1)
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithGracePeriodBeforeMaturity_PassesValidation()
        {
            // Arrange
            var today = DateOnly.FromDateTime(DateTime.Today);
            var issueDate = today.AddDays(-30);
            var graceDate = issueDate.AddDays(90);
            var maturityDate = today.AddDays(400);

            var request = CreateValidCDBRequest();
            request = request with
            {
                IssueDate = issueDate,
                MaturityDate = maturityDate,
                GracePeriodEnd = graceDate
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.True(result.IsValid);
        }

        #endregion

        #region Minimum Investment Tests

        [Fact]
        public void Validate_WithMinimumInvestmentExceedingFaceValue_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                FaceValue = 1000m,
                MinimumInvestment = 1500m
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithMinimumInvestmentEqualToFaceValue_PassesValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                FaceValue = 1000m,
                MinimumInvestment = 1000m
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.True(result.IsValid);
        }

        #endregion

        #region Public Offering Tests

        [Fact]
        public void Validate_WithDebenturNotMarkedAsPublicOffering_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                Type = SecurityType.Debenture,
                IsIRExempt = false,
                IsPubliclyOffered = false
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithCRINotMarkedAsPublicOffering_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                Type = SecurityType.CRI,
                IsIRExempt = true,
                IsPubliclyOffered = false
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithCRANotMarkedAsPublicOffering_FailsValidation()
        {
            // Arrange
            var request = CreateValidCDBRequest();
            request = request with
            {
                Type = SecurityType.CRA,
                IsIRExempt = true,
                IsPubliclyOffered = false
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        #endregion

        #region Tesouro Direto Tests

        [Fact]
        public void Validate_WithTesouroPrefixadoAndCDIIndexer_FailsValidation()
        {
            // Arrange
            var request = CreateValidTesouroDiretoRequest();
            request = request with
            {
                Type = SecurityType.TesouroPrefixado,
                Indexer = IndexerType.CDI
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithTesouroSELICAndValidIndexer_PassesValidation()
        {
            // Arrange
            var request = CreateValidTesouroDiretoRequest();
            request = request with
            {
                Type = SecurityType.TesouroSelic,
                Indexer = IndexerType.SELIC
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            Assert.True(result.IsValid);
        }

        #endregion

        #region Helper Methods

        private CreateSecurityRequest CreateValidCDBRequest()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var issueDate = today.AddDays(-30);
            var maturityDate = today.AddDays(400);

            return new CreateSecurityRequest(
                Id: 1,
                ISIN: "BRCDBCDB0001",
                CETIP: "123456789012",
                Name: "Test CDB",
                Type: SecurityType.CDB,
                IssuerName: "Test Bank",
                IssuerCNPJ: "11.222.333/0001-81",
                IssuerRating: "A",
                FaceValue: 1000m,
                UnitPrice: 950m,
                MinimumInvestment: 100m,
                Currency: "BRL",
                Indexer: IndexerType.CDI,
                Rate: 110m,
                RateType: RateType.PercentageOfIndexer,
                Spread: null,
                PaymentFrequency: InterestPaymentFrequency.AtMaturity,
                IssueDate: issueDate,
                MaturityDate: maturityDate,
                GracePeriodEnd: null,
                DurationDays: 252,
                IsIRExempt: false,
                IsIOFExempt: true,
                GuaranteeType: "FGC",
                FGCGuaranteeLimit: 250000m,
                IsPubliclyOffered: false,
                Liquidity: LiquidityType.D1,
                AllowsEarlyRedemption: true,
                EarlyRedemptionPenalty: 0.5m
            );
        }

        private CreateSecurityRequest CreateValidLCIRequest()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var issueDate = today.AddDays(-30);
            var maturityDate = today.AddDays(180);

            return new CreateSecurityRequest(
                Id: 1,
                ISIN: "BRLDILCD0001",
                CETIP: "123456789012",
                Name: "Test LCI",
                Type: SecurityType.LCI,
                IssuerName: "Test Bank",
                IssuerCNPJ: "11.222.333/0001-81",
                IssuerRating: "A",
                FaceValue: 1000m,
                UnitPrice: 950m,
                MinimumInvestment: 100m,
                Currency: "BRL",
                Indexer: IndexerType.CDI,
                Rate: 105m,
                RateType: RateType.PercentageOfIndexer,
                Spread: null,
                PaymentFrequency: InterestPaymentFrequency.AtMaturity,
                IssueDate: issueDate,
                MaturityDate: maturityDate,
                GracePeriodEnd: null,
                DurationDays: 252,
                IsIRExempt: true,
                IsIOFExempt: true,
                GuaranteeType: "FGC",
                FGCGuaranteeLimit: 250000m,
                IsPubliclyOffered: false,
                Liquidity: LiquidityType.D1,
                AllowsEarlyRedemption: false,
                EarlyRedemptionPenalty: null
            );
        }

        private CreateSecurityRequest CreateValidTesouroDiretoRequest()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var issueDate = today.AddDays(-30);
            var maturityDate = today.AddDays(500);

            return new CreateSecurityRequest(
                Id: 1,
                ISIN: "BRSTNCLP0020",
                CETIP: null,
                Name: "Tesouro Prefixado 2025",
                Type: SecurityType.TesouroPrefixado,
                IssuerName: "Ministério da Fazenda",
                IssuerCNPJ: "80.212.568/0001-05",
                IssuerRating: "AAA",
                FaceValue: 1000m,
                UnitPrice: 990m,
                MinimumInvestment: 100m,
                Currency: "BRL",
                Indexer: IndexerType.PreFixado,
                Rate: 12.5m,
                RateType: RateType.FixedRate,
                Spread: null,
                PaymentFrequency: InterestPaymentFrequency.AtMaturity,
                IssueDate: issueDate,
                MaturityDate: maturityDate,
                GracePeriodEnd: null,
                DurationDays: 252,
                IsIRExempt: false,
                IsIOFExempt: true,
                GuaranteeType: null,
                FGCGuaranteeLimit: null,
                IsPubliclyOffered: false,
                Liquidity: LiquidityType.SecondaryMarket,
                AllowsEarlyRedemption: false,
                EarlyRedemptionPenalty: null
            );
        }

        #endregion
    }
}
