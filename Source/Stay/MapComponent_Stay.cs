using RimWorld;
using Verse;
using Verse.AI;

namespace Stay;

public class MapComponent_Stay : MapComponent
{
    private const int CheckIntervalTicks = 30;

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
            JobDef animalJob = animal.CurJobDef;
            if (animalJob == JobDefOf.Wait || animalJob == JobDefOf.Wait_MaintainPosture) continue;
            if (animalJob == JobDefOf.Flee || animalJob == JobDefOf.FleeAndCower) continue;
            if (!handler.Position.InHorDistOf(animal.Position, settings.callRangeCells)) continue;
            if (!GenSight.LineOfSight(handler.Position, animal.Position, map, skipFirstCell: true)) continue;

            Job stay = JobMaker.MakeJob(JobDefOf.Wait);
            stay.expiryInterval = settings.stayDurationTicks;
            animal.jobs.StartJob(stay, JobCondition.InterruptForced);
        }
    }
}

