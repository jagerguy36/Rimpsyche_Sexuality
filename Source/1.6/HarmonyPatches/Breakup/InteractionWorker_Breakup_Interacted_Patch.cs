using HarmonyLib;
using RimWorld;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    [HarmonyPatch(typeof(InteractionWorker_Breakup), "Interacted")]
    public static class InteractionWorker_Breakup_Interacted_Patch
    {
        public static void Postfix(Pawn initiator, Pawn recipient)
        {
            var iniPsyche = initiator.compPsyche();
            var reciPsyche = recipient.compPsyche();
            if (iniPsyche?.Enabled != true || reciPsyche?.Enabled != true) return;
            iniPsyche.Sexuality.ClampRelationshipWith(recipient, 0.05f);
            reciPsyche.Sexuality.ClampRelationshipWith(initiator, 0.05f);
        }
    }
}