using Dalamud.Game.Config;

namespace Veilay.Services;

/// <summary>Turns off character-config target lines during replay, then restores them.</summary>
internal sealed class TargetLineService
{
    private const uint Hidden = 0;

    public void Update()
    {
        if (VisibilityService.ApplyReady && C.ToggleNativeCharacterConfig && C.HideTargetLines)
            Apply();
        else
            Restore();
    }

    public void Restore()
    {
        if (!C.TargetLineOverridden)
            return;

        if (!PluginServices.ClientState.IsLoggedIn)
            return;

        if (!GameConfigOverride.TrySet(UiControlOption.TargetLineType, C.TargetLineBackup))
            return;

        C.TargetLineOverridden = false;
        C.Save();
    }

    private static void Apply()
    {
        if (!C.TargetLineOverridden)
        {
            if (!GameConfigOverride.TryGet(UiControlOption.TargetLineType, out var current))
                return;

            C.TargetLineBackup = current;
            C.TargetLineOverridden = true;
            C.Save();
        }

        if (GameConfigOverride.TryGet(UiControlOption.TargetLineType, out var value) && value == Hidden)
            return;

        GameConfigOverride.TrySet(UiControlOption.TargetLineType, Hidden);
    }
}
