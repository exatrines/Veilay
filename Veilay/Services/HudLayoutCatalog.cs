using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;

namespace Veilay.Services;

internal readonly record struct HudLayoutItem(string AddonName, string Label, string Group);

/// <summary>Native HUD Layout entries from HudLayoutAddon, not the full addon table.</summary>
internal static unsafe class HudLayoutCatalog
{
    private static readonly string[] GroupOrder =
    [
        "Action Bars",
        "Target",
        "Party & Lists",
        "Status",
        "Job Gauges",
        "Interface",
        "Duty",
        "PvP & Special",
    ];

    private static IReadOnlyList<HudLayoutItem> _items = [];
    private static IReadOnlyList<(string Group, IReadOnlyList<HudLayoutItem> Items)> _grouped = [];
    private static int _loadedLength = -1;

    internal static IReadOnlyList<HudLayoutItem> GetItems()
    {
        EnsureLoaded();
        return _items;
    }

    internal static IReadOnlyList<(string Group, IReadOnlyList<HudLayoutItem> Items)> GetGrouped()
    {
        EnsureLoaded();
        return _grouped;
    }

    private static void EnsureLoaded()
    {
        var length = HudLayoutAddon.GetLength();
        if (length <= 0 || _loadedLength == length)
            return;

        var array = HudLayoutAddon.GetArrayPointer();
        if (array == null)
            return;

        var sheet = PluginServices.DataManager.GetExcelSheet<RawRow>(ClientLanguage.English, "Hud");
        var items = new List<HudLayoutItem>(length);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < length; i++)
        {
            var entry = array[i];
            var addonName = entry.AddonName.ToString();
            if (string.IsNullOrEmpty(addonName) || !seen.Add(addonName))
                continue;

            items.Add(new HudLayoutItem(addonName, LabelFor(sheet, entry.HudRowId, addonName), GroupFor(addonName)));
        }

        _items = items;
        _grouped = BuildGrouped(items);
        _loadedLength = length;
    }

    private static IReadOnlyList<(string Group, IReadOnlyList<HudLayoutItem> Items)> BuildGrouped(
        IReadOnlyList<HudLayoutItem> items)
    {
        var buckets = new Dictionary<string, List<HudLayoutItem>>(StringComparer.Ordinal);
        foreach (var group in GroupOrder)
            buckets[group] = [];

        foreach (var item in items)
        {
            if (!buckets.TryGetValue(item.Group, out var list))
            {
                list = [];
                buckets[item.Group] = list;
            }

            list.Add(item);
        }

        var grouped = new List<(string Group, IReadOnlyList<HudLayoutItem> Items)>(buckets.Count);
        foreach (var group in GroupOrder)
        {
            if (buckets.TryGetValue(group, out var list) && list.Count > 0)
                grouped.Add((group, list));
        }

        foreach (var (group, list) in buckets)
        {
            if (list.Count == 0 || Array.IndexOf(GroupOrder, group) >= 0)
                continue;

            grouped.Add((group, list));
        }

        return grouped;
    }

    private static string LabelFor(ExcelSheet<RawRow>? sheet, uint hudRowId, string addonName)
    {
        if (sheet != null && sheet.TryGetRow(hudRowId, out var row))
        {
            for (var column = 0; column < Math.Min(3, row.Columns.Count); column++)
            {
                var text = row.ReadStringColumn(column).ToString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
        }

        return addonName.TrimStart('_');
    }

    private static string GroupFor(string addonName)
    {
        if (IsActionBar(addonName))
            return "Action Bars";

        if (Contains(addonName, "Target")
            || Contains(addonName, "CastBar")
            || Contains(addonName, "Focus"))
            return "Target";

        if (Contains(addonName, "PartyList")
            || Contains(addonName, "AllianceList")
            || Contains(addonName, "EnemyList"))
            return "Party & Lists";

        if (addonName.StartsWith("JobHud", StringComparison.Ordinal) || Contains(addonName, "Gauge"))
            return "Job Gauges";

        if (Contains(addonName, "Status")
            || Contains(addonName, "Param")
            || Contains(addonName, "Exp")
            || Contains(addonName, "Limit"))
            return "Status";

        if (IsPvpOrSpecial(addonName))
            return "PvP & Special";

        if (IsDuty(addonName))
            return "Duty";

        return "Interface";
    }

    private static bool IsActionBar(string addonName) =>
        addonName.StartsWith("_ActionBar", StringComparison.Ordinal)
        || addonName.StartsWith("_ActionCross", StringComparison.Ordinal)
        || addonName.StartsWith("_ActionDoubleCross", StringComparison.Ordinal)
        || addonName.StartsWith("_ActionPvp", StringComparison.Ordinal)
        || addonName is "_ActionContents" or "_ActionBarEx";

    private static bool IsDuty(string addonName) =>
        Contains(addonName, "ToDo")
        || Contains(addonName, "DutyList")
        || Contains(addonName, "ContentsInfo")
        || Contains(addonName, "Scenario")
        || Contains(addonName, "OceanFishing")
        || Contains(addonName, "NG+")
        || Contains(addonName, "Cosmo");

    private static bool IsPvpOrSpecial(string addonName) =>
        Contains(addonName, "PvP")
        || Contains(addonName, "Feast")
        || Contains(addonName, "Frontline")
        || Contains(addonName, "RivalWings")
        || Contains(addonName, "CConflict")
        || Contains(addonName, "Blunder")
        || Contains(addonName, "GFate");

    private static bool Contains(string addonName, string value) =>
        addonName.Contains(value, StringComparison.Ordinal);
}
