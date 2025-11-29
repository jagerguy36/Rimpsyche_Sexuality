using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.Interacted))]
    public static class InteractionWorker_RomanceAttempt_Interacted_Patch
    {
        public static void Postfix(Pawn initiator, Pawn recipient)
        {
            if (initiator.CurJob?.def == JobDefOf.TryRomance)
            {
                var initPsyche = initiator.compPsyche();
                if (initPsyche?.Enabled != true) return;
                initPsyche.Sexuality.IncrementRelationshipWith(recipient, 0f);
            }
        }
    }
}