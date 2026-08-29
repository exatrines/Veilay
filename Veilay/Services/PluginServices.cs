namespace Veilay.Services;

/// <summary>Dalamud services injected at plugin startup.</summary>
internal static class PluginServices
{
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    internal static ICommandManager CommandManager { get; private set; } = null!;
    internal static IFramework Framework { get; private set; } = null!;
    internal static IClientState ClientState { get; private set; } = null!;
    internal static IObjectTable ObjectTable { get; private set; } = null!;
    internal static ITargetManager TargetManager { get; private set; } = null!;
    internal static IGameGui GameGui { get; private set; } = null!;
    internal static IGameConfig GameConfig { get; private set; } = null!;
    internal static IDataManager DataManager { get; private set; } = null!;
    internal static IGameInteropProvider GameInterop { get; private set; } = null!;
    internal static ISigScanner SigScanner { get; private set; } = null!;

    internal static void Init(
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
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        Framework = framework;
        ClientState = clientState;
        ObjectTable = objectTable;
        TargetManager = targetManager;
        GameGui = gameGui;
        GameConfig = gameConfig;
        DataManager = dataManager;
        GameInterop = gameInterop;
        SigScanner = sigScanner;
    }

    internal static void Clear()
    {
        PluginInterface = null!;
        CommandManager = null!;
        Framework = null!;
        ClientState = null!;
        ObjectTable = null!;
        TargetManager = null!;
        GameGui = null!;
        GameConfig = null!;
        DataManager = null!;
        GameInterop = null!;
        SigScanner = null!;
    }
}
