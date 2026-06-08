using System.Globalization;

namespace PromptMarketPlace.Helpers;

public static class PersianHelper
{
    private static readonly PersianCalendar Cal = new();

    public static string ToShamsi(this DateTime dt, string format = "yyyy/MM/dd")
    {
        var y = Cal.GetYear(dt);
        var m = Cal.GetMonth(dt);
        var d = Cal.GetDayOfMonth(dt);

        return format
            .Replace("yyyy", y.ToString("D4"))
            .Replace("yy",   (y % 100).ToString("D2"))
            .Replace("MM",   m.ToString("D2"))
            .Replace("dd",   d.ToString("D2"))
            .Replace("HH",   dt.Hour.ToString("D2"))
            .Replace("mm",   dt.Minute.ToString("D2"))
            .ToFarsiDigits();
    }

    public static string ToFarsiDigits(this string s) =>
        s.Replace('0','۰').Replace('1','۱').Replace('2','۲')
         .Replace('3','۳').Replace('4','۴').Replace('5','۵')
         .Replace('6','۶').Replace('7','۷').Replace('8','۸')
         .Replace('9','۹');

    public static string ToFarsiDigits(this int n)     => n.ToString().ToFarsiDigits();
    public static string ToFarsiDigits(this long n)    => n.ToString().ToFarsiDigits();
    public static string ToFarsiDigits(this double n, string fmt = "0.0") => n.ToString(fmt).ToFarsiDigits();
    public static string ToFarsiDigits(this decimal n, string fmt = "N0") => n.ToString(fmt).ToFarsiDigits();
    public static string N0Fa(this int n)     => n.ToString("N0").ToFarsiDigits();
    public static string N0Fa(this long n)    => n.ToString("N0").ToFarsiDigits();
    public static string N0Fa(this decimal n) => n.ToString("N0").ToFarsiDigits();
}
