using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class HairStylePreferenceWorker : PreferenceWorker
    {
        public class HairStylePrefBias
        {
            public string targetTag;
            public float weight;
            public List<GeneDef> NulifyingGeneList;
        }
        public enum StyleCategory
        {
            // Hair
            Bald, HairAny, Balding, Shaved, HairShort, HairMid, HairLong,
            // Beards
            NoBeard, BeardAny, BeardShort, BeardMid, BeardLong,
            // Count helper
            Count 
        }
        public static readonly HashSet<int>[] StyleBuckets = new HashSet<int>[(int)StyleCategory.Count];

        public static List<HairStylePrefBias> maleHairPrefBias;
        public static List<HairStylePrefBias> maleHairDislikeBias;
        public static List<HairStylePrefBias> maleBeardPrefBias;
        public static List<HairStylePrefBias> maleBeardDislikeBias;

        public static List<HairStylePrefBias> femaleHairPrefBias;
        public static List<HairStylePrefBias> femaleHairDislikeBias;
        public static List<HairStylePrefBias> femaleBeardPrefBias;
        public static List<HairStylePrefBias> femaleBeardDislikeBias;

        private static float mhTotal;
        private static float mhdTotal;
        private static float mbTotal;
        private static float mbdTotal;

        private static float fhTotal;
        private static float fhdTotal;
        private static float fbTotal;
        private static float fbdTotal;

        public HairStylePreferenceWorker()
        {
            for (int i = 0; i < StyleBuckets.Length; i++)
            {
                StyleBuckets[i] = new HashSet<int>();
            }
            var allHairDefs = DefDatabase<HairDef>.AllDefsListForReading;
            foreach(var hairdef in allHairDefs)
            {
                var taglist = hairdef.styleTags;
                //Bald and any
                if(taglist.Contains('Bald'))
                {
                    StyleBuckets[(int)StyleCategory.Bald].Add(hairdef.shortHash);
                    continue
                }
                StyleBuckets[(int)StyleCategory.HairAny].Add(hairdef.shortHash);
                if(taglist.Contains('Balding'))
                {
                    StyleBuckets[(int)StyleCategory.Balding].Add(hairdef.shortHash);
                    continue;
                }
                if(taglist.Contains('Shaved'))
                {
                    StyleBuckets[(int)StyleCategory.Shaved].Add(hairdef.shortHash);
                    continue;
                }
                int lengthIndex = 0;
                foreach(var tag in taglist)
                {
                    if (tag == "HairShort")
                        lengthIndex += 1;
                    else if (tag == "HairLong")
                        lengthIndex += 2;
                }
                if (lengthIndex == 1)
                    StyleBuckets[(int)StyleCategory.HairShort].Add(hairdef.shortHash);
                else if (lengthIndex == 2)
                    StyleBuckets[(int)StyleCategory.HairLong].Add(hairdef.shortHash);
                else
                    StyleBuckets[(int)StyleCategory.HairMid].Add(hairdef.shortHash);
            }

            var allBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading;
            foreach(var bearddef in allBeardDefs)
            {
                var taglist = bearddef.styleTags;
                //NoBeard and any
                if(taglist.Contains('NoBeard'))
                {
                    StyleBuckets[(int)StyleCategory.NoBeard].Add(bearddef.shortHash);
                    continue
                }
                StyleBuckets[(int)StyleCategory.BeardAny].Add(bearddef.shortHash);
                int lengthIndex = 0;
                foreach(var tag in taglist)
                {
                    if (tag == "BeardShort")
                        lengthIndex += 1;
                    else if (tag == "BeardLong")
                        lengthIndex += 2;
                }
                if (lengthIndex == 1)
                    StyleBuckets[(int)StyleCategory.BeardShort].Add(bearddef.shortHash);
                else if (lengthIndex == 2)
                    StyleBuckets[(int)StyleCategory.BeardLong].Add(bearddef.shortHash);
                else
                    StyleBuckets[(int)StyleCategory.BeardMid].Add(bearddef.shortHash);
            }
            EditorHeight = 200f;
            mhTotal = CalculateTotal(maleHairPrefBias);
            mhdTotal = CalculateTotal(maleHairDislikeBias);
            mbTotal = CalculateTotal(maleBeardPrefBias);
            mbdTotal = CalculateTotal(maleBeardDislikeBias);

            fhTotal = CalculateTotal(femaleHairPrefBias);
            fhdTotal = CalculateTotal(femaleHairDislikeBias);
            fbTotal = CalculateTotal(femaleBeardPrefBias);
            fbdTotal = CalculateTotal(femaleBeardDislikeBias);
        }
        private static float CalculateTotal(List<HairStylePrefBias> list)
        {
            if (list == null) return 0f;
            float sum = 0f;
            for (int i = 0; i < list.Count; i++)
            {
                sum += list[i].weight;
            }
            return sum;
        }

        public override bool TryGenerate(Pawn pawn, out List<PrefEntry> pref)
        {
            pref = new List<PrefEntry>();
            // Process Male Hair (Indices 0, 1)
            AddPreferencePair(pref, maleHairPrefBias, mhTotal, maleHairDislikeBias, mhdTotal);
            // Process Male Beard (Indices 2, 3)
            AddPreferencePair(pref, maleBeardPrefBias, mbTotal, maleBeardDislikeBias, mbdTotal);
            // Process Female Hair (Indices 4, 5)
            AddPreferencePair(pref, femaleHairPrefBias, fhTotal, femaleHairDislikeBias, fhdTotal);
            // Process Female Beard (Indices 6, 7)
            AddPreferencePair(pref, femaleBeardPrefBias, fbTotal, femaleBeardDislikeBias, fbdTotal);

            return true; // Return true so the generation is accepted
        }

        private void AddPreferencePair(List<PrefEntry> prefList, List<HairStylePrefBias> pList, float pTotal, List<HairStylePrefBias> dList, float dTotal)
        {
            string pTag = GetWeightedTag(pList, pTotal);
            string dTag = GetWeightedTag(dList, dTotal);
            if (pTag == dTag)
            {
                pTag = "NoPref";
                dTag = "NoPref";
            }
            prefList.Add(new PrefEntry(pTag, -1, 0f, 0f));
            prefList.Add(new PrefEntry(dTag, -1, 0f, 0f));
        }
        private string GetWeightedTag(List<HairStylePrefBias> list, float totalWeight)
        {
            if (list == null || list.Count == 0 || totalWeight <= 0) return "NoPref";

            float roll = Rand.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < list.Count; i++)
            {
                cumulative += list[i].weight;
                if (roll < cumulative)
                {
                    return list[i].targetTag;
                }
            }
            return list[list.Count - 1].targetTag;
        }

        public override string Report(Pawn pawn)
        {
            return "";
        }

        public override float Evaluate(Pawn observer, Pawn target, float result, bool isRomantic)
        {
            return result;
        }


        public override void DrawEditor(Rect rect, Pawn pawn, bool EditEnabled)
        {
            return;
        }
        public abstract void PostLoadAdjustment(Dictionary<string, List<PrefEntry>> _preference)
        {
            return;
        }
        public override void ClearEditorCache()
        {
            return;
        }
    }
}
