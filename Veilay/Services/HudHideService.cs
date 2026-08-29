using FFXIVClientStructs.FFXIV.Client.UI;

namespace Veilay.Services;

/// <summary>Hides selected native HUD Layout addons without touching the full addon table.</summary>
internal sealed unsafe class HudHideService
{
    private const string ReplayPlayerAddon = "ContentsReplayPlayer";
    private const string ReplayExitPromptAddon = "SelectYesno";

    private readonly HashSet<ushort> _hiddenHudAddonIds = [];
    private readonly HashSet<string> _appliedHudAddons = [];
    private bool _hudHiddenByPlugin;

    public void Update()
    {
        if (!PluginServices.ClientState.IsLoggedIn)
        {
            Restore();
            return;
        }

        Apply(VisibilityService.HudApplyReady);
    }

    public void Restore()
    {
        RestoreVisuals();
        if (!_hudHiddenByPlugin)
            PluginServices.PluginInterface.UiBuilder.DisableUserUiHide = false;
    }

    private void Apply(bool apply)
    {
        if (!apply || C.HiddenHudAddons.Count == 0)
        {
            Restore();
            return;
        }

        PluginServices.PluginInterface.UiBuilder.DisableUserUiHide = true;

        var manager = RaptureAtkUnitManager.Instance();
        if (manager == null)
            return;

        if (_hudHiddenByPlugin && !_appliedHudAddons.SetEquals(C.HiddenHudAddons))
            RestoreVisuals();

        HideSelected(manager);

        _appliedHudAddons.Clear();
        foreach (var name in C.HiddenHudAddons)
            _appliedHudAddons.Add(name);
        _hudHiddenByPlugin = true;
    }

    private void RestoreVisuals()
    {
        if (!_hudHiddenByPlugin)
            return;

        var manager = RaptureAtkUnitManager.Instance();
        if (manager == null)
            return;

        foreach (var id in _hiddenHudAddonIds)
            manager->SetAddonVisibility(id, true);

        _hiddenHudAddonIds.Clear();
        _appliedHudAddons.Clear();
        _hudHiddenByPlugin = false;
    }

    private void HideSelected(RaptureAtkUnitManager* manager)
    {
        foreach (var name in C.HiddenHudAddons)
        {
            if (name is ReplayPlayerAddon or ReplayExitPromptAddon)
                continue;

            var addon = manager->GetAddonByName(name);
            if (addon == null || !addon->IsVisible)
                continue;

            manager->SetAddonVisibility(addon->Id, false);
            _hiddenHudAddonIds.Add(addon->Id);
        }
    }
}
