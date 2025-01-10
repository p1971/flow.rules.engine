using Xunit;

namespace FlowRules.Engine.UnitTests
{
    public class NestedLookupTests
    {
        [Fact]
        public void CanLookupSingleKeyValue()
        {
            NestedLookup<string, object> lookups = new(
                [
                    (["Key1"], 123),
                    (["Key2"], 456)
                ]
            );

            Assert.Equal(123, lookups["Key1"]);
            Assert.Equal(456, lookups["Key2"]);
        }

        [Fact]
        public void CanLookupTwoKeyValue()
        {
            NestedLookup<string, object> lookups = new(
                [
                    (["Key11", "Key12"], 123),
                    (["Key21", "Key22"], 456)
                ]
            );

            Assert.Equal(123, lookups["Key11"]["Key12"]);
            Assert.Equal(456, lookups["Key21"]["Key22"]);
        }

        [Fact]
        public void CanLookupThreeKeyValue()
        {
            NestedLookup<string, object> lookups = new(
                [
                    (["Key11", "Key12", "Key13"], 123),
                    (["Key21", "Key22", "Key23"], 456)
                ]
            );

            Assert.Equal(123, lookups["Key11"]["Key12"]["Key13"]);
            Assert.Equal(456, lookups["Key21"]["Key22"]["Key23"]);
        }

        [Fact]
        public void T()
        {
            NestedLookup<string, object> lookups = new(
                [
                    // First Time Buyer rules
                    (["Default", "FTB", "MinLoan"], 100_000),
                    (["Default", "FTB", "MaxLoan"], 420_000),
                    (["Default", "FTB", "MinApplicantAge"], 25),
                    (["Default", "FTB", "MinLTV"], 95.0),
                    (["Default", "FTB", "MinDSCR"], 50),
                    (["Default", "FTB", "InterestRateDSCR"], 0.95),
                    // Buy to Let rules
                    (["Default", "BTL", "MinLoan"], 200_000),
                    (["Default", "BTL", "MaxLoan"], 2_000_000),
                    (["Default", "BTL", "MinApplicantAge"], 30),
                    (["Default", "BTL", "MinLTV"], 75.0),
                    (["Default", "BTL", "MinDSCR"], 50),
                    (["Default", "BTL", "InterestRateDSCR"], 0.95),
                    (["Default", "BTL", "String"], "test"),
                ]
            );

            Assert.Equal(100_000, lookups["Default"]["FTB"]["MinLoan"]);
            Assert.Equal(0, lookups["Default"]["FTB"]["MinLoanxxx"]);
            Assert.Equal("test", lookups["Default"]["BTL"]["String"]);
            
            Assert.False(lookups["Default"].IsDefined("XXX"));
            Assert.True(lookups["Default"].IsDefined("BTL"));
        }
    }
}
