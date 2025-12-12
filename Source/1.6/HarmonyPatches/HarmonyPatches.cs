using HarmonyLib;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("rimworld.mod.Maux.RimPsyche.Sexuality");
            harmony.PatchAllUncategorized(Assembly.GetExecutingAssembly());
            
            if (!Rimpsyche.RelationshipModuleLoaded)
            {
                harmony.PatchCategory("RPS_No_RPR");

                //Relation module will implement preference on its own way
                if (RimpsycheSexualitySettings.usePreferenceSystem)
                    harmony.PatchCategory("RPS_Preference");
            }
            //Don't patch Jobdriver_lovin if RJW is loaded.
            if (!ModsConfig.IsActive("rim.job.world"))
            {
                harmony.PatchCategory("RPS_No_RJW");
            }
        }
    }
}
