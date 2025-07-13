namespace NoelleNet.AspNetCore.Queries;

/// <summary>
/// 分页查询参数的数据传输对象
/// </summary>
public class NoellePaginationDto
{
    private static int _defaultLimit = 10;
    private static int _maxLimit = 1000;

    /// <summary>
    /// 默认返回记录数（必须大于0且小于或等于MaxLimit），默认值：10
    /// </summary>
    public static int DefaultLimit
    {
        get { return _defaultLimit; }
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "DefaultLimit必须大于0。");
            if (value > _maxLimit)
                throw new ArgumentOutOfRangeException(nameof(value), value, $"DefaultLimit不能大于MaxLimit({_maxLimit})。");

            _defaultLimit = value;
        }
    }

    /// <summary>
    /// 最大返回记录数（必须大于0且大于或等于DefaultLimit），默认值：1000
    /// </summary>
    public static int MaxLimit
    {
        get { return _maxLimit; }
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "MaxLimit必须大于0。");
            if (value < _defaultLimit)
                throw new ArgumentOutOfRangeException(nameof(value), value, $"MaxLimit不能小于DefaultLimit({_defaultLimit})。");

            _maxLimit = value;
        }
    }

    private int _offset;
    /// <summary>
    /// 跳过的记录数
    /// </summary>
    public int Offset
    {
        get { return _offset; }
        set { _offset = Math.Max(value, 0); }
    }

    private int _limit = DefaultLimit;
    /// <summary>
    /// 返回的记录数
    /// </summary>
    public int Limit
    {
        get { return _limit; }
        set { _limit = value <= 0 ? DefaultLimit : (value > MaxLimit ? MaxLimit : value); }
    }
}