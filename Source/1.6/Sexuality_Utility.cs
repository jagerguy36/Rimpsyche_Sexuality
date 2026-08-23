using RimWorld;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public static class Sexuality_Utility
    {
        public static float EvaluateRomPreference(Pawn pawn, Pawn otherPawn, float value)
        {
            //Only active preferences are stored in OrderedPrefDefs
            var prefDefs = RimpsycheDatabase.OrderedRomPreferenceDefs;
            for (int i = 0; i < prefDefs.Count; i++)
            {
                var def = prefDefs[i];
                value = def.worker.Evaluate(pawn, otherPawn, value, true);
            }
            return value;
        }

        //This takes both pawnBeauty and Physical preference into account, and is meant to replace vanilla PrettinessFactor.
        public static float EvaluatePhysicalAttractiveness(Pawn pawn, Pawn otherPawn)
        {
            var pawnPsyche = pawn.compPsyche();
            float prettiness = 0f;
            float physicalPref = 0f;
            if (otherPawn.RaceProps.Humanlike)
            {
                prettiness = otherPawn.GetStatValue(StatDefOf.PawnBeauty);
                prettiness = Mathf.Clamp(prettiness, -2.5f, 2.5f);
            }
            if (pawnPsyche?.Enabled == true)
            {
                prettiness *= pawnPsyche.Evaluate(SexualityFormula.AuthenticBeautyMultiplier);
                if (RimpsycheSexualitySettings.usePreferenceSystem)
                {
                    var prefDefs = RimpsycheDatabase.OrderedSexPreferenceDefs;
                    for (int i = 0; i < prefDefs.Count; i++)
                    {
                        var def = prefDefs[i];
                        physicalPref += def.worker.Evaluate(pawn, otherPawn, 0f, false);
                    }
                    //Translate prefDefs value unto -2~2 plane. This maps 2 to 1 and 3 to 1.5 and infinite+ to 2
                    //This means combined preference of 2 will have as much impact as having a 'pretty' trait, and 3 will have somewhere between pretty and beautiful.
                    if (physicalPref < 0f)
                        physicalPref = 4f / (2f - physicalPref) - 2f;
                    else if (physicalPref > 0f)
                        physicalPref = 2f - 4f / (2f + physicalPref);
                }
            }
            //Prettiness can range from -2.5 to 2.5
            //PhysicalPref all combined, can range from -2 to 2
            //All preferences combined cannot beat prettiness factor at its extreme.
            float finalScore = prettiness + physicalPref;
            if (finalScore < 0f)
            {
                return 1f / (1f - finalScore);
            }
            else if (finalScore >= 0f)
            {
                return 1f + (1f - (0.1f * finalScore)) * finalScore;
            }
            return 1f;
        }

        //Deprecated
        //This makes sexual preference act like prettiness factor, with multiple preference values still not going over the prettiness's influence.
        public static float EvaluateSexPreference(Pawn pawn, Pawn otherPawn, float attraction)
        {
            float physicalPref = 0f;
            //Only active preferences are stored in OrderedPrefDefs
            var prefDefs = RimpsycheDatabase.OrderedSexPreferenceDefs;
            for (int i = 0; i < prefDefs.Count; i++)
            {
                var def = prefDefs[i];
                physicalPref += def.worker.Evaluate(pawn, otherPawn, attraction, false);
            }
            //Translate value unto -2~2 plane. This maps 2 to 1 and 3 to 1.5 and infinite+ to 2
            //This means combined preference of 2 will have as much impact as having a 'pretty' trait, and 3 will have somewhere between pretty and beautiful.
            //Then use the prettiness calculation.
            if (physicalPref < 0f)
            {
                physicalPref = 4f / (2f - physicalPref) - 2f;
                return 1f / (1f - physicalPref);
            }
            if (physicalPref > 0f)
            {
                physicalPref = 2f - 4f / (2f + physicalPref);
                return 1f + (1f - (0.1f * physicalPref)) * physicalPref;
            }
            return 1f;
        }
    }
}
