using System.Text;
using UnityEngine;

namespace Maux36.RimPsyche.Sexuality
{
    public class AuthSwayDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            var authenticity = compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Authenticity);
            return authenticity;
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Authenticity);
        }
    }
    public class LoyalPartnerDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            var loyalty = compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Loyalty);
            return loyalty;
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Loyalty);
        }
    }
    public class MarriageProneDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            var loyalty = compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Loyalty);
            return loyalty;
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Loyalty);
        }
    }
}
