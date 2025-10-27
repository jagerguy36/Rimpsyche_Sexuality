using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class PsychePerferenceWorker : PreferenceWorker
    {
        private static readonly List<PersonalityDef> SourceDefs = DefDatabase<PersonalityDef>.AllDefsListForReading;
        private static readonly int nodeCount = SourceDefs.Count;
        private static readonly int[] IndexPool = Enumerable.Range(0, nodeCount).ToArray();
        public PsychePerferenceWorker()
        {
            EditorHeight = 100f;
        }

        public static List<PersonalityDef> RandomFiveNodes()
        {
            const int selectCount = 5;
            for (int i = 0; i < selectCount; i++)
            {
                int randIndex = Rand.Range(i, nodeCount);
                (IndexPool[i], IndexPool[randIndex]) = (IndexPool[randIndex], IndexPool[i]);
            }
            List<PersonalityDef> result = new List<PersonalityDef>(selectCount);
            for (int i = 0; i < selectCount; i++)
            {
                result.Add(SourceDefs[IndexPool[i]]);
            }
            return result;
        }
        public override void SetUp(Pawn pawn)
        {
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true) return;
            var relevantNodes = RandomFiveNodes();
            for (int i = 0; i < relevantNodes.Count; i++)
            {
                float importance = Rand.Range(0f, 1f);

            }

        }

        public override string Report(Pawn pawn)
        {
            return "";
        }

        public override float Evaluate(Pawn obesrver, Pawn target)
        {
            return 0f;
        }

        public override void DrawEditor(Rect rect, Pawn pawn)
        {
            return;
        }
    }
}
