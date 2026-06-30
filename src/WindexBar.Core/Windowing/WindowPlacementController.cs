namespace WindexBar.Core.Windowing;

public readonly record struct WindowPosition(int X, int Y);

public sealed class WindowPlacementController
{
    private readonly WindowPosition _defaultPosition;
    private bool _hasAppliedInitialPosition;

    public WindowPlacementController(WindowPosition defaultPosition)
    {
        _defaultPosition = defaultPosition;
    }

    public WindowPosition PositionForResize(WindowPosition currentPosition)
    {
        if (_hasAppliedInitialPosition)
        {
            return currentPosition;
        }

        _hasAppliedInitialPosition = true;
        return _defaultPosition;
    }
}

public readonly record struct WindowActivationPlan(int X, int Y, int Width, int Height, uint Flags)
{
    public const uint NoSize = 0x0001;
    public const uint NoMove = 0x0002;
    public const uint ShowWindow = 0x0040;

    public static WindowActivationPlan PreserveCurrentBounds { get; } =
        new(0, 0, 0, 0, NoMove | NoSize | ShowWindow);

    public bool PreservesPosition => (Flags & NoMove) == NoMove;

    public bool PreservesSize => (Flags & NoSize) == NoSize;
}
