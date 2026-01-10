
namespace Maux36.RimPsyche.Sexuality
{
    public static class SexualityFormula
    {
        //Psyche Attraction
        public static RimpsycheFormula PsychePrefAuthSway = new(
            "PsychePrefAuthSway",
            (tracker) =>
            {
                float auth = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Authenticity);
                return 0.1f + 0.06f * auth; // 0.04~0.16 (0.2*0.2~0.8*0.2)
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
        //Psyche Attraction
        public static RimpsycheFormula PhysicalPrefAuthSway = new(
            "PhysicalPrefAuthSway",
            (tracker) =>
            {
                float auth = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Authenticity);
                return 1f - 0.6f * auth; // 0.8(high auth) ~ 1.2(high superficial)
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
    }
}
