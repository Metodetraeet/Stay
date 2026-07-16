using UnityEngine;
using Verse;

namespace Stay;

public class StaySettings : ModSettings
{
    public float callRangeCells = 30f;
    public int stayDurationTicks = 300;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref callRangeCells, "callRangeCells", 30f);
        Scribe_Values.Look(ref stayDurationTicks, "stayDurationTicks", 300);
    }
}

public class StayMod : Mod
{
    public static StaySettings Settings;

    public StayMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<StaySettings>();
    }

    public override string SettingsCategory() => "Stay!";
    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.Label($"Call range: {Settings.callRangeCells:0} cells",
            tooltip: "The maximum distance at which a colonist can call an animal to stay.");
        Settings.callRangeCells = Mathf.Round(listing.Slider(Settings.callRangeCells, 1f, 100f));
        listing.Gap();
        float staySeconds = Settings.stayDurationTicks / 60f;
        listing.Label($"Stay duration: {staySeconds:0} seconds",
            tooltip: "The duration for which the animal will stay after being called.");
        staySeconds = Mathf.Round(listing.Slider(staySeconds, 2f, 15f));
        Settings.stayDurationTicks = (int)(staySeconds * 60f);
        listing.End();
    }
}