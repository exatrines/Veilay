using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Veilay.Services;

/// <summary>Hides player, pet, NPC, and enemy models without removing entities. Owns apply-ready gates.</summary>
internal sealed unsafe class VisibilityService
{
    private const uint InvalidOwnerId = 0xE0000000;
    private const int ReadyFrameThreshold = 5;
    internal const int ObjectSlotCount = 819;
    private const VisibilityFlags HiddenRenderMask = VisibilityFlags.Model | VisibilityFlags.Nameplate;
    private const string ReplayPlayerAddon = "ContentsReplayPlayer";

    private readonly HashSet<ushort> _hiddenIndexes = [];
    private readonly HashSet<ushort> _hiddenScratch = [];
    private readonly HashSet<ushort> _dummyIndexes = [];
    private readonly HashSet<uint> _playerEntityIds = [];
    private int _readyFrames;
    private int _liveModelsReadyFrames;
    private int _liveHudReadyFrames;
    private int _liveCameraReadyFrames;
    private bool _wasInPlayback;

    public static bool InPlayback { get; private set; }

    public static bool ApplyReady { get; private set; }

    public static bool ModelsApplyReady { get; private set; }

    public static bool HudApplyReady { get; private set; }

    public static bool CameraApplyReady { get; private set; }

    public IReadOnlySet<ushort> DummyIndexes => _dummyIndexes;

    public void Update()
    {
        if (!PluginServices.ClientState.IsLoggedIn)
        {
            Restore();
            InPlayback = false;
            _wasInPlayback = false;
            return;
        }

        InPlayback = ReadInPlayback();
        if (InPlayback != _wasInPlayback)
            TurnAutoOffSwitches();
        _wasInPlayback = InPlayback;

        if (!HasEnabledFeature())
        {
            RestoreApplied();
            return;
        }

        if (!InPlayback)
            _readyFrames = 0;
        else if (!ApplyReady)
        {
            if (IsReplayReady())
                _readyFrames++;
            else
                _readyFrames = 0;
        }

        ApplyReady = _readyFrames >= ReadyFrameThreshold;
        ModelsApplyReady = InPlayback
            ? ApplyReady && C.EnableVeilay
            : UpdateLiveReady(ref _liveModelsReadyFrames, ModelsApplyReady, C.EnableVeilay);
        HudApplyReady = InPlayback
            ? ApplyReady
            : UpdateLiveReady(ref _liveHudReadyFrames, HudApplyReady, HidesAnyHud());
        CameraApplyReady = InPlayback
            ? ApplyReady
            : UpdateLiveReady(ref _liveCameraReadyFrames, CameraApplyReady, C.LockCamera);
        UpdateModels(ModelsApplyReady);
    }

    public void Restore()
    {
        TurnAutoOffSwitches();
        RestoreApplied();
    }

    public void ReleaseForTransition()
    {
        if (ReadInPlayback())
            return;

        Restore();
        InPlayback = false;
        _wasInPlayback = false;
    }

    private static bool HasEnabledFeature() =>
        HidesModel(C.PlayerMode)
        || HidesModel(C.PetMode)
        || HidesModel(C.NpcMode)
        || HidesModel(C.EnemyMode)
        || (C.ToggleNativeCharacterConfig && (C.HideBattleEffects || C.HideTargetLines))
        || HidesAnyHud()
        || C.LockCamera;

    private static bool HidesAnyHud() =>
        C.HiddenHudAddons.Count > 0;

    private static bool HidesModel(EntityDisplayMode mode) =>
        mode is EntityDisplayMode.Hide or EntityDisplayMode.ReplaceToDummy;

    private static bool ShowsDummy(EntityDisplayMode mode) =>
        mode is EntityDisplayMode.ReplaceToDummy;

    private static void TurnAutoOffSwitches()
    {
        var changed = false;
        if (C.EnableVeilay)
        {
            C.EnableVeilay = false;
            changed = true;
        }

        if (C.LockCamera)
        {
            C.LockCamera = false;
            changed = true;
        }

        if (C.HiddenHudAddons.Count > 0)
        {
            C.HiddenHudAddons.Clear();
            changed = true;
        }

        if (changed)
            C.Save();
    }

    private static bool ReadInPlayback()
    {
        var manager = ContentsReplayManager.Instance();
        if (manager == null)
            return false;

        return manager->PlaybackControls.HasFlag(ContentsReplayPlaybackControl.InPlayback);
    }

    private void RestoreApplied()
    {
        ApplyReady = false;
        ModelsApplyReady = false;
        HudApplyReady = false;
        CameraApplyReady = false;
        _readyFrames = 0;
        _liveModelsReadyFrames = 0;
        _liveHudReadyFrames = 0;
        _liveCameraReadyFrames = 0;
        RestoreModels();
        _dummyIndexes.Clear();
    }

    private bool UpdateLiveReady(ref int frames, bool alreadyReady, bool enabled)
    {
        if (!enabled)
        {
            frames = 0;
            return false;
        }

        if (alreadyReady)
            return true;

        if (!IsLocalPlayerDrawable())
        {
            frames = 0;
            return false;
        }

        frames++;
        return frames >= ReadyFrameThreshold;
    }

