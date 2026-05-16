using AutoFixture;
using Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity;
using Playground.VerticalSlice.Application.Shared.Enums;

namespace Playground.VeticalSlice.UnitTests.Application.Features.FixedIncome.CreateSecurity
{
    public class CreateSecurityEntityTests
    {
        private readonly IFixture _fixture = new Fixture();

        #region Instantiation Tests

        [Fact]
        public void CreateSecuritEntity_WithDefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var entity = new CreateSecuritEntity();

            // Assert
            Assert.NotNull(entity);
        }

        [Fact]
        public void CreateSecuritEntity_WithAllPropertiesSet_CreatesSuccessfully()
        {
            // Arrange
            var entity = new CreateSecuritEntity
            {
                Id = 1,
                ISIN = "BRCDBCDB0001",
                CETIP = "123456",
                Name = "Test CDB",
                Type = SecurityType.CDB,
                IssuerName = "Test Bank",
                IssuerCNPJ = "00.000.000/0000-00",
                IssuerRating = "A",
                FaceValue = 1000m,
                UnitPrice = 950m,
                MinimumInvestment = 100m,
                Currency = "BRL",
                Indexer = IndexerType.CDI,
                Rate = 110m,
                RateType = RateType.PercentageOfIndexer,
                Spread = null,
                PaymentFrequency = InterestPaymentFrequency.AtMaturity,
                IssueDate = new DateOnly(2024, 1, 1),
                MaturityDate = new DateOnly(2025, 1, 1),
                GracePeriodEnd = null,
                DurationDays = 252,
                IsIRExempt = false,
                IsIOFExempt = false,
                GuaranteeType = "FGC",
                FGCGuaranteeLimit = 250000m,
                IsPubliclyOffered = false,
                Liquidity = LiquidityType.D1,
                AllowsEarlyRedemption = true,
                EarlyRedemptionPenalty = 0.5m,
                Status = SecurityStatus.Active,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // Act & Assert
            Assert.Equal(1, entity.Id);
            Assert.Equal("BRCDBCDB0001", entity.ISIN);
            Assert.Equal("Test CDB", entity.Name);
            Assert.Equal(SecurityType.CDB, entity.Type);
            Assert.Equal("Test Bank", entity.IssuerName);
            Assert.Equal("00.000.000/0000-00", entity.IssuerCNPJ);
        }

        #endregion

        #region Identification Properties Tests

        [Fact]
        public void ISIN_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var isin = "BRCDBCDB0001";

            // Act
            entity.ISIN = isin;

            // Assert
            Assert.Equal(isin, entity.ISIN);
        }

        [Fact]
        public void CETIP_CanBeSetToNullOrValue()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.CETIP = "123456";
            Assert.Equal("123456", entity.CETIP);

