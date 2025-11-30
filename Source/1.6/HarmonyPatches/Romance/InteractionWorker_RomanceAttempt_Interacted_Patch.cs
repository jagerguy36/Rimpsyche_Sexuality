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
            if (initPsyche?.Enabled != true) return;
            var initSexuality = initPsyche.Sexuality;

            //Learn orientation
            initSexuality.LearnOrientationOf(recipient);

            //Remember that initiator is attracted to recipient unless forced.
            if (initiator.CurJob?.def == JobDefOf.TryRomance) return;
            initSexuality.IncrementRelationshipWith(recipient, 0f);
        }
    }
}