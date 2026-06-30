using KeyEngine.Resources;

namespace KeyEngine.Tests.Resources;

public sealed class ResourceLocationTests
{
    [Fact]
    public void Constructor_ValidScheme_NormalizesSchemeAndPreservesIdentifier()
    {
        ResourceLocation location = new("  Embedded+Plugin  ", "icons/main");

        Assert.Equal("embedded+plugin", location.Scheme);
        Assert.Equal("icons/main", location.Identifier);
        Assert.Equal("embedded+plugin://icons/main", location.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("1file")]
    [InlineData("file_name")]
    [InlineData("file/path")]
    public void Constructor_InvalidScheme_ThrowsArgumentException(string scheme)
    {
        Assert.Throws<ArgumentException>(() =>
            new ResourceLocation(scheme, "resource"));
    }
}
