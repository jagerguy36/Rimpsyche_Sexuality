using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class PsychePreferenceWorker : PreferenceWorker
    {
        private static readonly List<PersonalityDef> SourceDefs = DefDatabase<PersonalityDef>.AllDefsListForReading;
        private static readonly int nodeCount = SourceDefs.Count;
        private static readonly int[] IndexPool = Enumerable.Range(0, nodeCount).ToArray();
        public PsychePreferenceWorker()
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
        public override bool TryGenerate(Pawn pawn, out PrefEntry[] pref)
        {
            var pref = new PrefEntry[5];
            var relevantNodes = RandomFiveNodes();
            for (int i = 0; i < relevantNodes.Count; i++)
            {
                float importance = Rand.Range(0f, 1f);
                float target = Rand.Range(-1f, 1f);
                pref[i] = new PrefEntry(relevantNodes[i], target, importance);
            }
            return true;
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
