using Dalamud.Game.Config;

namespace Veilay.Services;

/// <summary>Rewrites Character Configuration during duty replay, then restores the previous values.</summary>
internal sealed class CharacterConfigOverrideService
{
    private const uint BattleEffectNone = 2;
    private const uint Hidden = 0;

    internal static bool WantsApply =>
        C.ToggleNativeCharacterConfig
        && (C.HideBattleEffects || C.HideTargetLines || C.HideFlyText || C.HidePopupText);

    public void Update()
    {
        var apply = VisibilityService.ApplyReady && C.ToggleNativeCharacterConfig;
        TickBattleEffects(apply && C.HideBattleEffects);
        TickUiControl(
            apply && C.HideTargetLines,
            UiControlOption.TargetLineType,
            Hidden,
            C.TargetLineOverridden,
            C.TargetLineBackup,
            overridden => C.TargetLineOverridden = overridden,
            backup => C.TargetLineBackup = backup);
        TickUiControl(
            apply && C.HideFlyText,
            UiControlOption.FlyTextDisp,
            Hidden,
            C.FlyTextOverridden,
            C.FlyTextBackup,
            overridden => C.FlyTextOverridden = overridden,
            backup => C.FlyTextBackup = backup);
        TickUiControl(
            apply && C.HidePopupText,
            UiControlOption.PopUpTextDisp,
            Hidden,
            C.PopupTextOverridden,
            C.PopupTextBackup,
            overridden => C.PopupTextOverridden = overridden,
            backup => C.PopupTextBackup = backup);
    }

    public void Restore()
    {
        TickBattleEffects(hide: false);
        TickUiControl(
            hide: false,
            UiControlOption.TargetLineType,
            Hidden,
            C.TargetLineOverridden,
            C.TargetLineBackup,
            overridden => C.TargetLineOverridden = overridden,
            backup => C.TargetLineBackup = backup);
        TickUiControl(
            hide: false,
            UiControlOption.FlyTextDisp,
            Hidden,
            C.FlyTextOverridden,
            C.FlyTextBackup,
            overridden => C.FlyTextOverridden = overridden,
            backup => C.FlyTextBackup = backup);
        TickUiControl(
            hide: false,
            UiControlOption.PopUpTextDisp,
            Hidden,
            C.PopupTextOverridden,
            C.PopupTextBackup,
            overridden => C.PopupTextOverridden = overridden,
            backup => C.PopupTextBackup = backup);
    }

    private static void TickBattleEffects(bool hide)
    {
        if (hide)
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

            if (GameConfigOverride.TryGet(UiConfigOption.BattleEffectSelf, out var curSelf)
                && curSelf == BattleEffectNone
                && GameConfigOverride.TryGet(UiConfigOption.BattleEffectParty, out var curParty)
                && curParty == BattleEffectNone
                && GameConfigOverride.TryGet(UiConfigOption.BattleEffectOther, out var curOther)
                && curOther == BattleEffectNone)
                return;

            GameConfigOverride.TrySet(UiConfigOption.BattleEffectSelf, BattleEffectNone);
            GameConfigOverride.TrySet(UiConfigOption.BattleEffectParty, BattleEffectNone);
            GameConfigOverride.TrySet(UiConfigOption.BattleEffectOther, BattleEffectNone);
            return;
        }

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

    private static void TickUiControl(
        bool hide,
        UiControlOption option,
        uint hidden,
        bool overridden,
        uint backup,
        Action<bool> setOverridden,
        Action<uint> setBackup)
    {
        if (hide)
        {
            if (!overridden)
            {
                if (!GameConfigOverride.TryGet(option, out var current))
                    return;

                setBackup(current);
                setOverridden(true);
                C.Save();
            }

            if (GameConfigOverride.TryGet(option, out var value) && value == hidden)
                return;

            GameConfigOverride.TrySet(option, hidden);
            return;
        }

        if (!overridden)
            return;

        if (!PluginServices.ClientState.IsLoggedIn)
            return;

        if (!GameConfigOverride.TrySet(option, backup))
            return;

        setOverridden(false);
        C.Save();
    }
}
