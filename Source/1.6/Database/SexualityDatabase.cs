using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [StaticConstructorOnStartup]
    public class SexualityDatabase
    {
        static SexualityDatabase()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (ModsConfig.IsActive("vexedtrees980.rimrobots"))
            {
                var MechanentDef = DefDatabase<ThingDef>.GetNamed("Mechanent", false);
                if (MechanentDef != null)
                    SexualityHelper.NonSexualDefShorthashSet.Add(MechanentDef.shortHash);
                else
                    Log.Warning("[Rimpsyche - Sexuality] vexedtrees980.rimrobots is loaded but Mechanent Def is not found.");
            }
        }
    }
}
