using System.Text;

namespace PromptMarketPlace.Services;

public interface ICaptchaService
{
    string GenerateCode();
    byte[] GenerateImage(string code);
    void StoreCode(ISession session, string code);
    bool Validate(ISession session, string userInput);
}

public class CaptchaService : ICaptchaService
{
    private const string SessionKey = "_Captcha";
    private static readonly char[] Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
    private static readonly Random _rng = new();

    private static readonly string[] TextColors =
        { "#1e5ab4", "#aa3228", "#1e7846", "#783ca0", "#966414" };

    public string GenerateCode()
        => new(Enumerable.Range(0, 5).Select(_ => Chars[_rng.Next(Chars.Length)]).ToArray());

    public void StoreCode(ISession session, string code)
        => session.SetString(SessionKey, code);

    public bool Validate(ISession session, string userInput)
    {
        var stored = session.GetString(SessionKey);
        session.Remove(SessionKey);
        return !string.IsNullOrEmpty(stored) &&
               string.Equals(stored, userInput?.Trim().ToUpper(), StringComparison.Ordinal);
    }

    public byte[] GenerateImage(string code)
    {
        const int w = 160, h = 50;
        var sb = new StringBuilder(512);

        sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{w}' height='{h}'>");

        // پس‌زمینه با گرادیان ملایم
        sb.Append($"<defs><linearGradient id='bg' x1='0' y1='0' x2='0' y2='1'>");
        sb.Append("<stop offset='0%' stop-color='#f0f5ff'/>");
        sb.Append("<stop offset='100%' stop-color='#dce6f8'/>");
        sb.Append("</linearGradient></defs>");
        sb.Append($"<rect width='{w}' height='{h}' fill='url(#bg)' rx='4'/>");

        // خطوط نویز منحنی
        for (int i = 0; i < 5; i++)
        {
            int x1 = _rng.Next(w / 3), y1 = _rng.Next(h);
            int cx1 = _rng.Next(w / 2), cy1 = _rng.Next(h);
            int cx2 = _rng.Next(w / 2, w), cy2 = _rng.Next(h);
            int x2 = _rng.Next(w / 2, w), y2 = _rng.Next(h);
            int r = _rng.Next(130, 190), g = _rng.Next(130, 190), b = _rng.Next(180, 255);
            sb.Append($"<path d='M{x1},{y1} C{cx1},{cy1} {cx2},{cy2} {x2},{y2}' " +
                      $"stroke='rgba({r},{g},{b},0.45)' stroke-width='1.5' fill='none'/>");
        }

        // رسم کاراکترها
        float x = 14;
        for (int i = 0; i < code.Length; i++)
        {
            int angle = _rng.Next(-18, 19);
            int cy = 30 + _rng.Next(-3, 4);
            int fontSize = 20 + _rng.Next(-2, 4);
            float cx = x + 12;
            string color = TextColors[i % TextColors.Length];
            sb.Append($"<text x='{cx:F0}' y='{cy}' " +
                      $"font-family='Arial,Tahoma,Verdana,sans-serif' " +
                      $"font-size='{fontSize}' font-weight='bold' fill='{color}' " +
                      $"transform='rotate({angle},{cx:F0},{cy})'>{code[i]}</text>");
            x += 27;
        }

        // نقاط نویز
        for (int i = 0; i < 30; i++)
        {
            int dx = _rng.Next(w), dy = _rng.Next(h);
            int r = _rng.Next(256), g = _rng.Next(256), b = _rng.Next(256);
            int size = _rng.Next(1, 3);
            sb.Append($"<circle cx='{dx}' cy='{dy}' r='{size}' fill='rgba({r},{g},{b},0.4)'/>");
        }

        sb.Append("</svg>");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
