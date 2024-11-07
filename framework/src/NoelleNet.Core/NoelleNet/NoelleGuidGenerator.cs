namespace NoelleNet;

/// <summary>
/// <see cref="IGuidGenerator"/> 的默认实现。
/// </summary>
public class NoelleGuidGenerator : IGuidGenerator
{
    public Guid Generate()
    {
        return Guid.NewGuid();
    }
}
