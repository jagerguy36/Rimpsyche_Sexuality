using RimWorld;
using Verse;
namespace Maux36.RimPsyche.Sexuality.Rimpsyche_Intimacy_Compat
{
    public class RimpsycheIntimacySexdriveStatPart : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing is Pawn pawn)
            {
                var compPsyche = pawn.compPsyche();
                if (compPsyche?.Enabled == true)
                {
                    val *= 1.5f - compPsyche.Sexuality.SexDrive;
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
                    return "RP_Stat_Psyche".Translate() + "\n    " + "RP_Stat_Sexdrive".Translate() + ": x" + (1.5f - compPsyche.Sexuality.SexDrive).ToStringPercent() + "\n";
                }
            }
            return null;
        }

    }
}
