namespace DigitalMenu.Core.ValueObjects;

public class ColorPalette
{
    public string Primary { get; set; } = "#2563eb";

    public string Secondary { get; set; } = "#64748b";

    public string Background { get; set; } = "#ffffff";

    public string Text { get; set; } = "#0f172a";

    public string Accent { get; set; } = "#f59e0b";

    public static ColorPalette Default => new();
}
