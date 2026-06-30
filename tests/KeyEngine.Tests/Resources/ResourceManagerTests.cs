using KeyEngine.Resources;

namespace KeyEngine.Tests.Resources;

public sealed class ResourceManagerTests
{
    [Fact]
    public void Load_DispatchesByExactResourceTypeAndNormalizedScheme()
    {
        using ResourceManager resources = new();
        TestLoader<TextResource> fileTextLoader = new(new TextResource("file"));
        TestLoader<TextResource> memoryTextLoader = new(new TextResource("memory"));
        TestLoader<BinaryResource> fileBinaryLoader = new(new BinaryResource());

        resources.Register("file", fileTextLoader);
        resources.Register("memory", memoryTextLoader);
        resources.Register("file", fileBinaryLoader);

        ResourceLocation memoryLocation = new("MEMORY", "message");
        ResourceHandle<TextResource> memoryText =
            resources.Load<TextResource>(memoryLocation);
        ResourceHandle<TextResource> cachedMemoryText =
            resources.Load<TextResource>(memoryLocation);
        ResourceHandle<TextResource> fileText = resources.Load<TextResource>(
            new ResourceLocation("file", "message"));
        ResourceHandle<BinaryResource> binary = resources.Load<BinaryResource>(
            new ResourceLocation("file", "payload"));

        Assert.Same(memoryText, cachedMemoryText);
        Assert.Equal("memory", memoryText.Resource.Source);
        Assert.Equal("file", fileText.Resource.Source);
        Assert.Same(fileBinaryLoader.Resource, binary.Resource);
        Assert.Equal(1, fileTextLoader.LoadCount);
        Assert.Equal(1, memoryTextLoader.LoadCount);
        Assert.Equal(1, fileBinaryLoader.LoadCount);
    }

    private sealed record TextResource(string Source);

    private sealed class BinaryResource;

    private sealed class TestLoader<T>(T resource)
        : IResourceLoader<T>
        where T : class
    {
        public T Resource { get; } = resource;

        public int LoadCount { get; private set; }

        public T Load(ResourceLocation location)
        {
            LoadCount++;
            return Resource;
        }
    }
}
