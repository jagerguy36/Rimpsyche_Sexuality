
namespace Maux36.RimPsyche.Sexuality
{
    public static class SexualityFormulaDB
    {
        //Shame mechanics
        public static RimpsycheFormula PsychePrefAuthSway = new(
            "PsychePrefAuthSway",
            (tracker) =>
            {
                float auth = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Authenticity);
                return 0.08f + 0.048f * auth; // 0.032~0.128 (0.2~0.16~0.8*0.16)
            },
            RimpsycheFormulaManager.FormulaIdDict
        );
    }
}
