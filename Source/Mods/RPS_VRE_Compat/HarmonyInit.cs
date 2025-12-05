using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace Maux36.RimPsyche.Sexuality.RPS_VRE_Compat
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        public static HashSet<int> androidPawnkindShorthash = new HashSet<int>();
        static HarmonyInit()
        {
            Log.Message("[Rimpsyche - Sexuality] Patching VRE");
            var AndroidBasicKindDef = DefDatabase<PawnKindDef>.GetNamed("VREA_AndroidBasic", false);
            if (AndroidBasicKindDef != null)
            {
                androidPawnkindShorthash.Add(AndroidBasicKindDef.shortHash);
            }
            var AwakenedKindDef = DefDatabase<PawnKindDef>.GetNamed("VREA_AndroidAwakened", false);
            if (AwakenedKindDef != null)
            {
                androidPawnkindShorthash.Add(AwakenedKindDef.shortHash);
            }
            var harmony = new Harmony("rimworld.mod.Maux.RimPsyche.Sexuality.VRE");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
