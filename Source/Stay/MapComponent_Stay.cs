using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using UnityEngine;

namespace Stay;

public class MapComponent_Stay : MapComponent
{
    private const int CheckIntervalTicks = 30;
    private const int TextCooldownTicks = 600;

    private readonly Dictionary <int, int> lastTextTick = new ();

    public MapComponent_Stay(Map map) : base(map) { }

    public override void MapComponentTick()
    {
        if (Find.TickManager.TicksGame % CheckIntervalTicks != 0) return;
        
        StaySettings settings = StayMod.Settings;

        var colonists = map.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < colonists.Count; i++)
        {
            Pawn handler = colonists[i];
            JobDef jobDef = handler.CurJobDef;
            if (jobDef != JobDefOf.Train && jobDef != JobDefOf.Tame) continue;

            Pawn animal = handler.CurJob?.targetA.Pawn;
            if (animal == null || !animal.Spawned || animal.Map != map) continue;
            if (animal.Faction != Faction.OfPlayer) continue;
            if (animal.pather == null || !animal.pather.Moving) continue;
            if (animal.Downed || animal.Dead || animal.InMentalState) continue;
            if (animal.roping != null && animal.roping.IsRoped) continue;
            Job animalJob = animal.CurJob;
            if (animalJob == null) continue;

            JobDef animalJobDef = animalJob.def;
            if (animalJobDef == JobDefOf.Wait || animalJobDef == JobDefOf.Wait_MaintainPosture) continue;
            if (animalJobDef == JobDefOf.Flee || animalJobDef == JobDefOf.FleeAndCower) continue;
            if (animalJob.playerForced) continue;
            if (!animalJobDef.suspendable ||
                !animalJobDef.casualInterruptible ||
                !animalJobDef.playerInterruptible) continue;
            if (animalJobDef.forceCompleteBeforeNextJob) continue;
            if (!handler.Position.InHorDistOf(animal.Position, settings.callRangeCells)) continue;
            if (!GenSight.LineOfSight(handler.Position, animal.Position, map, skipFirstCell: true)) continue;

            PawnUtility.ForceWait(animal, settings.stayDurationTicks, handler);

            if (settings.showCallText) ThrowCallText(handler, animal);
        }
    }

    private void ThrowCallText(Pawn handler, Pawn animal)
    {
        int now = Find.TickManager.TicksGame;
        if (lastTextTick.TryGetValue(animal.thingIDNumber, out int last) && now - last < TextCooldownTicks) return;
        if (lastTextTick.Count > 200) lastTextTick.Clear();
        lastTextTick[animal.thingIDNumber] = now;
        MoteMaker.ThrowText(handler.DrawPos + new Vector3(0f, 0f, 0.65f), map, "Stay!", 2.2f);
    }
}

