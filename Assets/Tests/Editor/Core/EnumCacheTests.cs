using NUnit.Framework;

enum TestEnum { Alpha, Beta, Gamma }

public class EnumCacheTests : TestBase
{
    [Test] public void GetName_ReturnsNonNull()
    {
        NotNull(EnumCache<TestEnum>.GetName(TestEnum.Alpha));
        NotNull(EnumCache<TestEnum>.GetName(TestEnum.Beta));
        NotNull(EnumCache<TestEnum>.GetName(TestEnum.Gamma));
    }

    [Test] public void GetName_ReturnsCorrectString()
    {
        Eq("Alpha", EnumCache<TestEnum>.GetName(TestEnum.Alpha));
        Eq("Beta", EnumCache<TestEnum>.GetName(TestEnum.Beta));
        Eq("Gamma", EnumCache<TestEnum>.GetName(TestEnum.Gamma));
    }

    [Test] public void Values_ContainsAll()
    {
        Eq(3, EnumCache<TestEnum>.Values.Length);
        H.Contains(EnumCache<TestEnum>.Values, TestEnum.Alpha);
        H.Contains(EnumCache<TestEnum>.Values, TestEnum.Beta);
        H.Contains(EnumCache<TestEnum>.Values, TestEnum.Gamma);
    }

    [Test] public void GetSummary_NotEmpty()
    {
        var summary = EnumCache<TestEnum>.GetSummary();
        NotNull(summary);
        True(summary.Length > 0);
    }
}
