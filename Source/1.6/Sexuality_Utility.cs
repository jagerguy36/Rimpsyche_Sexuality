using LudeonTK;
using System.Text;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public static class Sexuality_Utility
    {
        [DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
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
                for (int i = 0; i < psychePref.Count; i++)
                {
                    if (string.IsNullOrEmpty(psychePref[i].stringKey)) continue;
                    if (parts.Length > 0) parts.Append(" | ");
                    parts.Append($"{pawn.Name}'s preference || {psychePref[i].stringKey}: {psychePref[i].target:F2} ({psychePref[i].importance:F2})");
                }
                Log.Message(parts.ToString());
            }
        }
        [DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        private static void DisplayRelationsInfoWithPreference(Pawn pawn)
        {
            List<TableDataGetter<Pawn>> list = new List<TableDataGetter<Pawn>>
        {
            new TableDataGetter<Pawn>("name", (Pawn p) => p.LabelCap),
            new TableDataGetter<Pawn>("kind label", (Pawn p) => p.KindLabel),
            new TableDataGetter<Pawn>("gender", (Pawn p) => p.gender.GetLabel()),
            new TableDataGetter<Pawn>("age", (Pawn p) => p.ageTracker.AgeBiologicalYears),
            new TableDataGetter<Pawn>("my compat", (Pawn p) => pawn.relations.CompatibilityWith(p).ToString("F2")),
            new TableDataGetter<Pawn>("their compat", (Pawn p) => p.relations.CompatibilityWith(pawn).ToString("F2")),
            new TableDataGetter<Pawn>("my 2nd\nrom chance", (Pawn p) => pawn.relations.SecondaryRomanceChanceFactor(p).ToStringPercent("F0")),
            new TableDataGetter<Pawn>("their 2nd\nrom chance", (Pawn p) => p.relations.SecondaryRomanceChanceFactor(pawn).ToStringPercent("F0")),
            new TableDataGetter<Pawn>("lovin mtb", (Pawn p) => LovePartnerRelationUtility.GetLovinMtbHours(pawn, p).ToString("F1") + " h"),
            new TableDataGetter<Pawn>("psychePref", (Pawn p) => DefOfRimpsycheSexuality.Rimpsyche_PsychePreference.worker.Evaluate(pawn, p, 1f).ToString("F2"))
        };
            DebugTables.MakeTablesDialog(from x in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
                                         where x != pawn && x.RaceProps.Humanlike
                                         select x, list.ToArray());
        }
    }
}
