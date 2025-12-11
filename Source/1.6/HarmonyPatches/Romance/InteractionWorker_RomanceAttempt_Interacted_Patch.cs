using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.Interacted))]
    public static class InteractionWorker_RomanceAttempt_Interacted_Patch
    {
        public static void Postfix(Pawn initiator, Pawn recipient)
        {
            var initPsyche = initiator.compPsyche();
            var reciPsyche = recipient.compPsyche();
            if (reciPsyche?.Enabled != true || initPsyche?.Enabled != true) return;
            
            var initSexuality = initPsyche.Sexuality;

            //Learn orientation
            reciPsyche.Sexuality.LearnOrientationOf(initiator);

            //Unless forced: Remember that initiator is attracted to recipient as well as learning orientation.
            //learning orientation is included in increment
            if (initiator.CurJob?.def != JobDefOf.TryRomance)
                initSexuality.IncrementRelationshipWith(recipient, 0f);
            else
                initSexuality.LearnOrientationOf(recipient);
        }
    }
}