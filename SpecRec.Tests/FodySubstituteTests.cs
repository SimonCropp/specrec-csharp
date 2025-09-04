using Substitute;

[assembly: Substitute(typeof(FodySubstituteTests.Target), typeof(FodySubstituteTests.Replace))]

public class FodySubstituteTests
{
    [Fact]
    public void Run()
    {
        var target = new Target();
        var result = target.Method();
        Assert.Equal("NewValue", result);
        Assert.Equal("NewValue", Target.StaticMethod());
    }

    public class Target
    {
        public string Method() => "Value";

        public static string StaticMethod() => "Value";
    }

    public class Replace
    {
        public string Method() => "NewValue";

        public static string StaticMethod() => "NewValue";
    }
}