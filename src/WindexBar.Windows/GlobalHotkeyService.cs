using System.ComponentModel;
using System.Runtime.InteropServices;
using WindexBar.Core.Config;
using Forms = System.Windows.Forms;

namespace WindexBar.Windows;

internal sealed class GlobalHotkeyService : IDisposable
{
    private const int HotkeyId = 0x5742;
    private const int WindowHotkeyMessage = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModWin = 0x0008;
    private const uint ModNoRepeat = 0x4000;

    private readonly HotkeyMessageWindow _window;
    private bool _registered;
    private bool _disposed;

    public GlobalHotkeyService(Action onPressed)
    {
        _window = new HotkeyMessageWindow(() =>
        {
            if (!_disposed)
            {
                onPressed();
            }
        });
    }

    public bool Register(string shortcutText, out string? error)
    {
        error = null;
        Unregister();

        if (!HotkeyShortcut.TryParse(shortcutText, out var shortcut) || shortcut is null)
        {
            error = "Invalid shortcut.";
            return false;
        }

        if (!TryGetVirtualKey(shortcut.Key, out var virtualKey))
        {
            error = "Unsupported shortcut key.";
            return false;
        }

        var modifiers = ModNoRepeat;
        if (shortcut.Alt)
        {
            modifiers |= ModAlt;
        }

        if (shortcut.Control)
        {
            modifiers |= ModControl;
        }

        if (shortcut.Shift)
        {
            modifiers |= ModShift;
        }

        if (shortcut.Windows)
        {
            modifiers |= ModWin;
        }

        if (!RegisterHotKey(_window.Handle, HotkeyId, modifiers, virtualKey))
        {
            error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            return false;
        }

        _registered = true;
        return true;
    }

    public void Unregister()
    {
        if (!_registered)
        {
            return;
        }

        _ = UnregisterHotKey(_window.Handle, HotkeyId);
        _registered = false;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Unregister();
        _window.Dispose();
    }

    private static bool TryGetVirtualKey(string key, out uint virtualKey)
    {
        virtualKey = 0;
        if (key.Length == 1 && char.IsLetterOrDigit(key[0]))
        {
            virtualKey = char.ToUpperInvariant(key[0]);
            return true;
        }

        if (key.Length is >= 2 and <= 3
            && key[0] == 'F'
            && int.TryParse(key[1..], out var functionKey)
            && functionKey is >= 1 and <= 24)
        {
            virtualKey = (uint)(0x70 + functionKey - 1);
            return true;
        }

        virtualKey = key switch
        {
            "Space" => 0x20,
            "Escape" => 0x1B,
            "Tab" => 0x09,
            "Enter" => 0x0D,
            "Backspace" => 0x08,
            "Insert" => 0x2D,
            "Delete" => 0x2E,
            "Home" => 0x24,
            "End" => 0x23,
            "PageUp" => 0x21,
            "PageDown" => 0x22,
            "Up" => 0x26,
            "Down" => 0x28,
            "Left" => 0x25,
            "Right" => 0x27,
            _ => 0
        };
        return virtualKey != 0;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private sealed class HotkeyMessageWindow : Forms.NativeWindow, IDisposable
    {
        private readonly Action _onPressed;

        public HotkeyMessageWindow(Action onPressed)
        {
            _onPressed = onPressed;
            CreateHandle(new Forms.CreateParams { Caption = "WindexBarHotkey" });
        }

        protected override void WndProc(ref Forms.Message message)
        {
            if (message.Msg == WindowHotkeyMessage && message.WParam.ToInt32() == HotkeyId)
            {
                _onPressed();
                return;
            }

            base.WndProc(ref message);
        }

        public void Dispose() => DestroyHandle();
    }
}
