using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{

    [HarmonyPatch(typeof(Pawn_SexualityTracker), nameof(Pawn_SexualityTracker.Initialize))]
    public static class Pawn_SexualityTracker_Initialize
    {
        static bool Prefix(Pawn_SexualityTracker __instance, Pawn pawn, bool generate)
        {
            Log.Message($"initializer called for {pawn.Name} with generate: {generate}");
            //Already initialized before
            if (__instance.orientationCategory != SexualOrientation.None && __instance.orientationCategory != SexualOrientation.Developing) return false;
            var traits = pawn.story?.traits;
            var gender = pawn.gender;
            //Not Applicable
            if (traits == null || gender == Gender.None) return false;
            if (pawn.ageTracker.AgeBiologicalYears < Rimpsyche_Utility.GetMinAdultAge(pawn))
            {
                __instance.orientationCategory = SexualOrientation.Developing;
                return false;
            }

            float attraction;
            //Decide sexuality
            if (generate)
            {
                var kinsey = SexualityHelper.GenerateKinsey();
                attraction = SexualityHelper.GenerateAttraction();
                __instance.kinsey = kinsey;
                if (attraction < 0.05f) __instance.orientationCategory = SexualOrientation.Asexual;
                else if (kinsey < 0.2f) __instance.orientationCategory = SexualOrientation.Heterosexual;
                else if (kinsey < 0.8f) __instance.orientationCategory = SexualOrientation.Bisexual;
                else __instance.orientationCategory = SexualOrientation.Homosexual;
                Log.Message($"generating sexuality for {pawn.Name} | {__instance.kinsey} | A: {attraction}");
            }
            //Compatibility
            else
            {
                if (traits.HasTrait(TraitDefOf.Gay)) __instance.orientationCategory = SexualOrientation.Homosexual;
                else if (traits.HasTrait(TraitDefOf.Bisexual)) __instance.orientationCategory = SexualOrientation.Bisexual;
                else if (traits.HasTrait(TraitDefOf.Asexual)) __instance.orientationCategory = SexualOrientation.Asexual;
                else __instance.orientationCategory = SexualOrientation.Heterosexual;
                __instance.kinsey = SexualityHelper.GenerateKinseyFor(__instance.orientationCategory);
                attraction = SexualityHelper.GenerateAttractionFor(__instance.orientationCategory);
                Log.Message($"interpreting sexuality for {pawn.Name} | {__instance.kinsey} | A: {attraction}");
            }

            if (__instance.kinsey < 0) //Un initialized.
            {
                Log.Error("Kinsey is uninitialized");
                return false;
            }
            else
            {
                //TODO: Rework sexdrive
                __instance.sexDrive = SexualityHelper.GenerateSexDriveFor(__instance.orientationCategory);
                float forSame = __instance.kinsey;
                float forDiff = 1f - __instance.kinsey;
                float multiplier = attraction / Mathf.Max(forSame, forDiff);
                if (gender == Gender.Male)
                {
                    __instance.mAattraction = multiplier * forSame;
                    __instance.fAattraction = multiplier * forDiff;
                }
                else
                {
                    __instance.mAattraction = multiplier * forDiff;
                    __instance.fAattraction = multiplier * forSame;
                }
            }
            return false;
        }
    }
}
