using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel.Sheets;

namespace Veilay.Services;

/// <summary>Draws ImGui placeholders at dummy-replaced character positions.</summary>
internal sealed unsafe class MarkerOverlayService
{
    private const uint LabelOutline = 0xE0000000;

    private readonly VisibilityService _visibility;
    private string[]? _jobAbbreviations;

    public MarkerOverlayService(VisibilityService visibility) =>
        _visibility = visibility;

    public void Draw()
    {
        if (!VisibilityService.ModelsApplyReady)
            return;

        var dummy = _visibility.DummyIndexes;
        if (dummy.Count == 0)
            return;

        var manager = GameObjectManager.Instance();
        if (manager == null)
            return;

        var gui = PluginServices.GameGui;
        var draw = ImGui.GetBackgroundDrawList();
        var size = Math.Clamp(C.DummySize, 0.5f, 4f);

        for (var i = 0; i < VisibilityService.ObjectSlotCount; i++)
        {
            var native = manager->Objects.IndexSorted[i].Value;
            if (native == null || !dummy.Contains(native->ObjectIndex))
                continue;

            var position = GetDrawPosition(native);
            if (!gui.WorldToScreen(position, out var feet))
                continue;

            var height = native->GetHeight();
            if (height < 0.4f)
                height = 2f;

            var worldRadius = height * 0.12f * size;
            if (native->ObjectKind != ObjectKind.Pc)
                worldRadius *= 0.65f;

            var radius = WorldToScreenRadius(gui, position, worldRadius);
            if (radius < 1.5f)
                radius = 1.5f;

            var headWorld = position with { Y = position.Y + height };
            var headOk = gui.WorldToScreen(headWorld, out var head);
            var color = ColorFor(native);

            if (headOk)
            {
                draw.AddLine(feet, head, color, Math.Max(1.5f, radius * 0.18f));
                draw.AddCircleFilled(head, radius * 0.55f, color);
                DrawLabel(draw, native, head, radius, color);
            }

            draw.AddCircleFilled(feet, radius, color);
            draw.AddCircle(feet, radius, 0xE0FFFFFF, 12, 1.25f);
        }
    }

    private void DrawLabel(ImDrawListPtr draw, GameObject* native, Vector2 head, float radius, uint color)
    {
        var text = LabelFor(native);
        if (string.IsNullOrEmpty(text))
            return;

        var size = ImGui.CalcTextSize(text);
        var pos = new Vector2(head.X - size.X * 0.5f, head.Y - radius - size.Y - 2f);
        var fill = color | 0xFF000000;
        draw.AddText(pos + new Vector2(1f, 0f), LabelOutline, text);
        draw.AddText(pos + new Vector2(-1f, 0f), LabelOutline, text);
        draw.AddText(pos + new Vector2(0f, 1f), LabelOutline, text);
        draw.AddText(pos + new Vector2(0f, -1f), LabelOutline, text);
        draw.AddText(pos, fill, text);
    }

    private string? LabelFor(GameObject* native)
    {
        if (VisibilityService.IsEnemy(native) || VisibilityService.IsNpc(native))
        {
            var name = native->GetName().ToString();
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }

        if (native->ObjectKind != ObjectKind.Pc)
            return null;

        if (C.PlayerDummyLabel == PlayerDummyLabelMode.None)
            return null;

        var jobId = ((Character*)native)->ClassJob;
        if (C.PlayerDummyLabel == PlayerDummyLabelMode.Job)
            return JobAbbreviation(jobId);

        return RoleName(RoleForJob(jobId));
    }

    private string JobAbbreviation(byte jobId)
    {
        var abbreviations = JobAbbreviations();
        if (jobId < abbreviations.Length && !string.IsNullOrEmpty(abbreviations[jobId]))
            return abbreviations[jobId];

        return RoleName(RoleForJob(jobId));
    }

    private string[] JobAbbreviations()
    {
        if (_jobAbbreviations != null)
            return _jobAbbreviations;

        var names = new string[64];
        var sheet = PluginServices.DataManager.GetExcelSheet<ClassJob>(ClientLanguage.English);
        if (sheet != null)
        {
            foreach (var row in sheet)
            {
                if (row.RowId == 0 || row.RowId >= names.Length)
                    continue;

                var abbreviation = row.Abbreviation.ToString();
                if (!string.IsNullOrWhiteSpace(abbreviation))
                    names[row.RowId] = abbreviation;
            }
        }

        _jobAbbreviations = names;
        return names;
    }

    private static Vector3 GetDrawPosition(GameObject* native)
    {
        var drawObject = native->DrawObject;
        if (drawObject != null)
        {
            var drawn = drawObject->Position;
            return new Vector3(drawn.X, drawn.Y, drawn.Z);
        }

        var fallback = native->Position;
        return new Vector3(fallback.X, fallback.Y, fallback.Z);
    }

    private static float WorldToScreenRadius(IGameGui gui, Vector3 origin, float worldRadius)
    {
        if (!gui.WorldToScreen(origin, out var center))
            return 0f;

        var radius = 0f;
        if (gui.WorldToScreen(origin + new Vector3(worldRadius, 0f, 0f), out var alongX))
            radius = Math.Max(radius, Vector2.Distance(center, alongX));
        if (gui.WorldToScreen(origin + new Vector3(0f, worldRadius, 0f), out var alongY))
            radius = Math.Max(radius, Vector2.Distance(center, alongY));
        if (gui.WorldToScreen(origin + new Vector3(0f, 0f, worldRadius), out var alongZ))
            radius = Math.Max(radius, Vector2.Distance(center, alongZ));
        return radius;
    }

    private static uint ColorFor(GameObject* native)
    {
        if (VisibilityService.IsEnemy(native))
            return ToU32(C.DummyColorEnemy);

        if (VisibilityService.IsNpc(native))
            return ToU32(C.DummyColorNpc);

        if (native->ObjectKind != ObjectKind.Pc)
            return ToU32(C.DummyColorPet);

        return RoleForJob(((Character*)native)->ClassJob) switch
        {
            DummyRole.Tank => ToU32(C.DummyColorTank),
            DummyRole.Healer => ToU32(C.DummyColorHealer),
            DummyRole.Crafter => ToU32(C.DummyColorCrafter),
            DummyRole.Gatherer => ToU32(C.DummyColorGatherer),
            _ => ToU32(C.DummyColorDps),
        };
    }

    private static DummyRole RoleForJob(byte jobId) => jobId switch
    {
        1 or 3 or 19 or 21 or 32 or 37 => DummyRole.Tank,
        6 or 24 or 28 or 33 or 40 => DummyRole.Healer,
        >= 8 and <= 15 => DummyRole.Crafter,
        >= 16 and <= 18 => DummyRole.Gatherer,
        _ => DummyRole.Dps,
    };

    private static string RoleName(DummyRole role) => role switch
    {
        DummyRole.Tank => "Tank",
        DummyRole.Healer => "Healer",
        DummyRole.Crafter => "Crafter",
        DummyRole.Gatherer => "Gatherer",
        _ => "DPS",
    };

    private static uint ToU32(Vector4 color) => ImGui.ColorConvertFloat4ToU32(color);

    private enum DummyRole
    {
        Tank,
        Healer,
        Dps,
        Crafter,
        Gatherer,
    }
}
