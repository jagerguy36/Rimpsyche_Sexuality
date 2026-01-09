using LudeonTK;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public static class SexualityDebugActions
    {
        [DebugAction("Rimpsyche", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void LogSexualityVariables(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche == null)
            {
                return;
            }
            var sexuality = compPsyche.Sexuality;
            Log.Message($"{pawn.Name}: Orientation: {sexuality.orientationCategory} | MKinsey: {sexuality.MKinsey} | Attraction: {sexuality.Attraction} | SexDrive: {sexuality.SexDrive}");

        }
        [DebugAction("Rimpsyche", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void LogAdultAge (Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche == null)
            {
                return;
            }
            Log.Message($"{pawn.Name}| Age: {pawn.ageTracker.AgeBiologicalYears} | Growth: {pawn.ageTracker.Growth} | MinAdultAge: {compPsyche.MinAdultAge} | FullAdultAge: {compPsyche.FullAdultAge}");

        }
        [DebugAction("Rimpsyche", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void LogPawnPsychePreference(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche != null)
            {
                var psychePref = compPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_PsychePreference);
                if (psychePref == null || psychePref.Count == 0)
                {
                    Log.Message("None");
                    return;
                }
                psychePref = [.. psychePref.OrderByDescending(p => p.importance)];
                var parts = new StringBuilder();
                parts.Append($"{pawn.Name}'s preference ||");
                for (int i = 0; i < psychePref.Count; i++)
                {
                    if (string.IsNullOrEmpty(psychePref[i].stringKey)) continue;
                    if (parts.Length > 0) parts.Append(" | ");
                    parts.Append($"{psychePref[i].stringKey}: {psychePref[i].target:F2} ({psychePref[i].importance:F2})");
                }
                Log.Message(parts.ToString());
            }
        }
        [DebugAction("Rimpsyche", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void DisplayRelationsInfoWithPreference(Pawn pawn)
        {
            var pawnPsyche = pawn.compPsyche();
            if (pawnPsyche?.Enabled != true)
                return;
            List<TableDataGetter<Pawn>> list = new List<TableDataGetter<Pawn>>
            {
                new TableDataGetter<Pawn>("name", (Pawn p) => p.LabelCap),
                new TableDataGetter<Pawn>("kind label", (Pawn p) => p.KindLabel),
                new TableDataGetter<Pawn>("gender", (Pawn p) => p.gender.GetLabel()),
                new TableDataGetter<Pawn>("age", (Pawn p) => p.ageTracker.AgeBiologicalYears),
                new TableDataGetter<Pawn>("prettinessFactor", (Pawn p) => pawn.relations.PrettinessFactor(p).ToString("F2")),
                new TableDataGetter<Pawn>("my Attraction", (Pawn p) => pawn.compPsyche().Sexuality.GetAdjustedAttraction(p)),
                new TableDataGetter<Pawn>("their Attraction", (Pawn p) => p.compPsyche().Sexuality.GetAdjustedAttraction(pawn)),
                new TableDataGetter<Pawn>("my 2nd\nlov factor", (Pawn p) => pawn.relations.SecondaryLovinChanceFactor(p).ToStringPercent("F0")),
                new TableDataGetter<Pawn>("their 2nd\nlov factor", (Pawn p) => p.relations.SecondaryLovinChanceFactor(pawn).ToStringPercent("F0")),
                new TableDataGetter<Pawn>("my 2nd\nrom chance", (Pawn p) => pawn.relations.SecondaryRomanceChanceFactor(p).ToStringPercent("F0")),
                new TableDataGetter<Pawn>("their 2nd\nrom chance", (Pawn p) => p.relations.SecondaryRomanceChanceFactor(pawn).ToStringPercent("F0")),
                new TableDataGetter<Pawn>("my Romance Attempt Chance", (Pawn p) => InteractionDefOf.RomanceAttempt.Worker.RandomSelectionWeight(pawn, p)),
                new TableDataGetter<Pawn>("their Romance Attempt Chance", (Pawn p) => InteractionDefOf.RomanceAttempt.Worker.RandomSelectionWeight(p, pawn)),
                new TableDataGetter<Pawn>("my Romance Success Chance", (Pawn p) => InteractionWorker_RomanceAttempt.SuccessChance(pawn, p)),
                new TableDataGetter<Pawn>("their Romance Success Chance", (Pawn p) => InteractionWorker_RomanceAttempt.SuccessChance(p, pawn)),
                new TableDataGetter<Pawn>("LovinAgeFactor", (Pawn p) => pawn.relations.LovinAgeFactor(p).ToString("F2")),
                new TableDataGetter<Pawn>("lovin mtb", (Pawn p) => LovePartnerRelationUtility.GetLovinMtbHours(pawn, p).ToString("F1") + " h"),
                new TableDataGetter<Pawn>("psychePref", (Pawn p) => DefOfRimpsycheSexuality.Rimpsyche_PsychePreference.worker.Evaluate(pawn, p, 1f, true).ToString("F2")),
                new TableDataGetter<Pawn>("stylePref", (Pawn p) => DefOfRimpsycheSexuality.Rimpsyche_HairStylePreference.worker.Evaluate(pawn, p, 1f, false).ToString("F2"))
            };
            DebugTables.MakeTablesDialog(from x in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
                                         where x != pawn && x.compPsyche()?.Enabled == true
                                         select x, list.ToArray());
        }
        [DebugAction("Rimpsyche", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void LogNextLovinTick(Pawn pawn)
        {
            var nlt = pawn.mindState.canLovinTick;
            var tickCurrent = Find.TickManager.TicksGame;
            Log.Message($"current tick: {tickCurrent} || NLT: {nlt} ... nextTickFactor: {Mathf.Min(1f / pawn.compPsyche().Sexuality.GetAdjustedSexdrive(), 60f)}");
        }
        [DebugAction("Rimpsyche", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void LogRelationship(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true)
                Log.Message($"{pawn.Name} has no psyche");

            var relationship = compPsyche.Sexuality.relationship;
            var pairs = relationship.Select(kvp => $"{kvp.Key}: {kvp.Value}");
            string logMessage = $"{pawn.Name} relationship = {{ {string.Join(", ", pairs)} }}";
            Log.Message($"{logMessage}");
        }
        [DebugAction("Rimpsyche", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void LogKnownOrientation(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true)
                Log.Message($"{pawn.Name} has no psyche");

            var orientation = compPsyche.Sexuality.knownOrientation;
            var pawnId = orientation.Select(kvp => $"{kvp}");
            string logMessage = $"{pawn.Name} knownOrientation = {{ {string.Join(", ", pawnId)} }}";
            Log.Message($"{logMessage}");
        }
    }
}
