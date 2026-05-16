using AutoFixture;
using Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity;
using Playground.VerticalSlice.Application.Shared.Enums;

namespace Playground.VeticalSlice.UnitTests.Application.Features.FixedIncome.CreateSecurity
{
    public class CreateSecurityModelTests
    {
        private readonly IFixture _fixture = new Fixture();

        #region CreateSecurityRequest Tests

        [Fact]
        public void CreateSecurityRequest_WithValidData_CreatesSuccessfully()
        {
            // Arrange
            var request = new CreateSecurityRequest(
                Id: 1,
                ISIN: "BRCDBCDB0001",
                CETIP: "123456",
                Name: "Test CDB",
                Type: SecurityType.CDB,
                IssuerName: "Test Bank",
                IssuerCNPJ: "00.000.000/0000-00",
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
                IssueDate: new DateOnly(2024, 1, 1),
                MaturityDate: new DateOnly(2025, 1, 1),
                GracePeriodEnd: null,
                DurationDays: 252,
                IsIRExempt: false,
                IsIOFExempt: false,
                GuaranteeType: "FGC",
                FGCGuaranteeLimit: 250000m,
                IsPubliclyOffered: false,
                Liquidity: LiquidityType.D1,
                AllowsEarlyRedemption: true,
                EarlyRedemptionPenalty: 0.5m
            );

            // Act & Assert
            Assert.NotNull(request);
            Assert.Equal("BRCDBCDB0001", request.ISIN);
            Assert.Equal("Test CDB", request.Name);
            Assert.Equal(SecurityType.CDB, request.Type);
            Assert.Equal(1000m, request.FaceValue);
        }

        [Fact]
        public void CreateSecurityRequest_IsRecord_SupportEquality()
        {
            // Arrange
            var request1 = new CreateSecurityRequest(
                Id: 1,
                ISIN: "BRCDBCDB0001",
                CETIP: "123456",
                Name: "Test CDB",
                Type: SecurityType.CDB,
                IssuerName: "Test Bank",
                IssuerCNPJ: "00.000.000/0000-00",
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
                IssueDate: new DateOnly(2024, 1, 1),
                MaturityDate: new DateOnly(2025, 1, 1),
                GracePeriodEnd: null,
                DurationDays: 252,
                IsIRExempt: false,
                IsIOFExempt: false,
                GuaranteeType: "FGC",
                FGCGuaranteeLimit: 250000m,
                IsPubliclyOffered: false,
                Liquidity: LiquidityType.D1,
                AllowsEarlyRedemption: true,
                EarlyRedemptionPenalty: 0.5m
            );

            var request2 = new CreateSecurityRequest(
                Id: 1,
                ISIN: "BRCDBCDB0001",
                CETIP: "123456",
                Name: "Test CDB",
                Type: SecurityType.CDB,
                IssuerName: "Test Bank",
                IssuerCNPJ: "00.000.000/0000-00",
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
                IssueDate: new DateOnly(2024, 1, 1),
                MaturityDate: new DateOnly(2025, 1, 1),
                GracePeriodEnd: null,
                DurationDays: 252,
                IsIRExempt: false,
                IsIOFExempt: false,
                GuaranteeType: "FGC",
                FGCGuaranteeLimit: 250000m,
                IsPubliclyOffered: false,
                Liquidity: LiquidityType.D1,
                AllowsEarlyRedemption: true,
                EarlyRedemptionPenalty: 0.5m
            );

            // Act & Assert
            Assert.Equal(request1, request2);
        }

        [Fact]
        public void CreateSecurityRequest_WithNullableFields_CreatesSuccessfully()
        {
            // Arrange & Act
            var request = new CreateSecurityRequest(
                Id: 1,
                ISIN: "BRCDBCDB0001",
                CETIP: null,
                Name: "Test CDB",
                Type: SecurityType.CDB,
                IssuerName: "Test Bank",
                IssuerCNPJ: "00.000.000/0000-00",
                IssuerRating: null,
                FaceValue: 1000m,
                UnitPrice: 950m,
                MinimumInvestment: 100m,
                Currency: "BRL",
                Indexer: IndexerType.CDI,
                Rate: 110m,
                RateType: RateType.PercentageOfIndexer,
                Spread: null,
                PaymentFrequency: InterestPaymentFrequency.AtMaturity,
                IssueDate: new DateOnly(2024, 1, 1),
                MaturityDate: new DateOnly(2025, 1, 1),
                GracePeriodEnd: null,
                DurationDays: 252,
                IsIRExempt: false,
                IsIOFExempt: false,
                GuaranteeType: null,
                FGCGuaranteeLimit: null,
                IsPubliclyOffered: false,
                Liquidity: LiquidityType.D1,
                AllowsEarlyRedemption: false,
                EarlyRedemptionPenalty: null
            );

            // Assert
            Assert.Null(request.CETIP);
            Assert.Null(request.IssuerRating);
            Assert.Null(request.Spread);
            Assert.Null(request.GracePeriodEnd);
            Assert.Null(request.GuaranteeType);
            Assert.Null(request.FGCGuaranteeLimit);
            Assert.Null(request.EarlyRedemptionPenalty);
        }

        #endregion

        #region CreateSecurityResponse Tests

        [Fact]
        public void CreateSecurityResponse_WithValidId_CreatesSuccessfully()
        {
            // Arrange
            var id = _fixture.Create<int>();

            // Act
            var response = new CreateSecurityResponse(id);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(id, response.id);
        }

        [Fact]
        public void CreateSecurityResponse_IsRecord_SupportEquality()
        {
            // Arrange
            var id = _fixture.Create<int>();
            var response1 = new CreateSecurityResponse(id);
            var response2 = new CreateSecurityResponse(id);

            // Act & Assert
            Assert.Equal(response1, response2);
        }

        [Fact]
        public void CreateSecurityResponse_WithDifferentIds_AreNotEqual()
        {
            // Arrange
            var response1 = new CreateSecurityResponse(1);
            var response2 = new CreateSecurityResponse(2);

            // Act & Assert
            Assert.NotEqual(response1, response2);
        }

        #endregion
    }
}
