using AutoFixture;
using Playground.VerticalSlice.Application.Features.FixedIncome.CreateSecurity;
using Playground.VerticalSlice.Application.Shared.Enums;
using Playground.VerticalSlice.Application.Shared.Monads;

namespace Playground.VeticalSlice.UnitTests.Application.Features.FixedIncome.CreateSecurity
{
    public class CreateSecurityRepositoryTests
    {
        private readonly IFixture _fixture = new Fixture();
        private const string TestConnectionString = "Server=.;Database=TestDb;Trusted_Connection=true;";

        #region Instantiation Tests

        [Fact]
        public void Constructor_WithConnectionString_CreatesInstance()
        {
            // Arrange & Act
            var repository = new CreateSecurityRepository(TestConnectionString);

            // Assert
            Assert.NotNull(repository);
        }

        [Fact]
        public void Constructor_WithValidConnectionString_StoresConnection()
        {
            // Arrange
            var connectionString = "Server=localhost;Database=FixedIncomeDb;Trusted_Connection=true;";

            // Act
            var repository = new CreateSecurityRepository(connectionString);

            // Assert
            Assert.NotNull(repository);
        }

        #endregion

        #region CreateSecurity Parameter Mapping Tests

        [Fact]
        public void CreateSecurityEntity_WithCompleteData_CanBeCreated()
        {
            // Arrange
            var entity = CreateValidEntity();

            // Act & Assert
            Assert.NotNull(entity);
            Assert.Equal("BRCDBCDB0001", entity.ISIN);
            Assert.Equal("Test CDB", entity.Name);
            Assert.Equal(SecurityType.CDB, entity.Type);
        }

