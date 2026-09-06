namespace NoelleNet.Application.Dtos;

/// <summary>
/// 实现了 <see cref="IPaging"/> 的数据传输对象。
/// </summary>
public class PagingDto : LimitDto, IPaging
{
    private int _offset;

    /// <inheritdoc/>
    public virtual int Offset
    {
        get => _offset;
        set => _offset = value < 0 ? 0 : value;
    }

    /// <inheritdoc/>
    public string? Sort { get; set; }
}
