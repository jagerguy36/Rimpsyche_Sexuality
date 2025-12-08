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
            //Relation module will implement preference on its own
            if (!Rimpsyche.RelationshipModuleLoaded && RimpsycheSettings.usePreferenceSystem)
            {
                harmony.PatchCategory("RPS_Preference");
            }
        }
    }
}
