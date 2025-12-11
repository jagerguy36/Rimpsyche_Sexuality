using HarmonyLib;
using RomanceOnTheRim;
using System;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality.RPS_ROR_Compat
{
    [HarmonyPatch(typeof(LordJob_RomanticInteraction), "ApplyPartialOutcome")]
    public static class LordJob_RomanticInteraction_ApplyPartialOutcome_Patch
    {
        private static readonly Func<LordJob_RomanticInteraction, int> GetInteractionDuration =
            (Func<LordJob_RomanticInteraction, int>)Delegate.CreateDelegate(
                typeof(Func<LordJob_RomanticInteraction, int>),
                AccessTools.PropertyGetter(typeof(LordJob_RomanticInteraction), "InteractionDuration")
            );


        public static void Postfix(LordJob_RomanticInteraction __instance, Pawn ___Initiator, Pawn ___Recipient,  int ___StartInteractionTick)
        {
            if (__instance.Interacted)
            {
                int interactionDuration = GetInteractionDuration(__instance);
                float num = 0.02f * Mathf.Clamp01((float)(Find.TickManager.TicksGame - ___StartInteractionTick) / (float)interactionDuration);
                Log.Message($"Rimpsyche adding partial outcome to {___Initiator.Name} and {___Recipient.Name} by {num}");
            }
        }
    }


    [HarmonyPatch(typeof(LordJob_RomanticInteraction), "ApplyFinishOutcome")]
    public static class LordJob_RomanticInteraction_ApplyFinishOutcome_Patch
    {
        private static readonly Func<LordJob_RomanticInteraction, float> GetRomanceNeedGainFromInteraction =
            (Func<LordJob_RomanticInteraction, float>)Delegate.CreateDelegate(
                typeof(Func<LordJob_RomanticInteraction, float>),
                AccessTools.PropertyGetter(typeof(LordJob_RomanticInteraction), "RomanceNeedGainFromInteraction")
            );
        public static void Postfix(LordJob_RomanticInteraction __instance, Pawn ___Initiator, Pawn ___Recipient)
        {
            if (__instance.Interacted)
            {

                float RomanceNeedGainFromInteraction = GetRomanceNeedGainFromInteraction(__instance);
                float num = 0.02f * RomanceNeedGainFromInteraction;
                Log.Message($"Rimpsyche adding partial outcome to {___Initiator.Name} and {___Recipient.Name} by {num}");
            }
        }
    }
}
