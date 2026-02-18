using System;
using System.Reflection;
using NUnit.Framework;

public class ReflectionUtilsTests : TestBase
{
    [Test]
    public void SetValue_Field_Works()
    {
        var s = new Sample();
        FieldInfo mi = typeof(Sample).GetField("PublicField");
        mi.SetValue(s, 42);
        Eq(42, s.PublicField);
    }

    [Test]
    public void SetValue_Property_Works()
    {
        var s = new Sample();
        PropertyInfo mi = typeof(Sample).GetProperty("PublicProp");
        mi.SetValue(s, 99);
        Eq(99, s.PublicProp);
    }

    [Test]
    public void GetValue_Field()
    {
        var s = new Sample { PublicField = 123 };
        FieldInfo fi = typeof(Sample).GetField("PublicField");
        Eq(123, fi.GetValue(s));
    }

    [Test]
    public void GetTypeByName_FindsType()
    {
        Type t = ReflectionUtils.GetTypeByName("Sample");
        NotNull(t);
        Eq(typeof(Sample), t);
    }

    [Test]
    public void AllInstance_FindsPrivateFields()
    {
        FieldInfo fi = typeof(Sample).GetField("_privateField", ReflectionUtils.AllInstance);
        NotNull(fi);
    }

    private class Sample
    {
        private int _privateField;
        public int PublicField;
        public int PublicProp { get; set; }
        public int GetPrivate() => _privateField;
        public void SetPrivate(int v) => _privateField = v;
    }
}