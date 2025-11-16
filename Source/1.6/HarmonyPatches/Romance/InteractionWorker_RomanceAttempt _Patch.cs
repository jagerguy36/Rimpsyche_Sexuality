using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight))]
    public static class InteractionWorker_RomanceAttempt_RandomSelectionWeight_Patch
    {
        //Prevent Less stupid romance attempt
        private static bool Prefix(Pawn initiator, Pawn recipient)
        {
            return true;
        }

        //Orientation knowledge respect
        private static void Postfix(ref float __result, Pawn initiator, Pawn recipient)
        {
            var initPsyche = initiator.compPsyche();
            if (initPsyche.Enabled != true) return;
            //confidence etc factor
            //
            //
            if (!initPsyche.Sexuality.knownOrientation.Contains(recipient.thingIDNumber)) return;
            var reciPsyche = recipient.compPsyche();
            if (reciPsyche.Enabled != true) return;
            var knownReciAttraction = reciPsyche.Sexuality.GetAdjustedAttraction(initiator.gender);
            //Case possible
            if (knownReciAttraction > 0f)
            {

            }
            //Case impossible
            else
            {

            }
        }
    }
}