            entity.CETIP = null;
            Assert.Null(entity.CETIP);
        }

        [Fact]
        public void Name_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var name = "Test Security";

            // Act
            entity.Name = name;

            // Assert
            Assert.Equal(name, entity.Name);
        }

        [Fact]
        public void Type_CanBeSetToVariousSecurityTypes()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.Type = SecurityType.CDB;
            Assert.Equal(SecurityType.CDB, entity.Type);

            entity.Type = SecurityType.LCI;
            Assert.Equal(SecurityType.LCI, entity.Type);

            entity.Type = SecurityType.Debenture;
            Assert.Equal(SecurityType.Debenture, entity.Type);
        }

        #endregion

        #region Issuer Properties Tests

        [Fact]
        public void IssuerName_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var issuerName = "Test Bank";

            // Act
            entity.IssuerName = issuerName;

            // Assert
            Assert.Equal(issuerName, entity.IssuerName);
        }

        [Fact]
        public void IssuerCNPJ_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var cnpj = "00.000.000/0000-00";

            // Act
            entity.IssuerCNPJ = cnpj;

            // Assert
            Assert.Equal(cnpj, entity.IssuerCNPJ);
        }

        [Fact]
        public void IssuerRating_CanBeSetToNullOrValue()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.IssuerRating = "AAA";
            Assert.Equal("AAA", entity.IssuerRating);

            entity.IssuerRating = null;
            Assert.Null(entity.IssuerRating);
        }

        #endregion

        #region Financial Terms Properties Tests

        [Fact]
        public void FaceValue_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var faceValue = 1000m;

            // Act
            entity.FaceValue = faceValue;

            // Assert
            Assert.Equal(faceValue, entity.FaceValue);
        }

        [Fact]
        public void UnitPrice_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var unitPrice = 950m;

            // Act
            entity.UnitPrice = unitPrice;

            // Assert
            Assert.Equal(unitPrice, entity.UnitPrice);
        }

        [Fact]
        public void MinimumInvestment_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var minInvestment = 100m;

            // Act
            entity.MinimumInvestment = minInvestment;

            // Assert
            Assert.Equal(minInvestment, entity.MinimumInvestment);
        }

        [Fact]
        public void Currency_DefaultsToAndCanBeModified()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            Assert.Equal("BRL", entity.Currency);

            entity.Currency = "USD";
            Assert.Equal("USD", entity.Currency);
        }

        #endregion

        #region Yield/Remuneration Properties Tests

        [Fact]
        public void Indexer_CanBeSetToVariousIndexerTypes()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.Indexer = IndexerType.CDI;
            Assert.Equal(IndexerType.CDI, entity.Indexer);

            entity.Indexer = IndexerType.IPCA;
            Assert.Equal(IndexerType.IPCA, entity.Indexer);

            entity.Indexer = IndexerType.PreFixado;
            Assert.Equal(IndexerType.PreFixado, entity.Indexer);
        }

        [Fact]
        public void Rate_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var rate = 110m;

            // Act
            entity.Rate = rate;

            // Assert
            Assert.Equal(rate, entity.Rate);
        }

        [Fact]
        public void RateType_CanBeSetToVariousRateTypes()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.RateType = RateType.PercentageOfIndexer;
            Assert.Equal(RateType.PercentageOfIndexer, entity.RateType);

            entity.RateType = RateType.SpreadOverIndexer;
            Assert.Equal(RateType.SpreadOverIndexer, entity.RateType);

            entity.RateType = RateType.FixedRate;
            Assert.Equal(RateType.FixedRate, entity.RateType);
        }

        [Fact]
        public void Spread_CanBeSetToNullOrValue()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.Spread = 2.5m;
            Assert.Equal(2.5m, entity.Spread);

            entity.Spread = null;
            Assert.Null(entity.Spread);
        }

        [Fact]
        public void PaymentFrequency_CanBeSetToVariousFrequencies()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.PaymentFrequency = InterestPaymentFrequency.Monthly;
            Assert.Equal(InterestPaymentFrequency.Monthly, entity.PaymentFrequency);

            entity.PaymentFrequency = InterestPaymentFrequency.Semiannual;
            Assert.Equal(InterestPaymentFrequency.Semiannual, entity.PaymentFrequency);

            entity.PaymentFrequency = InterestPaymentFrequency.AtMaturity;
            Assert.Equal(InterestPaymentFrequency.AtMaturity, entity.PaymentFrequency);
        }

        #endregion

        #region Dates Properties Tests

        [Fact]
        public void IssueDate_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var issueDate = new DateOnly(2024, 1, 1);

            // Act
            entity.IssueDate = issueDate;

            // Assert
            Assert.Equal(issueDate, entity.IssueDate);
        }

        [Fact]
        public void MaturityDate_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var maturityDate = new DateOnly(2025, 1, 1);

            // Act
            entity.MaturityDate = maturityDate;

            // Assert
            Assert.Equal(maturityDate, entity.MaturityDate);
        }

        [Fact]
        public void GracePeriodEnd_CanBeSetToNullOrValue()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var graceDate = new DateOnly(2024, 6, 1);

            // Act & Assert
            entity.GracePeriodEnd = graceDate;
            Assert.Equal(graceDate, entity.GracePeriodEnd);

            entity.GracePeriodEnd = null;
            Assert.Null(entity.GracePeriodEnd);
        }

        [Fact]
        public void DurationDays_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var durationDays = 252;

            // Act
            entity.DurationDays = durationDays;

            // Assert
            Assert.Equal(durationDays, entity.DurationDays);
        }

        #endregion

        #region Tax & Regulation Properties Tests

        [Fact]
        public void IsIRExempt_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.IsIRExempt = true;
            Assert.True(entity.IsIRExempt);

            entity.IsIRExempt = false;
            Assert.False(entity.IsIRExempt);
        }

        [Fact]
        public void IsIOFExempt_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.IsIOFExempt = true;
            Assert.True(entity.IsIOFExempt);

            entity.IsIOFExempt = false;
            Assert.False(entity.IsIOFExempt);
        }

        [Fact]
        public void GuaranteeType_CanBeSetToNullOrValue()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.GuaranteeType = "FGC";
            Assert.Equal("FGC", entity.GuaranteeType);

            entity.GuaranteeType = null;
            Assert.Null(entity.GuaranteeType);
        }

        [Fact]
        public void FGCGuaranteeLimit_CanBeSetToNullOrValue()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.FGCGuaranteeLimit = 250000m;
            Assert.Equal(250000m, entity.FGCGuaranteeLimit);

            entity.FGCGuaranteeLimit = null;
            Assert.Null(entity.FGCGuaranteeLimit);
        }

        [Fact]
        public void IsPubliclyOffered_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.IsPubliclyOffered = true;
            Assert.True(entity.IsPubliclyOffered);

            entity.IsPubliclyOffered = false;
            Assert.False(entity.IsPubliclyOffered);
        }

        #endregion

        #region Liquidity Properties Tests

        [Fact]
        public void Liquidity_CanBeSetToVariousLiquidityTypes()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.Liquidity = LiquidityType.D0;
            Assert.Equal(LiquidityType.D0, entity.Liquidity);

            entity.Liquidity = LiquidityType.D1;
            Assert.Equal(LiquidityType.D1, entity.Liquidity);

            entity.Liquidity = LiquidityType.AtMaturity;
            Assert.Equal(LiquidityType.AtMaturity, entity.Liquidity);
        }

        [Fact]
        public void AllowsEarlyRedemption_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.AllowsEarlyRedemption = true;
            Assert.True(entity.AllowsEarlyRedemption);

            entity.AllowsEarlyRedemption = false;
            Assert.False(entity.AllowsEarlyRedemption);
        }

        [Fact]
        public void EarlyRedemptionPenalty_CanBeSetToNullOrValue()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.EarlyRedemptionPenalty = 0.5m;
            Assert.Equal(0.5m, entity.EarlyRedemptionPenalty);

            entity.EarlyRedemptionPenalty = null;
            Assert.Null(entity.EarlyRedemptionPenalty);
        }

        #endregion

        #region Status & Audit Properties Tests

        [Fact]
        public void Status_CanBeSetToVariousStatuses()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act & Assert
            entity.Status = SecurityStatus.Active;
            Assert.Equal(SecurityStatus.Active, entity.Status);

            entity.Status = SecurityStatus.Matured;
            Assert.Equal(SecurityStatus.Matured, entity.Status);

            entity.Status = SecurityStatus.Cancelled;
            Assert.Equal(SecurityStatus.Cancelled, entity.Status);
        }

        [Fact]
        public void CreatedAt_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var createdAt = DateTime.Now;

            // Act
            entity.CreatedAt = createdAt;

            // Assert
            Assert.Equal(createdAt, entity.CreatedAt);
        }

        [Fact]
        public void UpdatedAt_CanBeSetAndRetrieved()
        {
            // Arrange
            var entity = new CreateSecuritEntity();
            var updatedAt = DateTime.Now;

            // Act
            entity.UpdatedAt = updatedAt;

            // Assert
            Assert.Equal(updatedAt, entity.UpdatedAt);
        }

        #endregion

        #region Complex Scenarios Tests

        [Fact]
        public void CreateSecuritEntity_CanBeModifiedAfterCreation()
        {
            // Arrange
            var entity = new CreateSecuritEntity
            {
                ISIN = "BRCDBCDB0001",
                Name = "Initial Name",
                FaceValue = 1000m
            };

            // Act
            entity.Name = "Updated Name";
            entity.FaceValue = 1500m;

            // Assert
            Assert.Equal("BRCDBCDB0001", entity.ISIN);
            Assert.Equal("Updated Name", entity.Name);
            Assert.Equal(1500m, entity.FaceValue);
        }

        [Fact]
        public void CreateSecuritEntity_SupportsAllEnumTypes()
        {
            // Arrange
            var entity = new CreateSecuritEntity();

            // Act
            entity.Type = SecurityType.LCA;
            entity.Indexer = IndexerType.IGPM;
            entity.RateType = RateType.SpreadOverIndexer;
            entity.PaymentFrequency = InterestPaymentFrequency.Quarterly;
            entity.Liquidity = LiquidityType.SecondaryMarket;
            entity.Status = SecurityStatus.Suspended;

            // Assert
            Assert.Equal(SecurityType.LCA, entity.Type);
            Assert.Equal(IndexerType.IGPM, entity.Indexer);
            Assert.Equal(RateType.SpreadOverIndexer, entity.RateType);
            Assert.Equal(InterestPaymentFrequency.Quarterly, entity.PaymentFrequency);
            Assert.Equal(LiquidityType.SecondaryMarket, entity.Liquidity);
            Assert.Equal(SecurityStatus.Suspended, entity.Status);
        }

        #endregion
    }
}
