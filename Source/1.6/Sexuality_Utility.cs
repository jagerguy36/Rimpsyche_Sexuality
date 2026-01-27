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
            if (physicalPref >= 0f)
            {
                physicalPref = 2f - 4f / (2f + physicalPref);
                return 1f + (0.85f * physicalPref);
            }
            else
            {
                physicalPref = 4f / (2f - physicalPref) - 2f;
                return 1f / (1f - (1.5f * physicalPref));
            }
        }
    }
}
