namespace NoelleNet.Utils;

/// <summary>
/// 身份证号码工具类
/// </summary>
public static class IdNumberUtil
{
    // 加权因子
    private static readonly int[] _weightFactors = [7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2];

    // 校验码对应表
    private static readonly char[] _checkCodes = ['1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2'];

    /// <summary>
    /// 验证身份证号是否有效
    /// </summary>
    /// <param name="idCardNumber">身份证号码（支持15位和18位）</param>
    /// <returns></returns>
    public static bool Validate(string? idCardNumber)
    {
        if (string.IsNullOrWhiteSpace(idCardNumber))
            return false;

        if (idCardNumber.Length == 15)
        {
            // 将15位身份证转换为18位
            idCardNumber = ConvertTo18DigitIdCard(idCardNumber);
        }

        if (idCardNumber.Length != 18)
        {
            return false;
        }

        // 计算校验码
        char expectedCheckCode = CalculateCheckCode(idCardNumber[..17]);

        // 比较计算出的校验码与身份证号的第18位
        return expectedCheckCode == idCardNumber[17];
    }

    /// <summary>
    /// 把15位的身份证号码转换成18位的
    /// </summary>
    /// <param name="idCardNumber">15位的身份证号码</param>
    /// <returns>18位的身份证号码</returns>
    public static string ConvertTo18DigitIdCard(string? idCardNumber)
    {
        if (string.IsNullOrWhiteSpace(idCardNumber))
            throw new ArgumentException("身份证号码为空");
        if (idCardNumber.Length != 15)
            throw new ArgumentException("身份证号码的长度必须是15位");

        // 插入"19"表示出生年份
        string idCard18 = $"{idCardNumber[..6]}19{idCardNumber[6..]}";

        // 计算校验码
        char checkCode = CalculateCheckCode(idCard18);

        // 插入校验码并返回
        return $"{idCard18}{checkCode}";
    }

    /// <summary>
    /// 计算校验码
    /// </summary>
    /// <param name="idCardWithoutCheckCode">不包含校验码的码身份证号码部分</param>
    /// <returns>校验码</returns>
    private static char CalculateCheckCode(string idCardWithoutCheckCode)
    {
        int sum = 0;
        for (int i = 0; i < 17; i++)
        {
            sum += (idCardWithoutCheckCode[i] - '0') * _weightFactors[i];
        }

        int mod = sum % 11;
        return _checkCodes[mod];
    }

    /// <summary>
    /// 获取出生日期
    /// </summary>
    /// <param name="idCardNumber">身份证号码（支持15位和18位）</param>
    /// <returns></returns>
    public static DateTime? GetBirthday(string? idCardNumber)
    {
        if (string.IsNullOrWhiteSpace(idCardNumber))
            return null;

        if (idCardNumber.Length == 15)
        {
            idCardNumber = ConvertTo18DigitIdCard(idCardNumber);
        }

        if (!Validate(idCardNumber))
            return null;

        string birthDateStr = idCardNumber.Substring(6, 8);
        return DateTime.TryParseExact(birthDateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime birthDate) ? birthDate : null;
    }

    /// <summary>
    /// 获取性别
    /// </summary>
    /// <param name="idCardNumber">身份证号码（支持15位和18位）</param>
    /// <returns>0-未知、1-男、2-女</returns>
    public static int GetGender(string? idCardNumber)
    {
        if (string.IsNullOrWhiteSpace(idCardNumber))
            return 0;

        if (idCardNumber.Length == 15)
        {
            idCardNumber = ConvertTo18DigitIdCard(idCardNumber);
        }

        if (!Validate(idCardNumber))
            return 0;

        int genderCode = int.Parse(idCardNumber.Substring(16, 1));
        return genderCode % 2 == 0 ? 2 : 1;
    }

    /// <summary>
    /// 获取地区代码
    /// </summary>
    /// <param name="idCardNumber">身份证号码（支持15位和18位）</param>
    /// <returns>地区代码</returns>
    public static string? GetRegionCode(string? idCardNumber)
    {
        if (!Validate(idCardNumber))
            return null;
        return idCardNumber![..6];
    }
}
