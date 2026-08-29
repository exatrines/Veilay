using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Veilay.UI;

/// <summary>World pick via game/OS mouse state, not ImGui click.</summary>
internal static class WorldPick
{
    private const uint MarkerColor = 0xFF59D9FF;
    private const int VkLButton = 0x01;

    private static bool _waitRelease;
    private static bool _held;
    private static Vector3? _preview;

    internal static string? Id { get; private set; }

    internal static bool IsActive(string id) => Id == id;

    internal static void Toggle(string id)
    {
        if (Id == id)
        {
            Cancel();
            return;
        }

        Id = id;
        _waitRelease = true;
        _preview = null;
    }

    internal static void Cancel()
    {
        Id = null;
        _waitRelease = false;
        _held = false;
        _preview = null;
    }

    internal static void Draw()
    {
        if (Id == null)
            return;

        var held = IsLeftMouseHeld();
        var released = _held && !held;
        _held = held;

        if (_waitRelease)
        {
            if (!held)
                _waitRelease = false;
        }
        else if (released && !ImGui.GetIO().WantCaptureMouse && _preview.HasValue)
        {
            Apply(_preview.Value);
            Cancel();
            return;
        }

        if (ImGui.GetIO().WantCaptureMouse)
            return;

        if (PluginServices.GameGui.ScreenToWorld(ImGui.GetIO().MousePos, out var world))
        {
            _preview = world;
            DrawMarker(world);
        }
    }

    private static void Apply(Vector3 world)
    {
        switch (Id)
        {
            case "camera":
                C.CameraX = world.X;
                C.CameraY = world.Y;
                C.CameraZ = world.Z;
                break;
            case "cameraPos":
                C.CameraPosX = world.X;
                C.CameraPosY = world.Y;
                C.CameraPosZ = world.Z;
                break;
            case "lookAt":
                C.LookAtX = world.X;
                C.LookAtY = world.Y;
                C.LookAtZ = world.Z;
                break;
            default:
                return;
        }

        C.Save();
    }

    private static unsafe bool IsLeftMouseHeld()
    {
        var input = UIInputData.Instance();
        if (input != null)
        {
            if (input->CursorInputs.MouseButtonHeldFlags.HasFlag(MouseButtonFlags.LBUTTON))
                return true;
            if (input->UIFilteredCursorInputs.MouseButtonHeldFlags.HasFlag(MouseButtonFlags.LBUTTON))
                return true;
        }

        return (GetAsyncKeyState(VkLButton) & 0x8000) != 0;
    }

    private static void DrawMarker(Vector3 world)
    {
        if (!PluginServices.GameGui.WorldToScreen(world, out var screen))
            return;

        var draw = ImGui.GetForegroundDrawList();
        draw.AddCircleFilled(screen, 6f, MarkerColor);
        draw.AddCircle(screen, 10f, 0xE0FFFFFF, 16, 1.5f);

        var label = $"{world.X:0.00}, {world.Y:0.00}, {world.Z:0.00}";
        var textPos = screen + new Vector2(14f, -8f);
        draw.AddText(textPos + new Vector2(1f, 1f), 0xE0000000, label);
        draw.AddText(textPos, 0xFFFFFFFF, label);
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