    private static bool IsLocalPlayerDrawable()
    {
        var local = PluginServices.ObjectTable.LocalPlayer;
        if (local == null || local.Address == nint.Zero)
            return false;

        var native = (GameObject*)local.Address;
        return native->DrawObject != null;
    }

    private static bool IsReplayReady()
    {
        if (!IsLocalPlayerDrawable())
            return false;

        var addon = PluginServices.GameGui.GetAddonByName(ReplayPlayerAddon);
        if (addon == nint.Zero)
            return false;

        var unit = (AtkUnitBase*)addon.Address;
        return unit != null && unit->IsReady && unit->IsVisible;
    }

    private void UpdateModels(bool apply)
    {
        if (!apply
            || !C.EnableVeilay
            || (!HidesModel(C.PlayerMode)
                && !HidesModel(C.PetMode)
                && !HidesModel(C.NpcMode)
                && !HidesModel(C.EnemyMode)))
        {
            RestoreModels();
            return;
        }

        var manager = GameObjectManager.Instance();
        if (manager == null)
            return;

        _playerEntityIds.Clear();
        for (var i = 0; i < ObjectSlotCount; i++)
        {
            var native = manager->Objects.IndexSorted[i].Value;
            if (native != null && native->ObjectKind == ObjectKind.Pc)
                _playerEntityIds.Add(native->EntityId);
        }

        _hiddenScratch.Clear();
        _dummyIndexes.Clear();
        for (var i = 0; i < ObjectSlotCount; i++)
        {
            var native = manager->Objects.IndexSorted[i].Value;
            if (native == null)
                continue;

            var mode = ModeFor(native);
            if (HidesModel(mode))
            {
                if (!IsDrawn(native) && !_hiddenIndexes.Contains(native->ObjectIndex))
                    continue;

                SetHidden(native, true);
                _hiddenScratch.Add(native->ObjectIndex);
                if (ShowsDummy(mode))
                    _dummyIndexes.Add(native->ObjectIndex);
            }
            else if (_hiddenIndexes.Contains(native->ObjectIndex))
            {
                SetHidden(native, false);
            }
        }

        _hiddenIndexes.Clear();
        foreach (var index in _hiddenScratch)
            _hiddenIndexes.Add(index);
    }

    private void RestoreModels()
    {
        _dummyIndexes.Clear();
        if (_hiddenIndexes.Count == 0)
            return;

        var manager = GameObjectManager.Instance();
        if (manager != null)
        {
            for (var i = 0; i < ObjectSlotCount; i++)
            {
                var native = manager->Objects.IndexSorted[i].Value;
                if (native != null && _hiddenIndexes.Contains(native->ObjectIndex))
                    SetHidden(native, false);
            }
        }

        _hiddenIndexes.Clear();
    }

    private static void SetHidden(GameObject* native, bool hide)
    {
        if (native->IsCharacter())
        {
            SetCharacterHidden(native, hide);
            return;
        }

        if (hide)
        {
            if (!native->RenderFlags.HasFlag(HiddenRenderMask))
                native->RenderFlags |= HiddenRenderMask;
            return;
        }

        if ((native->RenderFlags & HiddenRenderMask) != 0)
            native->RenderFlags &= ~HiddenRenderMask;
    }

    private static void SetCharacterHidden(GameObject* native, bool hide)
    {
        if (hide)
        {
            native->RenderFlags |= VisibilityFlags.Nameplate;
            native->RenderFlags &= ~VisibilityFlags.Model;
            var drawObject = native->DrawObject;
            if (drawObject != null)
                drawObject->IsVisible = false;
            return;
        }

        native->RenderFlags &= ~HiddenRenderMask;
        var draw = native->DrawObject;
        if (draw != null)
            draw->IsVisible = true;
    }

    private EntityDisplayMode ModeFor(GameObject* native)
    {
        if (native->ObjectKind == ObjectKind.Pc)
            return C.PlayerMode;

        if (IsPlayerOwnedPet(native))
            return C.PetMode;

        if (IsEnemy(native))
            return C.EnemyMode;

        if (IsNpc(native))
            return C.NpcMode;

        return EntityDisplayMode.DoNothing;
    }

    internal static bool IsNpc(GameObject* native) =>
        native->ObjectKind == ObjectKind.EventNpc
        || (native->ObjectKind == ObjectKind.BattleNpc
            && native->BattleNpcSubKind == BattleNpcSubKind.NpcPartyMember);

    internal static bool IsEnemy(GameObject* native) =>
        native->ObjectKind == ObjectKind.BattleNpc
        && native->BattleNpcSubKind is BattleNpcSubKind.Combatant or BattleNpcSubKind.BNpcPart;

    private static bool IsDrawn(GameObject* native) =>
        native->DrawObject != null;

    private bool IsPlayerOwnedPet(GameObject* native)
    {
        if (native->ObjectKind != ObjectKind.BattleNpc)
            return false;

        var sub = native->BattleNpcSubKind;
        if (sub is not (BattleNpcSubKind.Pet or BattleNpcSubKind.Buddy))
            return false;

        var ownerId = native->OwnerId;
        if (ownerId == 0 || ownerId == InvalidOwnerId)
            return false;

        return _playerEntityIds.Contains(ownerId);
    }
}
