using HarmonyLib;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche.Sexuality.RPS_ROR_Compat
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            Log.Message("[Rimpsyche - Sexuality] Patching Romance on the Rim");
            var harmony = new Harmony("rimworld.mod.Maux.RimPsyche.Sexuality.ROR");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
