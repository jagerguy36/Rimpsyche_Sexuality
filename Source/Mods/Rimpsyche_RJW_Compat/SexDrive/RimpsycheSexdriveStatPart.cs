using RimWorld;
using Verse;
namespace Maux36.RimPsyche.Sexuality.Rimpsyche_RJW_Compat
{
    public class RimpsycheSexdriveStatPart : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing is Pawn pawn)
            {
                var compPsyche = pawn.compPsyche();
                if (compPsyche?.Enabled == true)
                {
                    val *= compPsyche.Sexuality.GetAdjustedSexdrive();
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing is Pawn pawn)
            {
                var compPsyche = pawn.compPsyche();
                if (compPsyche?.Enabled == true)
                {
                    return "RP_Stat_Psyche".Translate() + "\n    " + "RP_Stat_Sexdrive".Translate() + ": x" + compPsyche.Sexuality.GetAdjustedSexdrive().ToStringPercent() + "\n";
                }
            }
            return null;
        }

    }
}
