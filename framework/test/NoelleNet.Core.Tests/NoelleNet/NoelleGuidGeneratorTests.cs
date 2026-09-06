namespace NoelleNet;

/// <summary>
/// <see cref="NoelleGuidGenerator"/> 的单元测试
/// </summary>
public class NoelleGuidGeneratorTests
{
    /// <summary>
    /// 生成的是 .NET 9 的有序 GUID（版本 7），利于数据库索引
    /// </summary>
    [Fact]
    public void Generate_ShouldReturnVersion7Guid()
    {
        var guid = new NoelleGuidGenerator().Generate();

        var versionByte = guid.ToByteArray()[7];
        var version = (versionByte >> 4) & 0x0F;
        Assert.Equal(7, version);
    }

    /// <summary>
    /// 多次生成应返回互不相同的 GUID
    /// </summary>
    [Fact]
    public void Generate_MultipleCalls_ShouldReturnUniqueGuids()
    {
        var generator = new NoelleGuidGenerator();
        var guids = Enumerable.Range(0, 100).Select(_ => generator.Generate()).ToHashSet();

        Assert.Equal(100, guids.Count);
    }
}
