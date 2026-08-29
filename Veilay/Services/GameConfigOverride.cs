using Dalamud.Game.Config;

namespace Veilay.Services;

/// <summary>Shared TryGet / TrySet for UiConfig and UiControl overrides.</summary>
internal static class GameConfigOverride
{
    internal static bool TryGet(UiConfigOption option, out uint value) =>
        PluginServices.GameConfig.TryGet(option, out value);

    internal static bool TryGet(UiControlOption option, out uint value) =>
        PluginServices.GameConfig.TryGet(option, out value);

    internal static bool TrySet(UiConfigOption option, uint value)
    {
        if (!TryGet(option, out var current))
            return false;

        if (current != value)
            PluginServices.GameConfig.Set(option, value);

        return true;
    }

    internal static bool TrySet(UiControlOption option, uint value)
    {
        if (!TryGet(option, out var current))
            return false;

        if (current != value)
            PluginServices.GameConfig.Set(option, value);

        return true;
    }
}
