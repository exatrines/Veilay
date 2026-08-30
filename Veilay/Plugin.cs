using Dalamud.Game.Command;
using Veilay.UI;

namespace Veilay;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/veilay";

    internal static Configuration C = null!;

    private readonly WindowSystem _windowSystem;
    private readonly MainWindow _mainWindow;
    private readonly VisibilityService _visibility;
    private readonly HudHideService _hudHide;
    private readonly CameraLockService _cameraLock;
    private readonly CharacterConfigOverrideService _characterConfig;
    private readonly NamePlateHideService _namePlates;
    private readonly MarkerOverlayService _markers;

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IFramework framework,
        IClientState clientState,
        IObjectTable objectTable,
        ITargetManager targetManager,
        IGameGui gameGui,
        IGameConfig gameConfig,
        IDataManager dataManager,
        IGameInteropProvider gameInterop,
        ISigScanner sigScanner)
    {
        PluginServices.Init(
            pluginInterface,
            commandManager,
            framework,
            clientState,
            objectTable,
            targetManager,
            gameGui,
            gameConfig,
            dataManager,
            gameInterop,
            sigScanner);

        C = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        C.Initialize(pluginInterface);

        _visibility = new VisibilityService();
        _hudHide = new HudHideService();
        _cameraLock = new CameraLockService();
        _characterConfig = new CharacterConfigOverrideService();
        _namePlates = new NamePlateHideService();
        _markers = new MarkerOverlayService(_visibility);
        _mainWindow = new MainWindow();
        _windowSystem = new WindowSystem("Veilay");
        _windowSystem.AddWindow(_mainWindow);

        pluginInterface.UiBuilder.Draw += DrawUi;
        pluginInterface.UiBuilder.OpenConfigUi += ToggleMain;
        pluginInterface.UiBuilder.OpenMainUi += ToggleMain;

        commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle Veilay.",
        });

        framework.Update += OnFrameworkUpdate;
        clientState.TerritoryChanged += OnTerritoryChanged;
    }

    private void OnCommand(string command, string args) => ToggleMain();

    private void ToggleMain() => _mainWindow.Toggle();

    private void OnTerritoryChanged(uint territoryType)
    {
        if (VisibilityService.InPlayback)
            return;

        ReleaseForTransition();
    }

    private void ReleaseForTransition()
    {
        _cameraLock.Release();
        _visibility.ReleaseForTransition();
        _hudHide.Restore();
        _characterConfig.Restore();
        _namePlates.Restore();
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        _visibility.Update();
        _hudHide.Update();
        _cameraLock.Update();
        _characterConfig.Update();
        _namePlates.Update();
    }

    private void DrawUi()
    {
        _windowSystem.Draw();
        _markers.Draw();
        WorldPick.Draw();
    }

    public void Dispose()
    {
        PluginServices.Framework.Update -= OnFrameworkUpdate;
        PluginServices.ClientState.TerritoryChanged -= OnTerritoryChanged;
        PluginServices.CommandManager.RemoveHandler(CommandName);
        PluginServices.PluginInterface.UiBuilder.Draw -= DrawUi;
        PluginServices.PluginInterface.UiBuilder.OpenConfigUi -= ToggleMain;
        PluginServices.PluginInterface.UiBuilder.OpenMainUi -= ToggleMain;

        _cameraLock.Dispose();
        _characterConfig.Restore();
        _namePlates.Restore();
        _visibility.Restore();
        _hudHide.Restore();
        C.Save();

        _windowSystem.RemoveAllWindows();
        PluginServices.Clear();
        C = null!;
    }
}
