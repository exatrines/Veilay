using Dalamud.Configuration;

namespace Veilay;

public enum EntityDisplayMode
{
    DoNothing = 0,
    Hide = 1,
    ReplaceToDummy = 2,
}

public enum CameraCenterMode
{
    At100 = 0,
    AtOrigin = 1,
    Manual = 2,
}

public enum CameraPosMode
{
    High = 0,
    Far = 1,
    Manual = 2,
}

public enum CameraAngleMode
{
    Off = 0,
    Center = 1,
    LookAt = 2,
    Manual = 3,
}

public enum PlayerDummyLabelMode
{
    None = 0,
    Role = 1,
    Job = 2,
}

/// <summary>Persisted Models / Native / Camera settings.</summary>
[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 6;

    public bool EnableVeilay { get; set; }

    public EntityDisplayMode PlayerMode { get; set; } = EntityDisplayMode.ReplaceToDummy;

    public EntityDisplayMode PetMode { get; set; } = EntityDisplayMode.ReplaceToDummy;

    public EntityDisplayMode NpcMode { get; set; }

    public EntityDisplayMode EnemyMode { get; set; }

    public float DummySize { get; set; } = 1f;

    public PlayerDummyLabelMode PlayerDummyLabel { get; set; } = PlayerDummyLabelMode.Role;

    public Vector4 DummyColorTank { get; set; } = new(0.25f, 0.45f, 0.98f, 0.86f);

    public Vector4 DummyColorHealer { get; set; } = new(0.22f, 0.82f, 0.38f, 0.86f);

    public Vector4 DummyColorDps { get; set; } = new(0.95f, 0.32f, 0.32f, 0.86f);

    public Vector4 DummyColorCrafter { get; set; } = new(0.95f, 0.62f, 0.22f, 0.86f);

    public Vector4 DummyColorGatherer { get; set; } = new(0.22f, 0.78f, 0.72f, 0.86f);

    public Vector4 DummyColorPet { get; set; } = new(0.82f, 0.82f, 0.86f, 0.72f);

    public Vector4 DummyColorNpc { get; set; } = new(0.72f, 0.55f, 0.92f, 0.86f);

    public Vector4 DummyColorEnemy { get; set; } = new(0.95f, 0.45f, 0.18f, 0.86f);

    public bool ToggleNativeCharacterConfig { get; set; } = true;

    public bool HideBattleEffects { get; set; } = true;

    public bool HideTargetLines { get; set; } = true;

    public HashSet<string> HiddenHudAddons { get; set; } = [];

    public bool BattleEffectsOverridden { get; set; }

    public uint BattleEffectSelfBackup { get; set; }

    public uint BattleEffectPartyBackup { get; set; }

    public uint BattleEffectOtherBackup { get; set; }

    public bool TargetLineOverridden { get; set; }

    public uint TargetLineBackup { get; set; }

    public bool LockCamera { get; set; }

    public CameraCenterMode CameraCenterMode { get; set; }

    public float CameraX { get; set; }

    public float CameraY { get; set; }

    public float CameraZ { get; set; }

    public CameraPosMode CameraPosMode { get; set; }

    public float CameraPosX { get; set; }

    public float CameraPosY { get; set; }

    public float CameraPosZ { get; set; }

    public CameraAngleMode CameraAngleMode { get; set; }

    public float CameraYaw { get; set; }

    public float CameraPitch { get; set; }

    public float LookAtX { get; set; }

    public float LookAtY { get; set; }

    public float LookAtZ { get; set; }

    public bool DisableWasdInput { get; set; } = true;

    public bool DisableArrowInput { get; set; } = true;

    public bool DisableMouseWheelZoom { get; set; } = true;

    public float CameraZoom { get; set; } = 0.06f;

    [NonSerialized]
    private IDalamudPluginInterface? _pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
        HiddenHudAddons ??= [];
        if (Version >= 6)
            return;

        Version = 6;
        Save();
    }

    public void Save() => _pluginInterface?.SavePluginConfig(this);
}
