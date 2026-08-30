using FFXIVClientStructs.FFXIV.Client.UI;

namespace Veilay.Services;

/// <summary>Hides the NamePlate addon while Enable Veilay is applying.</summary>
internal sealed unsafe class NamePlateHideService
{
    private const string AddonName = "NamePlate";

    private ushort _hiddenAddonId;
    private bool _hiddenByPlugin;

    public void Update()
    {
        if (!PluginServices.ClientState.IsLoggedIn)
        {
            Restore();
            return;
        }

        Apply(VisibilityService.ModelsApplyReady);
    }

    public void Restore() => RestoreVisuals();

    private void Apply(bool apply)
    {
        if (!apply)
        {
            Restore();
            return;
        }

        PluginServices.PluginInterface.UiBuilder.DisableUserUiHide = true;

        var manager = RaptureAtkUnitManager.Instance();
        if (manager == null)
            return;

        var addon = manager->GetAddonByName(AddonName);
        if (addon == null)
            return;

        if (addon->IsVisible)
        {
            manager->SetAddonVisibility(addon->Id, false);
            _hiddenAddonId = addon->Id;
        }

        _hiddenByPlugin = true;
    }

    private void RestoreVisuals()
    {
        if (!_hiddenByPlugin)
            return;

        var manager = RaptureAtkUnitManager.Instance();
        if (manager != null && _hiddenAddonId != 0)
            manager->SetAddonVisibility(_hiddenAddonId, true);

        _hiddenAddonId = 0;
        _hiddenByPlugin = false;
    }
}
