using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace Veilay.UI;

/// <summary>Single window for Models, Native, and Camera settings.</summary>
internal sealed class MainWindow : Window
{
    private static readonly Vector2 DefaultSize = new(420, 640);
    private static readonly Vector4 CammyNoticeColor = new(1f, 0.85f, 0.35f, 1f);
    private static readonly Vector4 WorldPickButtonColor = new(0.95f, 0.72f, 0.2f, 1f);

    private static readonly string[] DisplayModeLabels =
    [
        "Do nothing",
        "Hide",
        "Replace to dummy",
    ];

    private static readonly string[] CameraCenterModeLabels =
    [
        "(100, 0, 100)",
        "(0, 0, 0)",
        "Manual",
    ];

    private static readonly string[] CameraPosModeLabels =
    [
        "(Center.y + 50)",
        "(Center.y + 30 & Center.z + 40)",
        "Manual",
    ];

    private static readonly string[] CameraAngleModeLabels =
    [
        "Off",
        "Center",
        "LookAt",
        "Manual",
    ];

    private static readonly string[] PlayerDummyLabelLabels =
    [
        "None",
        "Role",
        "Job",
    ];

    public MainWindow()
        : base("Veilay###veilayMain")
    {
        Size = DefaultSize;
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(320, 280),
            MaximumSize = new Vector2(720, 1100),
        };
    }

    public override void Draw()
    {
        if (!ImGui.BeginTabBar("##veilayTabs"))
            return;

        if (ImGui.BeginTabItem("Models"))
        {
            DrawModelsTab();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Native"))
        {
            DrawNativeCharacterConfig();
            ImGui.Separator();
            DrawNativeHud();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Camera"))
        {
            DrawCameraLock();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    private static void DrawModelsTab()
    {
        DrawSavedCheckbox(
            "Enable Veilay",
            C.EnableVeilay,
            value => C.EnableVeilay = value,
            "Hides selected models only. Turns off with HUD checks and Lock Camera when replay begins or ends, and on zone change outside replay. Off keeps Player / Pets / NPC / Enemy and Dummy Size but does not apply them.");

        ImGui.Separator();

        if (DrawDisplayMode("Player", "playerMode", C.PlayerMode, out var playerMode))
        {
            C.PlayerMode = playerMode;
            C.Save();
        }

        Hint("All player characters, including you.");

        if (DrawDisplayMode("Pets", "petMode", C.PetMode, out var petMode))
        {
            C.PetMode = petMode;
            C.Save();
        }

        Hint(
            "Fairy, carbuncle and summons, automaton queen, lilybell, living shadow, and other player-owned pets, plus chocobo companions. Not minions.");

        if (DrawDisplayMode("NPC", "npcMode", C.NpcMode, out var npcMode))
        {
            C.NpcMode = npcMode;
            C.Save();
        }

        Hint("Town NPCs and NPC party members such as Trust and squadron. Not minions.");

        if (DrawDisplayMode("Enemy", "enemyMode", C.EnemyMode, out var enemyMode))
        {
            C.EnemyMode = enemyMode;
            C.Save();
        }

        Hint("Hostile and guard Battle NPCs, including weak spots.");

        var dummySize = C.DummySize;
        if (ImGui.SliderFloat("Dummy Size", ref dummySize, 0.5f, 4f, "%.1f"))
        {
            C.DummySize = dummySize;
            C.Save();
        }

        Hint("On-screen dummy scale. Used when a category is set to Replace to dummy.");

        if (DrawEnumCombo("Player label", "playerDummyLabel", C.PlayerDummyLabel, PlayerDummyLabelLabels, out var playerLabel))
        {
            C.PlayerDummyLabel = playerLabel;
            C.Save();
        }

        Hint("Shown above player dummies. NPC and enemy dummies show names.");

        ImGui.Separator();
        ImGui.TextUnformatted("Dummy colors");
        DrawDummyColor("Tank", C.DummyColorTank, value => C.DummyColorTank = value);
        ImGui.SameLine();
        DrawDummyColor("Healer", C.DummyColorHealer, value => C.DummyColorHealer = value);
        ImGui.SameLine();
        DrawDummyColor("DPS", C.DummyColorDps, value => C.DummyColorDps = value);
        ImGui.SameLine();
        DrawDummyColor("Crafter", C.DummyColorCrafter, value => C.DummyColorCrafter = value);
        DrawDummyColor("Gatherer", C.DummyColorGatherer, value => C.DummyColorGatherer = value);
        ImGui.SameLine();
        DrawDummyColor("Pet", C.DummyColorPet, value => C.DummyColorPet = value);
        ImGui.SameLine();
        DrawDummyColor("NPC", C.DummyColorNpc, value => C.DummyColorNpc = value);
        ImGui.SameLine();
        DrawDummyColor("Enemy", C.DummyColorEnemy, value => C.DummyColorEnemy = value);
    }

    private static void DrawDummyColor(string label, Vector4 current, Action<Vector4> set)
    {
        var value = current;
        if (ImGui.ColorEdit4(label, ref value, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar))
        {
            set(value);
            C.Save();
        }
    }

    private static void DrawNativeCharacterConfig()
    {
        DrawSavedCheckbox(
            "Toggle Native Character Config",
            C.ToggleNativeCharacterConfig,
            value => C.ToggleNativeCharacterConfig = value,
            "Applies during content replay only. Temporarily changes Character Configuration. Previous values are restored when replay ends.");

        using (ImRaii.Disabled(!C.ToggleNativeCharacterConfig))
        {
            ImGui.Indent();
            DrawSavedCheckbox(
                "Hide Battle Effect",
                C.HideBattleEffects,
                value => C.HideBattleEffects = value,
                "Battle effects for self, party, and others.");
            DrawSavedCheckbox(
                "Hide Target Lines",
                C.HideTargetLines,
                value => C.HideTargetLines = value,
                "The arrows when a player or enemy targets someone.");
            ImGui.Unindent();
        }
    }

    private static void DrawNativeHud()
    {
        ImGui.TextUnformatted("HUD Layout");
        Hint("Native HUD Layout elements only, not every addon. Independent from Enable Veilay while applying. Checks clear when replay begins or ends, and on zone change outside replay. The replay seek bar and exit Yes/No stay visible.");

        var grouped = HudLayoutCatalog.GetGrouped();
        if (grouped.Count == 0)
        {
            ImGui.TextDisabled("HUD Layout list is not available yet.");
            return;
        }

        if (ImGui.Button("Check all"))
        {
            foreach (var item in HudLayoutCatalog.GetItems())
                C.HiddenHudAddons.Add(item.AddonName);
            C.Save();
        }

        ImGui.SameLine();
        if (ImGui.Button("Uncheck all"))
        {
            C.HiddenHudAddons.Clear();
            C.Save();
        }

        if (!ImGui.BeginChild("##hudLayoutList", new Vector2(-1f, 360f), true))
            return;

        foreach (var (group, items) in grouped)
            DrawHudGroup(group, items);

        ImGui.EndChild();
    }

    private static void DrawHudGroup(string group, IReadOnlyList<HudLayoutItem> items)
    {
        var hideGroup = true;
        foreach (var item in items)
        {
            if (C.HiddenHudAddons.Contains(item.AddonName))
                continue;

            hideGroup = false;
            break;
        }

        if (ImGui.Checkbox($"##hudGroup{group}", ref hideGroup))
        {
            foreach (var item in items)
            {
                if (hideGroup)
                    C.HiddenHudAddons.Add(item.AddonName);
                else
                    C.HiddenHudAddons.Remove(item.AddonName);
            }

            C.Save();
        }

        ImGui.SameLine();
        ImGui.TextUnformatted($"{group} ({items.Count})");

        ImGui.Indent();
        foreach (var item in items)
        {
            var hidden = C.HiddenHudAddons.Contains(item.AddonName);
            if (ImGui.Checkbox($"{item.Label}##hud{item.AddonName}", ref hidden))
            {
                if (hidden)
                    C.HiddenHudAddons.Add(item.AddonName);
                else
                    C.HiddenHudAddons.Remove(item.AddonName);
                C.Save();
            }

            Hint(item.AddonName);
        }

        ImGui.Unindent();
    }

    private static void DrawCameraLock()
    {
        DrawSavedCheckbox(
            "Lock Camera Position",
            C.LockCamera,
            value => C.LockCamera = value,
            "Locks the camera from Center and Pos in duties, overworld, and replay. Turns off with Enable Veilay and HUD checks when replay begins or ends, and on zone change outside replay. Angle Off keeps mouse look.");

        if (IsCammyLoaded())
            ImGui.TextColored(CammyNoticeColor, "Cammy is loaded. Do not use FreeCam with camera lock.");

        if (DrawEnumCombo("Center", "cameraCenterMode", C.CameraCenterMode, CameraCenterModeLabels, out var centerMode))
        {
            if (centerMode == CameraCenterMode.Manual)
            {
                CameraLockService.GetCenter(out var x, out var y, out var z);
                C.CameraX = x;
                C.CameraY = y;
                C.CameraZ = z;
            }

            C.CameraCenterMode = centerMode;
            C.Save();
        }

        if (C.CameraCenterMode == CameraCenterMode.Manual
            && DrawWorldXyzRow("##cameraXYZ", "camera", C.CameraX, C.CameraY, C.CameraZ, out var cameraX, out var cameraY, out var cameraZ))
        {
            C.CameraX = cameraX;
            C.CameraY = cameraY;
            C.CameraZ = cameraZ;
            C.Save();
        }

        if (DrawEnumCombo("Pos", "cameraPosMode", C.CameraPosMode, CameraPosModeLabels, out var posMode))
        {
            if (posMode == CameraPosMode.Manual)
            {
                CameraLockService.GetLockOrigin(out var x, out var y, out var z);
                C.CameraPosX = x;
                C.CameraPosY = y;
                C.CameraPosZ = z;
            }

            C.CameraPosMode = posMode;
            C.Save();
        }

        if (C.CameraPosMode == CameraPosMode.Manual
            && DrawWorldXyzRow("##cameraPosXYZ", "cameraPos", C.CameraPosX, C.CameraPosY, C.CameraPosZ, out var posX, out var posY, out var posZ))
        {
            C.CameraPosX = posX;
            C.CameraPosY = posY;
            C.CameraPosZ = posZ;
            C.Save();
        }

        if (DrawEnumCombo("Angle", "cameraAngleMode", C.CameraAngleMode, CameraAngleModeLabels, out var angleMode))
        {
            C.CameraAngleMode = angleMode;
            C.Save();
        }

        if (C.CameraAngleMode == CameraAngleMode.LookAt)
        {
            if (DrawWorldXyzRow("##lookAtXYZ", "lookAt", C.LookAtX, C.LookAtY, C.LookAtZ, out var lookX, out var lookY, out var lookZ))
            {
                C.LookAtX = lookX;
                C.LookAtY = lookY;
                C.LookAtZ = lookZ;
                C.Save();
            }
        }
        else if (C.CameraAngleMode == CameraAngleMode.Manual)
        {
            if (DrawYawPitchRow())
                C.Save();
        }

        DrawSavedCheckbox(
            "Disable WASD input (Duty Replay)",
            C.DisableWasdInput,
            value => C.DisableWasdInput = value,
            "Stops WASD from moving the locked camera during duty replay. Does not block walking outside replay. Mouse look is unchanged.");

        DrawSavedCheckbox(
            "Disable Arrow input",
            C.DisableArrowInput,
            value => C.DisableArrowInput = value,
            "Stops arrow keys from moving the locked camera. Mouse look is unchanged.");

        DrawSavedCheckbox(
            "Disable Zoom on Mousewheel",
            C.DisableMouseWheelZoom,
            value => C.DisableMouseWheelZoom = value);

        if (C.DisableMouseWheelZoom)
        {
            var zoom = C.CameraZoom;
            if (ImGui.SliderFloat("Zoom", ref zoom, CameraLockService.MinZoom, CameraLockService.MaxZoom, "%.2f"))
            {
                C.CameraZoom = zoom;
                C.Save();
            }
        }
    }

    private static bool DrawDisplayMode(
        string label,
        string id,
        EntityDisplayMode current,
        out EntityDisplayMode next)
    {
        var selected = (int)current;
        if (selected < 0 || selected >= DisplayModeLabels.Length)
            selected = 0;

        if (!DrawLabeledCombo(label, id, ref selected, DisplayModeLabels))
        {
            next = current;
            return false;
        }

        next = (EntityDisplayMode)selected;
        return true;
    }

    private static bool DrawEnumCombo<T>(string label, string id, T current, string[] labels, out T next)
        where T : struct, Enum
    {
        var selected = Convert.ToInt32(current);
        if (selected < 0 || selected >= labels.Length)
            selected = 0;

        if (!DrawLabeledCombo(label, id, ref selected, labels))
        {
            next = current;
            return false;
        }

        next = (T)Enum.ToObject(typeof(T), selected);
        return true;
    }

    private static bool DrawLabeledCombo(string label, string id, ref int selected, string[] labels)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(label);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        return ImGui.Combo($"##{id}", ref selected, labels, labels.Length);
    }

    private static bool DrawYawPitchRow()
    {
        if (!BeginStretchRow("##cameraYawPitch", 2))
            return false;

        var changed = false;
        ImGui.TableNextColumn();
        var yaw = C.CameraYaw;
        if (DrawAxisInput("Yaw", "cameraYaw", ref yaw, "%.1f"))
        {
            C.CameraYaw = yaw;
            changed = true;
        }

        ImGui.TableNextColumn();
        var pitch = C.CameraPitch;
        if (DrawAxisInput("Pitch", "cameraPitch", ref pitch, "%.1f"))
        {
            C.CameraPitch = pitch;
            changed = true;
        }

        ImGui.EndTable();
        Hint("Degrees. 0 yaw is north, clockwise. 0 pitch is horizontal, positive looks up.");
        return changed;
    }

    private static bool DrawWorldXyzRow(
        string tableId,
        string idPrefix,
        float x,
        float y,
        float z,
        out float nextX,
        out float nextY,
        out float nextZ)
    {
        nextX = x;
        nextY = y;
        nextZ = z;
        var picking = WorldPick.IsActive(idPrefix);
        var changed = false;

        var buttonSize = ImGui.GetFrameHeight();
        var gap = ImGui.GetStyle().ItemSpacing.X;
        var buttons = 3;
        if (!ImGui.BeginTable(
                tableId,
                3,
                ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.NoPadOuterX,
                new Vector2(-(buttons * buttonSize + buttons * gap), 0f)))
            return changed;

        ImGui.TableNextColumn();
        if (DrawAxisInput("X", $"{idPrefix}X", ref nextX))
            changed = true;

        ImGui.TableNextColumn();
        if (DrawAxisInput("Y", $"{idPrefix}Y", ref nextY))
            changed = true;

        ImGui.TableNextColumn();
        if (DrawAxisInput("Z", $"{idPrefix}Z", ref nextZ))
            changed = true;

        ImGui.EndTable();
        ImGui.SameLine();
        if (DrawWorldIconButton($"pick{idPrefix}", FontAwesomeIcon.Crosshairs, picking))
        {
            WorldPick.Toggle(idPrefix);
            picking = WorldPick.IsActive(idPrefix);
            changed = true;
        }

        Hint("Click the field to set this position. Click the icon again to cancel.");

        ImGui.SameLine();
        if (DrawWorldIconButton($"self{idPrefix}", FontAwesomeIcon.User, false)
            && TryGetLocalPlayerPosition(out var self))
        {
            nextX = self.X;
            nextY = self.Y;
            nextZ = self.Z;
            WorldPick.Cancel();
            picking = false;
            changed = true;
        }

        Hint("Set to your position.");

        ImGui.SameLine();
        if (DrawWorldIconButton($"target{idPrefix}", FontAwesomeIcon.Bullseye, false)
            && TryGetTargetPosition(out var target))
        {
            nextX = target.X;
            nextY = target.Y;
            nextZ = target.Z;
            WorldPick.Cancel();
            picking = false;
            changed = true;
        }

        Hint("Set to current target position.");
        return changed;
    }

    private static bool DrawWorldIconButton(string id, FontAwesomeIcon icon, bool active)
    {
        if (active)
            ImGui.PushStyleColor(ImGuiCol.Button, WorldPickButtonColor);

        bool clicked;
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            clicked = ImGui.Button(
                $"{icon.ToIconString()}##{id}",
                new Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeight()));
        }

        if (active)
            ImGui.PopStyleColor();

        return clicked;
    }

    private static bool TryGetLocalPlayerPosition(out Vector3 position)
    {
        var local = PluginServices.ObjectTable.LocalPlayer;
        if (local == null)
        {
            position = default;
            return false;
        }

        position = local.Position;
        return true;
    }

    private static unsafe bool TryGetTargetPosition(out Vector3 position)
    {
        position = default;
        var target = PluginServices.TargetManager.Target
            ?? PluginServices.TargetManager.SoftTarget;
        if (target == null)
        {
            var ts = TargetSystem.Instance();
            ulong id = ts != null ? ts->GetTargetObjectId() : 0;
            if (id != 0)
            {
                target = PluginServices.ObjectTable.SearchById(id);
                var local = PluginServices.ObjectTable.LocalPlayer;
                if (target == null && local != null && local.GameObjectId == id)
                    target = local;
            }
        }

        if (target == null)
            return false;

        position = target.Position;
        return true;
    }

    private static bool DrawAxisInput(string label, string id, ref float value, string format = "%.3f")
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(label);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        return ImGui.InputFloat($"##{id}", ref value, 0f, 0f, format);
    }

    private static void DrawSavedCheckbox(string label, bool current, Action<bool> set, string? tooltip = null)
    {
        var value = current;
        if (ImGui.Checkbox(label, ref value))
        {
            set(value);
            C.Save();
        }

        if (tooltip != null)
            Hint(tooltip);
    }

    private static bool IsCammyLoaded()
    {
        foreach (var plugin in PluginServices.PluginInterface.InstalledPlugins)
        {
            if (plugin.InternalName == "Cammy" && plugin.IsLoaded)
                return true;
        }

        return false;
    }

    private static void Hint(string text)
    {
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(text);
    }

    private static bool BeginStretchRow(string id, int columns) =>
        ImGui.BeginTable(
            id,
            columns,
            ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.NoPadOuterX,
            new Vector2(-1f, 0f));
}
