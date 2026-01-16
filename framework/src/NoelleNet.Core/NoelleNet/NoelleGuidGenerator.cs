namespace NoelleNet;

/// <summary>
/// <see cref="IGuidGenerator"/> 的默认实现
/// </summary>
public class NoelleGuidGenerator : IGuidGenerator
{
    /// <inheritdoc/>
    public Guid Generate()
    {
        return Guid.NewGuid();
    }
}
