namespace Infrastructure;

public static class Extensions
{
    private const string ListFormat = "dd/MM/yyyy";

    private const string InputFormat = "yyyy-MM-dd";

    private const string MoneyFormat = "#,##0.0000";

    private const string RateFormat = "#,##0.0000";

    public static T ToEnum<T>(this string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static string ToListFormat(this DateTime value)
    {
        return value.ToString(ListFormat);
    }

    public static string ToListFormat(this DateTime? value)
    {
        if (value.HasValue)
        {
            return value.Value.ToString(ListFormat);
        }
        return string.Empty;

    }

    public static string ToInputFormat(this DateTime value)
    {
        return value.ToString(InputFormat);
    }

    public static string ToMoneyFormat(this decimal value)
    {
        return value.ToString(MoneyFormat);
    }

    public static string ToRateFormat(this decimal value)
    {
        return value.ToString(RateFormat);
    }

    public static string ToPercentageFormat(this decimal value)
    {
        return $"{value} %";
    }
}