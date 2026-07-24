using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace Stay;

public class StaySettings : ModSettings
{
    public float callRangeCells = 30f;
    public int stayDurationTicks = 300;
    public bool showCallText = true;
    public List<string> excludedAnimals = new();

    private HashSet<string> excludedLookup;

    public bool IsExcluded(ThingDef def)
    {
        if (excludedAnimals.Count == 0) return false;
        excludedLookup ??= new HashSet<string>(excludedAnimals);
        return excludedLookup.Contains(def.defName);
    }

    public void SetExcluded(ThingDef def, bool excluded)
    {
        if (excluded) excludedAnimals.Add(def.defName);
        else excludedAnimals.Remove(def.defName);
        excludedLookup = null;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref callRangeCells, "callRangeCells", 30f);
        Scribe_Values.Look(ref stayDurationTicks, "stayDurationTicks", 300);
        Scribe_Values.Look(ref showCallText, "showCallText", true);
        Scribe_Collections.Look(ref excludedAnimals, "excludedAnimals", LookMode.Value);
        excludedAnimals ??= new List<string>();
        excludedLookup = null;
    }
}

public class StayMod : Mod
{
    public static StaySettings Settings;

    private static Vector2 scrollPos;
    private static string search = "";
    private static List<ThingDef> cachedAnimals;

private static List<ThingDef> AllAnimals =>
    cachedAnimals ??= DefDatabase<ThingDef>.AllDefsListForReading
        .Where(d => d.race != null && d.race.Animal && d.category == ThingCategory.Pawn)
        .OrderBy(d => d.label)
        .ToList();

    public StayMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<StaySettings>();
    }

    public override string SettingsCategory() => "Stay!";
    public override void DoSettingsWindowContents(Rect inRect)
    {
        var top = new Listing_Standard();
        top.Begin(inRect);
        top.Label($"Call range: {Settings.callRangeCells:0} cells",
            tooltip: "The maximum distance at which a colonist can call an animal to stay.");
        Settings.callRangeCells = Mathf.Round(top.Slider(Settings.callRangeCells, 1f, 100f));
        top.Gap();
        float staySeconds = Settings.stayDurationTicks / 60f;
        top.Label($"Stay duration: {staySeconds:0} seconds",
            tooltip: "The duration for which the animal will stay after being called.");
        staySeconds = Mathf.Round(top.Slider(staySeconds, 2f, 15f));
        Settings.stayDurationTicks = (int)(staySeconds * 60f);
        top.Gap();
        top.CheckboxLabeled("Show call text", ref Settings.showCallText,
            tooltip: "Toggle to show a text message when a colonist calls an animal to stay.");
        top.GapLine();
        top.Label("Animals that respond to calls:",
            tooltip: "Unchecked species follow vanilla behavior and ignore the call.");
        search = top.TextEntry(search);
        if (Settings.excludedAnimals.Count > 0 && top.ButtonText($"Allow all ({Settings.excludedAnimals.Count} excluded)"))
        {
            Settings.excludedAnimals.Clear();
        }
        float topHeight = top.CurHeight;
        top.End();
        var filtered = search.NullOrEmpty()
            ? AllAnimals
            : AllAnimals.Where(d => d.label.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        var outRect = new Rect(inRect.x, inRect.y + topHeight + 6f, inRect.width, inRect.height - topHeight - 6f);
        var viewRect = new Rect(0f, 0f, outRect.width - 16f, filtered.Count * 30f);
        Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);
        float y = 0f;
        foreach (var def in filtered)
        {
            Widgets.DefIcon(new Rect(0f, y + 2f, 24f, 24f), def);
            bool allowed = !Settings.IsExcluded(def);
            bool was = allowed;
            Widgets.CheckboxLabeled(new Rect(30f, y, viewRect.width - 30f, 28f), def.LabelCap, ref allowed);
            if (allowed != was) Settings.SetExcluded(def, !allowed);
            y += 30f;
        }
        Widgets.EndScrollView();
    }
}