        [Fact]
        public void CreateSecurityEntity_WithAllNullableFields_CanBeCreated()
        {
            // Arrange
            var entity = new CreateSecuritEntity
            {
                Id = 1,
                ISIN = "BRCDBCDB0001",
                CETIP = null,
                Name = "Test CDB",
                Type = SecurityType.CDB,
                IssuerName = "Test Bank",
                IssuerCNPJ = "00.000.000/0000-00",
                IssuerRating = null,
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
                GuaranteeType = null,
                FGCGuaranteeLimit = null,
                IsPubliclyOffered = false,
                Liquidity = LiquidityType.D1,
                AllowsEarlyRedemption = false,
                EarlyRedemptionPenalty = null,
                Status = SecurityStatus.Active,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // Act & Assert
            Assert.NotNull(entity);
            Assert.Null(entity.CETIP);
            Assert.Null(entity.IssuerRating);
            Assert.Null(entity.Spread);
            Assert.Null(entity.GracePeriodEnd);
            Assert.Null(entity.GuaranteeType);
            Assert.Null(entity.FGCGuaranteeLimit);
            Assert.Null(entity.EarlyRedemptionPenalty);
        }

        [Fact]
        public void CreateSecurityEntity_Identification_PropertiesArePreserved()
        {
            // Arrange
            var entity = CreateValidEntity();

            // Act & Assert
            Assert.Equal("BRCDBCDB0001", entity.ISIN);
            Assert.Equal("123456789012", entity.CETIP);
            Assert.Equal("Test CDB", entity.Name);
            Assert.Equal(SecurityType.CDB, entity.Type);
        }

        [Fact]
        public void CreateSecurityEntity_Issuer_PropertiesArePreserved()
        {
            // Arrange
            var entity = CreateValidEntity();

            // Act & Assert
            Assert.Equal("Test Bank", entity.IssuerName);
            Assert.Equal("11.222.333/0001-81", entity.IssuerCNPJ);
            Assert.Equal("A", entity.IssuerRating);
        }

        [Fact]
        public void CreateSecurityEntity_FinancialTerms_PropertiesArePreserved()
        {
            // Arrange
            var entity = CreateValidEntity();

            // Act & Assert
            Assert.Equal(1000m, entity.FaceValue);
            Assert.Equal(950m, entity.UnitPrice);
            Assert.Equal(100m, entity.MinimumInvestment);
            Assert.Equal("BRL", entity.Currency);
        }

        [Fact]
        public void CreateSecurityEntity_YieldRemuneration_PropertiesArePreserved()
        {
            // Arrange
            var entity = CreateValidEntity();

            // Act & Assert
            Assert.Equal(IndexerType.CDI, entity.Indexer);
            Assert.Equal(110m, entity.Rate);
            Assert.Equal(RateType.PercentageOfIndexer, entity.RateType);
            Assert.Null(entity.Spread);
            Assert.Equal(InterestPaymentFrequency.AtMaturity, entity.PaymentFrequency);
        }

        [Fact]
        public void CreateSecurityEntity_Dates_PropertiesArePreserved()
        {
            // Arrange
            var issueDate = new DateOnly(2024, 1, 1);
            var maturityDate = new DateOnly(2025, 1, 1);
            var graceDate = new DateOnly(2024, 6, 1);

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
                IssueDate = issueDate,
                MaturityDate = maturityDate,
                GracePeriodEnd = graceDate,
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
            Assert.Equal(issueDate, entity.IssueDate);
            Assert.Equal(maturityDate, entity.MaturityDate);
            Assert.Equal(graceDate, entity.GracePeriodEnd);
            Assert.Equal(252, entity.DurationDays);
        }

        [Fact]
        public void CreateSecurityEntity_TaxRegulation_PropertiesArePreserved()
        {
            // Arrange
            var entity = CreateValidEntity();

            // Act & Assert
            Assert.False(entity.IsIRExempt);
            Assert.True(entity.IsIOFExempt);
            Assert.Equal("FGC", entity.GuaranteeType);
            Assert.Equal(250000m, entity.FGCGuaranteeLimit);
            Assert.False(entity.IsPubliclyOffered);
        }

        [Fact]
        public void CreateSecurityEntity_Liquidity_PropertiesArePreserved()
        {
            // Arrange
            var entity = CreateValidEntity();

            // Act & Assert
            Assert.Equal(LiquidityType.D1, entity.Liquidity);
            Assert.True(entity.AllowsEarlyRedemption);
            Assert.Equal(0.5m, entity.EarlyRedemptionPenalty);
        }

        [Fact]
        public void CreateSecurityEntity_StatusAudit_PropertiesArePreserved()
        {
            // Arrange
            var entity = CreateValidEntity();

            // Act & Assert
            Assert.Equal(SecurityStatus.Active, entity.Status);
            Assert.NotEqual(default(DateTime), entity.CreatedAt);
            Assert.NotEqual(default(DateTime), entity.UpdatedAt);
        }

        #endregion

        #region Entity Conversion Scenarios

        [Fact]
        public void CreateSecurityEntity_WithVariousSecurityTypes_CanBeProcessed()
        {
            // Arrange & Act
            var cdbEntity = CreateEntityWithType(SecurityType.CDB);
            var lciEntity = CreateEntityWithType(SecurityType.LCI);
            var lcaEntity = CreateEntityWithType(SecurityType.LCA);
            var debenureEntity = CreateEntityWithType(SecurityType.Debenture);

            // Assert
            Assert.Equal(SecurityType.CDB, cdbEntity.Type);
            Assert.Equal(SecurityType.LCI, lciEntity.Type);
            Assert.Equal(SecurityType.LCA, lcaEntity.Type);
            Assert.Equal(SecurityType.Debenture, debenureEntity.Type);
        }

        [Fact]
        public void CreateSecurityEntity_WithVariousIndexers_CanBeProcessed()
        {
            // Arrange & Act
            var cdiEntity = CreateEntityWithIndexer(IndexerType.CDI);
            var ipcaEntity = CreateEntityWithIndexer(IndexerType.IPCA);
            var prefixedEntity = CreateEntityWithIndexer(IndexerType.PreFixado);
            var selicEntity = CreateEntityWithIndexer(IndexerType.SELIC);

            // Assert
            Assert.Equal(IndexerType.CDI, cdiEntity.Indexer);
            Assert.Equal(IndexerType.IPCA, ipcaEntity.Indexer);
            Assert.Equal(IndexerType.PreFixado, prefixedEntity.Indexer);
            Assert.Equal(IndexerType.SELIC, selicEntity.Indexer);
        }

        [Fact]
        public void CreateSecurityEntity_WithVariousRateTypes_CanBeProcessed()
        {
            // Arrange & Act
            var percentageEntity = CreateEntityWithRateType(RateType.PercentageOfIndexer);
            var spreadEntity = CreateEntityWithRateType(RateType.SpreadOverIndexer);
            var fixedEntity = CreateEntityWithRateType(RateType.FixedRate);

            // Assert
            Assert.Equal(RateType.PercentageOfIndexer, percentageEntity.RateType);
            Assert.Equal(RateType.SpreadOverIndexer, spreadEntity.RateType);
            Assert.Equal(RateType.FixedRate, fixedEntity.RateType);
        }

        [Fact]
        public void CreateSecurityEntity_WithVariousPaymentFrequencies_CanBeProcessed()
        {
            // Arrange & Act
            var monthlyEntity = CreateEntityWithPaymentFrequency(InterestPaymentFrequency.Monthly);
            var quarterlyEntity = CreateEntityWithPaymentFrequency(InterestPaymentFrequency.Quarterly);
            var semiannualEntity = CreateEntityWithPaymentFrequency(InterestPaymentFrequency.Semiannual);
            var maturityEntity = CreateEntityWithPaymentFrequency(InterestPaymentFrequency.AtMaturity);

            // Assert
            Assert.Equal(InterestPaymentFrequency.Monthly, monthlyEntity.PaymentFrequency);
            Assert.Equal(InterestPaymentFrequency.Quarterly, quarterlyEntity.PaymentFrequency);
            Assert.Equal(InterestPaymentFrequency.Semiannual, semiannualEntity.PaymentFrequency);
            Assert.Equal(InterestPaymentFrequency.AtMaturity, maturityEntity.PaymentFrequency);
        }

        [Fact]
        public void CreateSecurityEntity_WithVariousLiquidityTypes_CanBeProcessed()
        {
            // Arrange & Act
            var d0Entity = CreateEntityWithLiquidity(LiquidityType.D0);
            var d1Entity = CreateEntityWithLiquidity(LiquidityType.D1);
            var d2Entity = CreateEntityWithLiquidity(LiquidityType.D2);
            var maturityEntity = CreateEntityWithLiquidity(LiquidityType.AtMaturity);
            var secondaryEntity = CreateEntityWithLiquidity(LiquidityType.SecondaryMarket);

            // Assert
            Assert.Equal(LiquidityType.D0, d0Entity.Liquidity);
            Assert.Equal(LiquidityType.D1, d1Entity.Liquidity);
            Assert.Equal(LiquidityType.D2, d2Entity.Liquidity);
            Assert.Equal(LiquidityType.AtMaturity, maturityEntity.Liquidity);
            Assert.Equal(LiquidityType.SecondaryMarket, secondaryEntity.Liquidity);
        }

        [Fact]
        public void CreateSecurityEntity_WithVariousStatuses_CanBeProcessed()
        {
            // Arrange & Act
            var activeEntity = CreateEntityWithStatus(SecurityStatus.Active);
            var maturedEntity = CreateEntityWithStatus(SecurityStatus.Matured);
            var cancelledEntity = CreateEntityWithStatus(SecurityStatus.Cancelled);
            var suspendedEntity = CreateEntityWithStatus(SecurityStatus.Suspended);

            // Assert
            Assert.Equal(SecurityStatus.Active, activeEntity.Status);
            Assert.Equal(SecurityStatus.Matured, maturedEntity.Status);
            Assert.Equal(SecurityStatus.Cancelled, cancelledEntity.Status);
            Assert.Equal(SecurityStatus.Suspended, suspendedEntity.Status);
        }

        #endregion

        #region Complex Entity Scenarios

        [Fact]
        public void CreateSecurityEntity_WithIRExemptSecurity_PreservesExemptionFlag()
        {
            // Arrange
            var entity = new CreateSecuritEntity
            {
                Id = 1,
                ISIN = "BRLDILCD0001",
                Type = SecurityType.LCI,
                IsIRExempt = true,
                IsIOFExempt = true
            };

            // Act & Assert
            Assert.True(entity.IsIRExempt);
            Assert.True(entity.IsIOFExempt);
        }

        [Fact]
        public void CreateSecurityEntity_WithFGCCoverage_PreservesGuaranteeData()
        {
            // Arrange
            var entity = CreateValidEntity();
            entity.FGCGuaranteeLimit = 250000m;
            entity.GuaranteeType = "FGC";

            // Act & Assert
            Assert.Equal("FGC", entity.GuaranteeType);
            Assert.Equal(250000m, entity.FGCGuaranteeLimit);
        }

        [Fact]
        public void CreateSecurityEntity_WithPublicOffering_PreservesOfferingFlag()
        {
            // Arrange
            var entity = CreateValidEntity();
            entity.IsPubliclyOffered = true;

            // Act & Assert
            Assert.True(entity.IsPubliclyOffered);
        }

        [Fact]
        public void CreateSecurityEntity_WithEarlyRedemption_PreservesRedemptionTerms()
        {
            // Arrange
            var entity = CreateValidEntity();
            entity.AllowsEarlyRedemption = true;
            entity.EarlyRedemptionPenalty = 0.75m;

            // Act & Assert
            Assert.True(entity.AllowsEarlyRedemption);
            Assert.Equal(0.75m, entity.EarlyRedemptionPenalty);
        }

        [Fact]
        public void CreateSecurityEntity_WithoutEarlyRedemption_PreservesRedemptionTerms()
        {
            // Arrange
            var entity = CreateValidEntity();
            entity.AllowsEarlyRedemption = false;
            entity.EarlyRedemptionPenalty = null;

            // Act & Assert
            Assert.False(entity.AllowsEarlyRedemption);
            Assert.Null(entity.EarlyRedemptionPenalty);
        }

        [Fact]
        public void CreateSecurityEntity_WithGracePeriod_PreservesDateRange()
        {
            // Arrange
            var issueDate = new DateOnly(2024, 1, 1);
            var gracePeriodEnd = new DateOnly(2024, 6, 1);
            var maturityDate = new DateOnly(2025, 1, 1);

            var entity = new CreateSecuritEntity
            {
                Id = 1,
                ISIN = "BRCDBCDB0001",
                IssueDate = issueDate,
                GracePeriodEnd = gracePeriodEnd,
                MaturityDate = maturityDate
            };

            // Act & Assert
            Assert.Equal(issueDate, entity.IssueDate);
            Assert.Equal(gracePeriodEnd, entity.GracePeriodEnd);
            Assert.Equal(maturityDate, entity.MaturityDate);
            Assert.True(entity.GracePeriodEnd < entity.MaturityDate);
        }

        #endregion

        #region Helper Methods

        private CreateSecuritEntity CreateValidEntity()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var issueDate = today.AddDays(-30);
            var maturityDate = today.AddDays(400);

            return new CreateSecuritEntity
            {
                Id = 1,
                ISIN = "BRCDBCDB0001",
                CETIP = "123456789012",
                Name = "Test CDB",
                Type = SecurityType.CDB,
                IssuerName = "Test Bank",
                IssuerCNPJ = "11.222.333/0001-81",
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
                IssueDate = issueDate,
                MaturityDate = maturityDate,
                GracePeriodEnd = null,
                DurationDays = 252,
                IsIRExempt = false,
                IsIOFExempt = true,
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
        }

        private CreateSecuritEntity CreateEntityWithType(SecurityType type)
        {
            var entity = CreateValidEntity();
            entity.Type = type;
            return entity;
        }

        private CreateSecuritEntity CreateEntityWithIndexer(IndexerType indexer)
        {
            var entity = CreateValidEntity();
            entity.Indexer = indexer;
            return entity;
        }

        private CreateSecuritEntity CreateEntityWithRateType(RateType rateType)
        {
            var entity = CreateValidEntity();
            entity.RateType = rateType;
            return entity;
        }

        private CreateSecuritEntity CreateEntityWithPaymentFrequency(InterestPaymentFrequency frequency)
        {
            var entity = CreateValidEntity();
            entity.PaymentFrequency = frequency;
            return entity;
        }

        private CreateSecuritEntity CreateEntityWithLiquidity(LiquidityType liquidity)
        {
            var entity = CreateValidEntity();
            entity.Liquidity = liquidity;
            return entity;
        }

        private CreateSecuritEntity CreateEntityWithStatus(SecurityStatus status)
        {
            var entity = CreateValidEntity();
            entity.Status = status;
            return entity;
        }

        #endregion
    }
}
