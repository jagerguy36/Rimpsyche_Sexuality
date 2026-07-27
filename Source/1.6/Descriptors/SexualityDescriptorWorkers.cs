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
            float loyalty = compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Loyalty);
            float passion = compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
            float loveC = loyalty * (3 + passion) / 4f; // -1 ~ 1
            float social = compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Sociability); // -1 ~ 1
            return (5f * loveC + social ) / 6f;
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Loyalty);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Passion);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Sociability);
        }
    }
    public class BoldRomantistDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            float confidence = compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Confidence);
            float passion = compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion);
            return (confidence + passion) * 0.5f;
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Confidence);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Passion);
        }
    }
    public class SexualOpennessDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            float openness = compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness);
            float experimental = Mathf.Max(0f, compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Experimentation));
            return (openness + 0.5f * experimental) / 1.5f;
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Openness);
            if(compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Experimentation) > 0f)
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Experimentation);
        }
    }
    public class CooperativeRomantistDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            var cooperative = -Mathf.Min(compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Competitiveness), 0f); //0~1
            var passion = compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Passion); // -1~1
            return Mathf.Max(0f, 0.8f * cooperative - 0.2f * passion);
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Competitiveness) < 0f)
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Competitiveness, PsycheDescDirection.Negative);
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Passion, PsycheDescDirection.Negative);
        }
    }
    public class ConsiderateRomantistDescriptorWorker : PsycheDescriptorWorker
    {
        protected override float Score(CompPsyche compPsyche)
        {
            var tracker = compPsyche.Personality;
            var confidence = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Confidence);
            var openness = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Openness);
            var optimism = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Optimism);
            var compassion = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_Compassion);
            var selfInterest = tracker.GetPersonality(PersonalityDefOf.Rimpsyche_SelfInterest);
            var confidenceFactor = 0.15f * confidence; //-0.15~0.15
            //Optimism > 0.5
            var hopefulnessFactor = 0.1f * Mathf.Max(0f, openness) * Mathf.Max(0f, optimism - 0.5f);//0~0.05
            //Optimism < -0.5
            var pessimisticFactor = 0.05f * (1f + Mathf.Max(0f, -openness)) * Mathf.Min(0f, optimism + 0.5);//0.05 * (1~2) * (-0.5~0): -0.05~0
            var entitlementFactor = 0.1f * Mathf.Max(0f, -compassion) * Mathf.Max(0f, selfInterest);//0~0.1
            //-0.2~0.3
            var collected = confidenceFactor + hopefulnessFactor + pessimisticFactor + entitlementFactor;
            return collected / -0.3f;
        }
        protected override void Evaluate(StringBuilder ctx, CompPsyche compPsyche, float score)
        {
            Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Confidence, PsycheDescDirection.Negative);
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Optimism) > 0.5f && compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness) > 0f)
            {
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Openness, PsycheDescDirection.Negative);
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Optimism, PsycheDescDirection.Negative);
            }
            if(compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Optimism) < -0.5f)
            {
                if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Openness) < 0f)
                    Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Openness, PsycheDescDirection.Negative);
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Optimism, PsycheDescDirection.Negative);
            }
            if (compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_Compassion) < 0f && compPsyche.Personality.GetPersonality(PersonalityDefOf.Rimpsyche_SelfInterest) > 0f)
            {
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_Compassion, PsycheDescDirection.Negative);
                Blame(ctx, compPsyche, PersonalityDefOf.Rimpsyche_SelfInterest);
            }
        }
    }
}
