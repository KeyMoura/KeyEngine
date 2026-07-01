using KeyEngine.Logging;

namespace KeyEngine.Tests.Logging;

public sealed class RuntimeLogManagerTests
{
    [Fact]
    public void Add_StoresRuntimeLogEntry()
    {
        RuntimeLogManager logs = new();

        RuntimeLogEntry entry = logs.Add(
            "Info",
            "Started",
            "Engine",
            "Test");

        RuntimeLogEntry stored = Assert.Single(logs.GetRecent());
        Assert.Same(entry, stored);
        Assert.Equal("Info", stored.Level);
        Assert.Equal("Started", stored.Message);
        Assert.Equal("Engine", stored.Category);
        Assert.Equal("Test", stored.Source);
    }

    [Fact]
    public void Add_RespectsCapacityByRemovingOldestEntries()
    {
        RuntimeLogManager logs = new(capacity: 2);

        logs.Add("Info", "First");
        logs.Add("Info", "Second");
        logs.Add("Info", "Third");

        IReadOnlyList<RuntimeLogEntry> recent = logs.GetRecent();
        Assert.Equal(2, recent.Count);
        Assert.Equal("Second", recent[0].Message);
        Assert.Equal("Third", recent[1].Message);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        RuntimeLogManager logs = new();
        logs.Add("Info", "Started");

        logs.Clear();

        Assert.Empty(logs.GetRecent());
    }
}
