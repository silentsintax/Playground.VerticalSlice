namespace Playground.VerticalSlice.Application.Shared.Enums
{
    public enum SecurityType
    {
        CDB, LCI, LCA, CRI, CRA,
        Debenture, LC, LF,
        TesouroPrefixado, TesouroSelic, TesouroIPCA
    }

    public enum IndexerType { CDI, SELIC, IPCA, IGPM, INPC, PreFixado, TR, USD }

    public enum RateType { PercentageOfIndexer, SpreadOverIndexer, FixedRate }

    public enum InterestPaymentFrequency { AtMaturity, Monthly, Quarterly, Semiannual, Annual }

    public enum LiquidityType { D0, D1, D2, AtMaturity, SecondaryMarket }

    public enum SecurityStatus { Active, Matured, Cancelled, Suspended }
}
