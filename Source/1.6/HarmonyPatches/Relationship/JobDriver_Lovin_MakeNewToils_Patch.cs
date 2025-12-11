using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatchCategory("RPS_No_RJW")]
    [HarmonyPatch(typeof(JobDriver_Lovin), "MakeNewToils")]
    public static class JobDriver_Lovin_MakeNewToils_Patch
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_Lovin __instance, TargetIndex ___PartnerInd)
        {
            try
            {
                var pawn = __instance.pawn;
                var partner = (Pawn)__instance.job.GetTarget(___PartnerInd);
                var newToil = ToilMaker.MakeToil("RelationIncrease");
                newToil.AddFinishAction(delegate
                {
                    var pawnPsyche = pawn.compPsyche();
                    var partnerPsyche = partner.compPsyche();
                    if (pawnPsyche?.Enabled == true && partnerPsyche?.Enabled == true)
                    {
                        pawnPsyche.Sexuality.IncrementRelationshipWith(partner, 0.01f);
                        partnerPsyche.Sexuality.IncrementRelationshipWith(pawn, 0.01f);
                    }
                });
                __result = __result.AddItem(newToil);
            }
            catch (Exception e)
            {
                Log.Error($"[Rimpsyche - Sexuality] Failed patching lovin job: {e.Message}\n{e.StackTrace}");
            }
            foreach (var toil in __result)
            {
                yield return toil;
            }
        }
    }
}