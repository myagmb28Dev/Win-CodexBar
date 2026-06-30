namespace WindexBar.Core.Config;

public sealed record HotkeyShortcut(
    bool Control,
    bool Alt,
    bool Shift,
    bool Windows,
    string Key)
{
    public string DisplayText
    {
        get
        {
            var parts = new List<string>();
            if (Control)
            {
                parts.Add("Ctrl");
            }

            if (Alt)
            {
                parts.Add("Alt");
            }

            if (Shift)
            {
                parts.Add("Shift");
            }

            if (Windows)
            {
                parts.Add("Win");
            }

            parts.Add(Key);
            return string.Join("+", parts);
        }
    }

    public bool HasModifier => Control || Alt || Shift || Windows;

    public static string NormalizeOrDefault(string? value, string defaultValue)
    {
        if (TryParse(value, out var shortcut) && shortcut is not null)
        {
            return shortcut.DisplayText;
        }

        return TryParse(defaultValue, out var defaultShortcut) && defaultShortcut is not null
            ? defaultShortcut.DisplayText
            : WindexBarConfig.DefaultToggleWindowHotkey;
    }

    public static bool TryParse(string? value, out HotkeyShortcut? shortcut)
    {
        shortcut = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var control = false;
        var alt = false;
        var shift = false;
        var windows = false;
        string? key = null;

        foreach (var token in value.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var normalized = token.Trim();
            if (normalized.Length == 0)
            {
                continue;
            }

            switch (normalized.ToLowerInvariant())
            {
                case "ctrl":
                case "control":
                    control = true;
                    continue;
                case "alt":
                case "option":
                    alt = true;
                    continue;
                case "shift":
                    shift = true;
                    continue;
                case "win":
                case "windows":
                case "super":
                case "meta":
                    windows = true;
                    continue;
            }

            if (key is not null || !TryNormalizeKey(normalized, out key))
            {
                shortcut = null;
                return false;
            }
        }

        if (key is null)
        {
            return false;
        }

        shortcut = new HotkeyShortcut(control, alt, shift, windows, key);
        if (!shortcut.HasModifier)
        {
            shortcut = null;
            return false;
        }

        return true;
    }

    private static bool TryNormalizeKey(string value, out string key)
    {
        key = string.Empty;
        var normalized = value.Trim();
        if (normalized.Length == 1 && char.IsLetterOrDigit(normalized[0]))
        {
            key = normalized.ToUpperInvariant();
            return true;
        }

        var lower = normalized.ToLowerInvariant().Replace(" ", string.Empty);
        if (lower.Length is >= 2 and <= 3
            && lower[0] == 'f'
            && int.TryParse(lower[1..], out var functionKey)
            && functionKey is >= 1 and <= 24)
        {
            key = $"F{functionKey}";
            return true;
        }

        key = lower switch
        {
            "space" => "Space",
            "esc" or "escape" => "Escape",
            "tab" => "Tab",
            "enter" or "return" => "Enter",
            "backspace" => "Backspace",
            "insert" or "ins" => "Insert",
            "delete" or "del" => "Delete",
            "home" => "Home",
            "end" => "End",
            "pageup" or "pgup" => "PageUp",
            "pagedown" or "pgdn" => "PageDown",
            "up" => "Up",
            "down" => "Down",
            "left" => "Left",
            "right" => "Right",
            _ => string.Empty
        };
        return key.Length > 0;
    }
}
