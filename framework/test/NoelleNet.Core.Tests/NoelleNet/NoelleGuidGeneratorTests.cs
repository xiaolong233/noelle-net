namespace NoelleNet;

public class NoelleGuidGeneratorTests
{
    [Fact]
    public void Generate_ShouldReturnNonEmptyGuid()
    {
        var generator = new NoelleGuidGenerator();
        var guid = generator.Generate();

        Assert.NotEqual(Guid.Empty, guid);
    }

    [Fact]
    public void Generate_ShouldReturnVersion7Guid()
    {
        var generator = new NoelleGuidGenerator();
        var guid = generator.Generate();

        // Version 7 GUID has version nibble set to 7
        var versionByte = guid.ToByteArray()[7];
        var version = (versionByte >> 4) & 0x0F;
        Assert.Equal(7, version);
    }

    [Fact]
    public void Generate_MultipleCalls_ShouldReturnUniqueGuids()
    {
        var generator = new NoelleGuidGenerator();
        var guids = Enumerable.Range(0, 100).Select(_ => generator.Generate()).ToHashSet();

        Assert.Equal(100, guids.Count);
    }

    [Fact]
    public void NoelleGuidGenerator_ShouldImplementIGuidGenerator()
    {
        var generator = new NoelleGuidGenerator();
        Assert.IsAssignableFrom<IGuidGenerator>(generator);
    }
}
