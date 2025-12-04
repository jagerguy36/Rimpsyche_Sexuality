using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche.Sexuality.Rimpsyche_Intimacy_Compat
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            Log.Message("[Rimpsyche - Sexuality] Patching Intimacy");
            var harmony = new Harmony("rimworld.mod.Maux.RimPsyche.Sexuality.Intimacy");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
