using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class HairPreferenceDef : PreferenceDef
    {
        public class HairStylePrefBias
        {
            public string targetTag;
            public float weight;
            public List<GeneDef> NulifyingGeneList;
        }

        public List<HairStylePrefBias> maleHairPrefBias;
        public List<HairStylePrefBias> maleHairDislikeBias;
        public List<HairStylePrefBias> maleBeardPrefBias;
        public List<HairStylePrefBias> maleBeardDislikeBias;

        public List<HairStylePrefBias> femaleHairPrefBias;
        public List<HairStylePrefBias> femaleHairDislikeBias;
        public List<HairStylePrefBias> femaleBeardPrefBias;
        public List<HairStylePrefBias> femaleBeardDislikeBias;
    }

    public class HairStylePreferenceWorker : PreferenceWorker
    {
        public enum StyleCategory
        {
            // Hair
            Bald, HairAny, Balding, Shaved, HairShort, HairMid, HairLong,
            // Beards
            NoBeard, BeardAny, BeardShort, BeardMid, BeardLong,
            // Count helper
            Count
        }
        public static readonly HashSet<int>[] StyleBuckets = new HashSet<int>[(int)StyleCategory.Count+1];
        public static readonly HashSet<string>[] StyleLabelBuckets = new HashSet<string>[(int)StyleCategory.Count + 1];
        public static Dictionary<string, int> StyleBucketIndex = new Dictionary<string, int>
        {
            { "Bald", 0 },
            { "HairAny", 1 },
            { "Balding", 2 },
            { "Shaved", 3 },
            { "HairShort", 4 },
            { "HairMid", 5 },
            { "HairLong", 6 },
            { "NoBeard", 7 },
            { "BeardAny", 8 },
            { "BeardShort", 9 },
            { "BeardMid", 10 },
            { "BeardLong", 11 },
            { "NoPref", 12 }
        };
        public static Dictionary<int, string> LabelDict = new Dictionary<int, string>
        {
            { 0, "RPS_Bald".Translate() },
            { 1, "RPS_HairAny".Translate() },
            { 2, "RPS_Balding".Translate() },
            { 3, "RPS_Shaved".Translate() },
            { 4, "RPS_HairShort".Translate() },
            { 5, "RPS_HairMid".Translate() },
            { 6, "RPS_HairLong".Translate() },
            { 7, "RPS_NoBeard".Translate() },
            { 8, "RPS_BeardAny".Translate() },
            { 9, "RPS_BeardShort".Translate() },
            { 10, "RPS_BeardMid".Translate() },
            { 11, "RPS_BeardLong".Translate() },
            { 12, "RPS_NoPref".Translate() }
        };

        private static HairPreferenceDef hairPreferenceDef;
        private static float mhTotal;
        public HashSet<int> maleHairPrefBiasGenes = [];
        private static float mhdTotal;
        public HashSet<int> maleHairDislikeBiasGenes = [];
        private static float mbTotal;
        public HashSet<int> maleBeardPrefBiasGenes = [];
        private static float mbdTotal;
        public HashSet<int> maleBeardDislikeBiasGenes = [];

        private static float fhTotal;
        public HashSet<int> femaleHairPrefBiasGenes = [];
        private static float fhdTotal;
        public HashSet<int> femaleHairDislikeBiasGenes = [];
        private static float fbTotal;
        public HashSet<int> femaleBeardPrefBiasGenes = [];
        private static float fbdTotal;
        public HashSet<int> femaleBeardDislikeBiasGenes = [];

        public HairStylePreferenceWorker()
        {
            EditorHeight = rowHeight * 9f + titleContentSpacing * 2f;
            for (int i = 0; i < StyleBuckets.Length; i++)
            {
                StyleLabelBuckets[i] = new HashSet<string>();
                StyleBuckets[i] = new HashSet<int>();
            }
        }
        public override void PostInit()
        {
            hairPreferenceDef = def as HairPreferenceDef;
            var allHairDefs = DefDatabase<HairDef>.AllDefsListForReading;
            foreach (var hairdef in allHairDefs)
            {
                var taglist = hairdef.styleTags;
                //Bald and any
                if (taglist.Contains("Bald"))
                {
                    StyleBuckets[(int)StyleCategory.Bald].Add(hairdef.shortHash);
                    StyleLabelBuckets[(int)StyleCategory.Bald].Add(hairdef.label);
                    continue;
                }
                StyleBuckets[(int)StyleCategory.HairAny].Add(hairdef.shortHash);
                StyleLabelBuckets[(int)StyleCategory.HairAny].Add(hairdef.label);
                if (taglist.Contains("Balding"))
                {
                    StyleBuckets[(int)StyleCategory.Balding].Add(hairdef.shortHash);
                    StyleLabelBuckets[(int)StyleCategory.Balding].Add(hairdef.label);
                    continue;
                }
                if (taglist.Contains("Shaved"))
                {
                    StyleBuckets[(int)StyleCategory.Shaved].Add(hairdef.shortHash);
                    StyleLabelBuckets[(int)StyleCategory.Shaved].Add(hairdef.label);
                    continue;
                }
                int lengthIndex = 0;
                foreach (var tag in taglist)
                {
                    if (tag == "HairShort")
                        lengthIndex += 1;
                    else if (tag == "HairLong")
                        lengthIndex += 2;
                }
                if (lengthIndex == 1)
                {
                    StyleBuckets[(int)StyleCategory.HairShort].Add(hairdef.shortHash);
                    StyleLabelBuckets[(int)StyleCategory.HairShort].Add(hairdef.label);
                }
                else if (lengthIndex == 2)
                {
                    StyleBuckets[(int)StyleCategory.HairLong].Add(hairdef.shortHash);
                    StyleLabelBuckets[(int)StyleCategory.HairLong].Add(hairdef.label);
                }
                else
                {
                    StyleBuckets[(int)StyleCategory.HairMid].Add(hairdef.shortHash);
                    StyleLabelBuckets[(int)StyleCategory.HairMid].Add(hairdef.label);
                }
            }

            var allBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading;
            foreach (var bearddef in allBeardDefs)
            {
                var taglist = bearddef.styleTags;
                //NoBeard and any
                if (taglist.Contains("NoBeard"))
                {
                    StyleBuckets[(int)StyleCategory.NoBeard].Add(bearddef.shortHash);
                    StyleLabelBuckets[(int)StyleCategory.NoBeard].Add(bearddef.label);
                    continue;
                }
                StyleBuckets[(int)StyleCategory.BeardAny].Add(bearddef.shortHash);
                StyleLabelBuckets[(int)StyleCategory.BeardAny].Add(bearddef.label);
                int lengthIndex = 0;
                foreach (var tag in taglist)
                {
                    if (tag == "BeardShort")
                        lengthIndex += 1;
                    else if (tag == "BeardLong")
                        lengthIndex += 2;
                }
                if (lengthIndex == 1)
                {
                    StyleBuckets[(int)StyleCategory.BeardShort].Add(bearddef.shortHash);
                    StyleLabelBuckets[(int)StyleCategory.BeardShort].Add(bearddef.label);
                }
                else if (lengthIndex == 2)
                {
                    StyleBuckets[(int)StyleCategory.BeardLong].Add(bearddef.shortHash);
                    StyleLabelBuckets[(int)StyleCategory.BeardLong].Add(bearddef.label);
                }
                else
                {
                    StyleBuckets[(int)StyleCategory.BeardMid].Add(bearddef.shortHash);
                    StyleLabelBuckets[(int)StyleCategory.BeardMid].Add(bearddef.label);
                }
            }
            mhTotal = CalculateTotal(hairPreferenceDef.maleHairPrefBias, maleHairPrefBiasGenes);
            mhdTotal = CalculateTotal(hairPreferenceDef.maleHairDislikeBias, maleHairDislikeBiasGenes);
            mbTotal = CalculateTotal(hairPreferenceDef.maleBeardPrefBias, maleBeardPrefBiasGenes);
            mbdTotal = CalculateTotal(hairPreferenceDef.maleBeardDislikeBias, maleBeardDislikeBiasGenes);

            fhTotal = CalculateTotal(hairPreferenceDef.femaleHairPrefBias, femaleHairPrefBiasGenes);
            fhdTotal = CalculateTotal(hairPreferenceDef.femaleHairDislikeBias, femaleHairDislikeBiasGenes);
            fbTotal = CalculateTotal(hairPreferenceDef.femaleBeardPrefBias, femaleBeardPrefBiasGenes);
            fbdTotal = CalculateTotal(hairPreferenceDef.femaleBeardDislikeBias, femaleBeardDislikeBiasGenes);
        }
        private static float CalculateTotal(List<HairPreferenceDef.HairStylePrefBias> list, HashSet<int> GeneHash)
        {
            if (list == null) return 0f;
            float sum = 0f;
            for (int i = 0; i < list.Count; i++)
            {
                sum += list[i].weight;
                if (list[i].NulifyingGeneList != null)
                {
                    foreach (var gene in list[i].NulifyingGeneList)
                    {
                        GeneHash.Add(gene.shortHash);
                    }
                }
            }
            return sum;
        }

        public override bool TryGenerate(Pawn pawn, out List<PrefEntry> pref)
        {
            pref = new List<PrefEntry>();
            HashSet<int> pawnGenes = BuildPawnGeneHash(pawn);
            var mhOff = GetOffendingGenes(pawnGenes, maleHairPrefBiasGenes);
            var mhdOff = GetOffendingGenes(pawnGenes, maleHairDislikeBiasGenes);

            var mbOff = GetOffendingGenes(pawnGenes, maleBeardPrefBiasGenes);
            var mbdOff = GetOffendingGenes(pawnGenes, maleBeardDislikeBiasGenes);

            var fhOff = GetOffendingGenes(pawnGenes, femaleHairPrefBiasGenes);
            var fhdOff = GetOffendingGenes(pawnGenes, femaleHairDislikeBiasGenes);

            var fbOff = GetOffendingGenes(pawnGenes, femaleBeardPrefBiasGenes);
            var fbdOff = GetOffendingGenes(pawnGenes, femaleBeardDislikeBiasGenes);

            // Process Male Hair (Indices 0, 1)
            AddPreferencePair(pref,
                hairPreferenceDef.maleHairPrefBias, mhTotal, mhOff,
                hairPreferenceDef.maleHairDislikeBias, mhdTotal, mhdOff);

            // Process Male Beard (Indices 2, 3)
            AddPreferencePair(pref,
                hairPreferenceDef.maleBeardPrefBias, mbTotal, mbOff,
                hairPreferenceDef.maleBeardDislikeBias, mbdTotal, mbdOff);

            // Process Female Hair (Indices 4, 5)
            AddPreferencePair(pref,
                hairPreferenceDef.femaleHairPrefBias, fhTotal, fhOff,
                hairPreferenceDef.femaleHairDislikeBias, fhdTotal, fhdOff);

            // Process Female Beard (Indices 6, 7)
            AddPreferencePair(pref,
                hairPreferenceDef.femaleBeardPrefBias, fbTotal, fbOff,
                hairPreferenceDef.femaleBeardDislikeBias, fbdTotal, fbdOff);

            return true; // Return true so the generation is accepted
        }
        private static HashSet<int> BuildPawnGeneHash(Pawn pawn)
        {
            var result = new HashSet<int>();

            var genes = pawn.genes?.GenesListForReading;
            if (genes == null)
                return result;

            for (int i = 0; i < genes.Count; i++)
            {
                result.Add(genes[i].def.shortHash);
            }

            return result;
        }
        private static HashSet<int> GetOffendingGenes(HashSet<int> pawnGenes, HashSet<int> listNullifyingGenes)
        {
            if (listNullifyingGenes == null || listNullifyingGenes.Count == 0)
                return null;

            HashSet<int> result = null;

            foreach (int gene in listNullifyingGenes)
            {
                if (pawnGenes.Contains(gene))
                {
                    result ??= new HashSet<int>();
                    result.Add(gene);
                }
            }

            return result;
        }
        private void AddPreferencePair(List<PrefEntry> prefList,
            List<HairPreferenceDef.HairStylePrefBias> pList, float pTotal, HashSet<int> pOffendingGenes,
            List<HairPreferenceDef.HairStylePrefBias> dList, float dTotal, HashSet<int> dOffendingGenes)
        {
            string pTag = GetWeightedTag(pList, pTotal, pOffendingGenes);
            string dTag = GetWeightedTag(dList, dTotal, dOffendingGenes);
            if (pTag == dTag)
            {
                pTag = "NoPref";
                dTag = "NoPref";
            }
            prefList.Add(new PrefEntry(pTag, StyleBucketIndex[pTag], 0f, 0f));
            prefList.Add(new PrefEntry(dTag, StyleBucketIndex[dTag], 0f, 0f));
        }
        private string GetWeightedTag(List<HairPreferenceDef.HairStylePrefBias> list, float precomputedTotal, HashSet<int> offendingGenes)
        {
            if (list == null || list.Count == 0 || precomputedTotal <= 0f)
                return "NoPref";

            if (offendingGenes == null)
            {
                return GetWeightedTagFast(list, precomputedTotal);
            }

            float total = 0f;
            for (int i = 0; i < list.Count; i++)
            {
                if (!IsInvalidated(list[i], offendingGenes))
                    total += list[i].weight;
            }

            if (total <= 0f)
                return "NoPref";

            float roll = Rand.Range(0f, total);
            float cumulative = 0f;

            for (int i = 0; i < list.Count; i++)
            {
                if (IsInvalidated(list[i], offendingGenes))
                    continue;

                cumulative += list[i].weight;
                if (roll < cumulative)
                    return list[i].targetTag;
            }

            return "NoPref";
        }
        private static bool IsInvalidated( HairPreferenceDef.HairStylePrefBias bias, HashSet<int> offendingGenes)
        {
            if (offendingGenes == null || bias.NulifyingGeneList == null)
                return false;

            for (int i = 0; i < bias.NulifyingGeneList.Count; i++)
            {
                if (offendingGenes.Contains(bias.NulifyingGeneList[i].shortHash))
                    return true;
            }

            return false;
        }

        private string GetWeightedTagFast(List<HairPreferenceDef.HairStylePrefBias> list, float totalWeight)
        {
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
        public override float Evaluate(Pawn observer, Pawn target, float result, bool isRomantic)
        {
            if (RimpsycheSexualitySettings.usePreferenceSystem != true) return result;
            if (isRomantic) return result;
            var observerPsyche = observer.compPsyche();
            if (observerPsyche?.Enabled != true) return result;
            var targetPsyche = target.compPsyche();
            if (targetPsyche?.Enabled != true) return result;
            var hairstylePreference = observerPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_HairStylePreference);
            if (hairstylePreference == null)
                return result;
            float value = 1f;
            if (target.gender == Gender.Male)
            {
                //0:hair pref | 1: hair dislike | 2: beard pref | 3: beard dislike
                if (StyleBuckets[hairstylePreference[0].intKey].Contains(target.story.hairDef.shortHash))
                    value += 0.075f;
                else if (StyleBuckets[hairstylePreference[1].intKey].Contains(target.story.hairDef.shortHash))
                    value -= 0.075f;

                if (StyleBuckets[hairstylePreference[2].intKey].Contains(target.style.beardDef.shortHash))
                    value += 0.075f;
                else if (StyleBuckets[hairstylePreference[3].intKey].Contains(target.style.beardDef.shortHash))
                    value -= 0.075f;
            }
            else if (target.gender == Gender.Female)
            {
                //4:hair pref | 5: hair dislike | 6: beard pref | 7: beard dislike
                if (StyleBuckets[hairstylePreference[4].intKey].Contains(target.story.hairDef.shortHash))
                    value += 0.075f;
                else if (StyleBuckets[hairstylePreference[5].intKey].Contains(target.story.hairDef.shortHash))
                    value -= 0.075f;

                if (StyleBuckets[hairstylePreference[6].intKey].Contains(target.style.beardDef.shortHash))
                    value += 0.075f;
                else if (StyleBuckets[hairstylePreference[7].intKey].Contains(target.style.beardDef.shortHash))
                    value -= 0.075f;
            }
            return result * value;
        }

        private static readonly float rowHeight = 22f;
        private static List<(string, string)> drawerCache = null;
        public override float GetViewerHeight(Pawn pawn)
        {
            var cachedData = GetDrawerCache(pawn);
            if (cachedData == null)
                return rowHeight * 2f;
            float result = rowHeight * (cachedData.Count + 1f);
            return result;
        }
        private static List<(string, string)> GetDrawerCache(Pawn pawn)
        {
            if (drawerCache != null) return drawerCache;
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true)
                return null;
            var hairstylePref = compPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_HairStylePreference);
            if (hairstylePref == null)
                return null;
            drawerCache = new();
            TaggedString subtitle = "RPS_Likes".Translate() + "(<color=#ff0000>♥</color>), " + "RPS_Dislikes".Translate() + "(<color=#555555>♥</color>)";
            if (compPsyche.Sexuality.CanFeelAttractionToGender(Gender.Male))
            {
                bool hasHairP = hairstylePref[0].intKey != 12; // Has Hair pref
                bool hasBeardP = hairstylePref[2].intKey != 12; // Has Beard pref
                bool hasHairD = hairstylePref[1].intKey != 12; // Has Hair dislike
                bool hasBeardD = hairstylePref[3].intKey != 12; // Has Beard dislike
                if (hasHairP || hasBeardP || hasHairD || hasBeardD)
                {
                    drawerCache.Add(($"  ♂ {subtitle}:", string.Empty));
                }
                if (hasHairP)
                {
                    string hairString = string.Join(",\n", StyleLabelBuckets[hairstylePref[0].intKey].Select(s => "  " + s));
                    drawerCache.Add(($"    <color=#ff0000>♥</color> {LabelDict[hairstylePref[0].intKey]}", "RPS_HairList".Translate() + "\n" + hairString));
                }
                if (hasBeardP)
                {
                    string beardString = string.Join(",\n", StyleLabelBuckets[hairstylePref[2].intKey].Select(s => "  " + s));
                    drawerCache.Add(($"    <color=#ff0000>♥</color> {LabelDict[hairstylePref[2].intKey]}", "RPS_BeardList".Translate() + "\n" + beardString));
                }
                if (hasHairD)
                {
                    string hairString = string.Join(",\n", StyleLabelBuckets[hairstylePref[1].intKey].Select(s => "  " + s));
                    drawerCache.Add(($"    <color=#555555>♥</color> {LabelDict[hairstylePref[1].intKey]}", "RPS_HairList".Translate() + "\n" + hairString));
                }
                if (hasBeardD)
                {
                    string beardString = string.Join(",\n", StyleLabelBuckets[hairstylePref[3].intKey].Select(s => "  " + s));
                    drawerCache.Add(($"    <color=#555555>♥</color> {LabelDict[hairstylePref[3].intKey]}", "RPS_BeardList".Translate() + "\n" + beardString));
                }
            }
            if (compPsyche.Sexuality.CanFeelAttractionToGender(Gender.Female))
            {
                bool hasHairP = hairstylePref[4].intKey != 12; // Has Hair pref
                bool hasBeardP = hairstylePref[6].intKey != 12; // Has Beard pref
                bool hasHairD = hairstylePref[5].intKey != 12; // Has Hair dislike
                bool hasBeardD = hairstylePref[7].intKey != 12; // Has Beard dislike
                if (hasHairP || hasBeardP || hasHairD || hasBeardD)
                {
                    drawerCache.Add(($"  ♀ {subtitle}:", string.Empty));
                }
                if (hasHairP)
                {
                    string hairString = string.Join(",\n", StyleLabelBuckets[hairstylePref[4].intKey].Select(s => "  " + s));
                    drawerCache.Add(($"    <color=#ff0000>♥</color> {LabelDict[hairstylePref[4].intKey]}", "RPS_HairList".Translate() + "\n" + hairString));
                }
                if (hasBeardP)
                {
                    string beardString = string.Join(",\n", StyleLabelBuckets[hairstylePref[6].intKey].Select(s => "  " + s));
                    drawerCache.Add(($"    <color=#ff0000>♥</color> {LabelDict[hairstylePref[6].intKey]}", "RPS_BeardList".Translate() + "\n" + beardString));
                }
                if (hasHairD || hasBeardD)
                {
                }
                if (hasHairD)
                {
                    string hairString = string.Join(",\n", StyleLabelBuckets[hairstylePref[5].intKey].Select(s => "  " + s));
                    drawerCache.Add(($"    <color=#555555>♥</color> {LabelDict[hairstylePref[5].intKey]}", "RPS_HairList".Translate() + "\n" + hairString));
                }
                if (hasBeardD)
                {
                    string beardString = string.Join(",\n", StyleLabelBuckets[hairstylePref[7].intKey].Select(s => "  " + s));
                    drawerCache.Add(($"    <color=#555555>♥</color> {LabelDict[hairstylePref[7].intKey]}", "RPS_BeardList".Translate() + "\n" + beardString));
                }
            }
            return drawerCache;
        }
        public override void DrawViewer(Rect rect, Pawn pawn)
        {
            var rectWidth = rect.width;
            var y = rect.y;
            Rect titleRect = new Rect(rect.x, rect.y, rectWidth, rowHeight);
            Widgets.Label(titleRect, "RPS_HairPreferenceReport".Translate());
            y += rowHeight;
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true)
            {
                Rect NoRect = new Rect(titleRect.x, y, rectWidth, rowHeight);
                Widgets.Label(NoRect, "  " + "RPS_NoPreference".Translate());
                return;
            }
            var cachedData = GetDrawerCache(pawn);
            if (cachedData == null || cachedData.Count == 0)
            {
                Rect NoRect = new Rect(titleRect.x, y, rectWidth, rowHeight);
                Widgets.Label(NoRect, "  " + "RPS_NoPreference".Translate());
                return;
            }
            for (int i = 0; i < cachedData.Count; i++)
            {
                Rect ContentRect = new Rect(titleRect.x, y, rectWidth, rowHeight);
                Widgets.Label(ContentRect, cachedData[i].Item1);
                if (cachedData[i].Item2 != string.Empty)
                {
                    if (Mouse.IsOver(ContentRect))
                    {
                        Widgets.DrawHighlight(ContentRect);
                        TooltipHandler.TipRegion(ContentRect, cachedData[i].Item2);
                    }
                }
                y += rowHeight;
            }
        }

        //Static values
        public static readonly float innerPadding = 5f;
        public static readonly float titleHeight = 35f;
        public static readonly float titleContentSpacing = 5f;
        private static readonly Dictionary<int, string> EditKeys = new Dictionary<int, string>
        {
            { 0, "RPS_MaleHairP".Translate() },
            { 1, "RPS_MaleHairD".Translate() },
            { 2, "RPS_MaleBeardP".Translate() },
            { 3, "RPS_MaleBeardD".Translate() },
            { 4, "RPS_FemaleHairP".Translate() },
            { 5, "RPS_FemaleHairD".Translate() },
            { 6, "RPS_FemaleBeardP".Translate() },
            { 7, "RPS_FemaleBeardD".Translate() }
        };
        bool IsStyleAllowedForIndex(int index, int styleIndex)
        {
            if (styleIndex == 12)
                return true;

            bool isHairSlot = index % 4 < 2;
            return isHairSlot ? styleIndex <= 6 : styleIndex >= 7;
        }
        public override void DrawEditor(Rect rect, Pawn pawn, bool EditEnabled)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            //Rect innerRect = rect.ContractedBy(innerPadding);
            Text.Anchor = TextAnchor.MiddleCenter;
            string titleString = "RPS_Hairstyle".Translate();
            Vector2 titleTextSize = Text.CalcSize(titleString);
            Rect titleRect = new Rect(rect.x, rect.y, rect.width, titleTextSize.y);
            Widgets.Label(titleRect, titleString);
            Text.Anchor = TextAnchor.UpperLeft;

            Rect viewRect = new Rect(rect.x, titleRect.yMax + titleContentSpacing, rect.width, rect.height - (titleRect.height + titleContentSpacing));
            float y = viewRect.y;

            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true) return;
            var stylePreference = compPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_HairStylePreference);
            if (stylePreference == null) return;

            float rowWidth = viewRect.width;
            for (int i = 0; i < stylePreference.Count; i++)
            {
                Rect rowRect = new Rect(rect.x, y, rowWidth, rowHeight);
                Widgets.Label(rowRect, EditKeys[i] + ": " + LabelDict[stylePreference[i].intKey]);
                if (EditEnabled && Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        int capturedIndex = i;
                        foreach (var kv in StyleBucketIndex)
                        {
                            string styleKey = kv.Key;
                            int styleIndex = kv.Value;
                            if (!IsStyleAllowedForIndex(capturedIndex, styleIndex))
                                continue;
                            Action action = delegate
                            {
                                int otherIndex = capturedIndex ^ 1;
                                if (stylePreference[otherIndex].intKey == styleIndex)
                                {
                                    stylePreference[capturedIndex].stringKey = "NoPref";
                                    stylePreference[capturedIndex].intKey = 12;

                                    stylePreference[otherIndex].stringKey = "NoPref";
                                    stylePreference[otherIndex].intKey = 12;
                                }
                                else
                                {
                                    stylePreference[capturedIndex].stringKey = styleKey;
                                    stylePreference[capturedIndex].intKey = styleIndex;
                                }
                            };
                            options.Add(new FloatMenuOption(LabelDict[styleIndex], action));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                        Event.current.Use();
                    }
                }
                y += rowHeight;
                if (i == 3)
                    y += titleContentSpacing;
            }
        }
        public override void PostLoadAdjustment(Dictionary<string, List<PrefEntry>> _preference)
        {
            return;
        }
        public override void ClearViewerCache()
        {
            drawerCache = null;
        }
        public override void ClearEditorCache()
        {
            return;
        }
    }
}
