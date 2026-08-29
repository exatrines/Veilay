using Dalamud.Game.Config;

namespace Veilay.Services;

/// <summary>Turns off character-config battle effects during replay, then restores them.</summary>
internal sealed class BattleEffectService
{
    private const uint ShowNone = 2;

    public void Update()
    {
        if (VisibilityService.ApplyReady && C.ToggleNativeCharacterConfig && C.HideBattleEffects)
            Apply();
        else
            Restore();
    }

    public void Restore()
    {
        if (!C.BattleEffectsOverridden)
            return;

        if (!PluginServices.ClientState.IsLoggedIn)
            return;

        if (!GameConfigOverride.TrySet(UiConfigOption.BattleEffectSelf, C.BattleEffectSelfBackup)
            || !GameConfigOverride.TrySet(UiConfigOption.BattleEffectParty, C.BattleEffectPartyBackup)
            || !GameConfigOverride.TrySet(UiConfigOption.BattleEffectOther, C.BattleEffectOtherBackup))
            return;

        C.BattleEffectsOverridden = false;
        C.Save();
    }

    private static void Apply()
    {
        if (!C.BattleEffectsOverridden)
        {
            if (!GameConfigOverride.TryGet(UiConfigOption.BattleEffectSelf, out var self)
                || !GameConfigOverride.TryGet(UiConfigOption.BattleEffectParty, out var party)
                || !GameConfigOverride.TryGet(UiConfigOption.BattleEffectOther, out var other))
                return;

            C.BattleEffectSelfBackup = self;
            C.BattleEffectPartyBackup = party;
            C.BattleEffectOtherBackup = other;
            C.BattleEffectsOverridden = true;
            C.Save();
        }

        if (IsAlreadyOff())
            return;

        GameConfigOverride.TrySet(UiConfigOption.BattleEffectSelf, ShowNone);
        GameConfigOverride.TrySet(UiConfigOption.BattleEffectParty, ShowNone);
        GameConfigOverride.TrySet(UiConfigOption.BattleEffectOther, ShowNone);
    }

    private static bool IsAlreadyOff() =>
        GameConfigOverride.TryGet(UiConfigOption.BattleEffectSelf, out var self) && self == ShowNone
        && GameConfigOverride.TryGet(UiConfigOption.BattleEffectParty, out var party) && party == ShowNone
        && GameConfigOverride.TryGet(UiConfigOption.BattleEffectOther, out var other) && other == ShowNone;
}
