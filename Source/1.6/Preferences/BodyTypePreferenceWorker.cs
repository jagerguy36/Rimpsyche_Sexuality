using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class BodyTypePreferenceDef : PreferenceDef
    {
        public enum BodyTypePrefEnum : int
        {
            None,
            Thin,
            Standard,
            Hulk,
            Fat
        }

        public class BodyPrefBias
        {
            public BodyTypePrefEnum bodytype;
            public float weight;
        }

        public List<BodyPrefBias> maleBodyPrefBias;
        public List<BodyPrefBias> maleBodyDislikeBias;

        public List<BodyPrefBias> femaleBodyPrefBias;
        public List<BodyPrefBias> femaleBodyDislikeBias;
    }

    public class BodyTypePreferenceWorker : PreferenceWorker
    {
        private static BodyTypePreferenceDef bodyTypePreferenceDef;
        public static Dictionary<int, BodyTypeDef> mBodyTypeDict = new Dictionary<int, BodyTypeDef>
        {
            { 0, null },
            { 1, BodyTypeDefOf.Thin },
            { 2, BodyTypeDefOf.Male },
            { 3, BodyTypeDefOf.Hulk },
            { 4, BodyTypeDefOf.Fat },
        };
        public static Dictionary<int, BodyTypeDef> fBodyTypeDict = new Dictionary<int, BodyTypeDef>
        {
            { 0, null },
            { 1, BodyTypeDefOf.Thin },
            { 2, BodyTypeDefOf.Female },
            { 3, BodyTypeDefOf.Hulk },
            { 4, BodyTypeDefOf.Fat },
        };
        public float mPrefTotal;
        public float mDislikeTotal;
        public float fPrefTotal;
        public float fDislikeTotal;

        public BodyTypePreferenceWorker()
        {
            EditorHeight = rowHeight * 5f + titleContentSpacing * 2f;
        }

        public override void PostInit()
        {
            bodyTypePreferenceDef = def as BodyTypePreferenceDef;
            mPrefTotal = CalculateTotal(bodyTypePreferenceDef.maleBodyPrefBias);
            mDislikeTotal = CalculateTotal(bodyTypePreferenceDef.maleBodyDislikeBias);
            fPrefTotal = CalculateTotal(bodyTypePreferenceDef.femaleBodyPrefBias);
            fDislikeTotal = CalculateTotal(bodyTypePreferenceDef.femaleBodyDislikeBias);
        }
        private static float CalculateTotal(List<BodyTypePreferenceDef.BodyPrefBias> list)
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
            pref = new List<PrefEntry>(4);//mPref, mDis, fPref, mDis
            //Log.Message($"generating bodytype preference for {pawn.Name}");
            var mPref = GetWeightedPref(bodyTypePreferenceDef.maleBodyPrefBias, mPrefTotal);
            var mDis = GetWeightedPref(bodyTypePreferenceDef.maleBodyDislikeBias, mDislikeTotal);
            var fPref = GetWeightedPref(bodyTypePreferenceDef.femaleBodyPrefBias, fPrefTotal);
            var fDis = GetWeightedPref(bodyTypePreferenceDef.femaleBodyDislikeBias, fDislikeTotal);
            // If preference and dislike conflict, reset both to None
            if (mPref == mDis)
            {
                mPref = BodyTypePreferenceDef.BodyTypePrefEnum.None;
                mDis = BodyTypePreferenceDef.BodyTypePrefEnum.None;
            }
            if (fPref == fDis)
            {
                fPref = BodyTypePreferenceDef.BodyTypePrefEnum.None;
                fDis = BodyTypePreferenceDef.BodyTypePrefEnum.None;
            }
            //string int float float
            pref.Add(new PrefEntry(mPref.ToString(), (int)mPref, 0f, 0f));
            pref.Add(new PrefEntry(mDis.ToString(), (int)mDis, 0f, 0f));
            pref.Add(new PrefEntry(fPref.ToString(), (int)fPref, 0f, 0f));
            pref.Add(new PrefEntry(fDis.ToString(), (int)fDis, 0f, 0f));
            return true;
        }
        private static BodyTypePreferenceDef.BodyTypePrefEnum GetWeightedPref(List<BodyTypePreferenceDef.BodyPrefBias> list, float totalWeight)
        {
            float roll = Rand.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < list.Count; i++)
            {
                cumulative += list[i].weight;
                if (roll < cumulative)
                {
                    return list[i].bodytype;
                }
            }
            return list[list.Count - 1].bodytype;
        }
        public override float Evaluate(Pawn observer, Pawn target, float result, bool isRomantic)
        {
            if (RimpsycheSexualitySettings.usePreferenceSystem != true) return result;
            if (isRomantic) return result;
            var observerPsyche = observer.compPsyche();
            if (observerPsyche?.Enabled != true) return 0f;
            var targetPsyche = target.compPsyche();
            if (targetPsyche?.Enabled != true) return 0f;
            var bodyPreference = observerPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_BodyTypePreference);
            if (bodyPreference == null)
                return 0f;
            float value = 0f;
            if (target.gender == Gender.Male)
            {
                //0:male pref | 1: male dislike
                if (mBodyTypeDict[bodyPreference[0].intKey] == target.story.bodyType)
                    value += 0.5f;
                else if (mBodyTypeDict[bodyPreference[1].intKey] == target.story.bodyType)
                    value -= 0.5f;
            }
            else if (target.gender == Gender.Female)
            {
                //2: female pref | 3: female dislike
                if (fBodyTypeDict[bodyPreference[2].intKey] == target.story.bodyType)
                    value += 0.5f;
                if (fBodyTypeDict[bodyPreference[3].intKey] == target.story.bodyType)
                    value -= 0.5f;
            }
            //value -0.5 ~ 0.5
            float sway = observerPsyche.Evaluate(SexualityFormula.PhysicalPrefAuthSway);// 0.2 ~ 1.8
            return value * sway; //HighAuth(-0.1~0.1) HighSuperficial(~0.9~0.9)
        }

        private static readonly float rowHeight = 22f;
        private static List<(string, string)> bodyDrawerCache = null;

        public override float GetViewerHeight(Pawn pawn)
        {
            var cachedData = GetBodyDrawerCache(pawn);
            if (cachedData == null || cachedData.Count == 0)
                return rowHeight * 2f;

            float result = rowHeight * (cachedData.Count + 1f);
            return result;
        }

        private static Dictionary<int, string> LabelDict = new Dictionary<int, string>
        {
            { 0, "RPS_NoPref".Translate() },
            { 1, "RPS_Thin".Translate() },
            { 2, "RPS_Standard".Translate() },
            { 3, "RPS_Hulk".Translate() },
            { 4, "RPS_Fat".Translate() },
        };
        private static List<(string, string)> GetBodyDrawerCache(Pawn pawn)
        {
            if (bodyDrawerCache != null) return bodyDrawerCache;

            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true)
                return null;

            var bodyPreference = compPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_BodyTypePreference);
            if (bodyPreference == null)
                return null;

            bodyDrawerCache = new();
            TaggedString subtitle = "RPS_Likes".Translate() + "(<color=#ff0000>♥</color>), " + "RPS_Dislikes".Translate() + "(<color=#555555>♥</color>)";

            // Male Body Preferences (Index 0 = Preferred, 1 = Disliked)
            if (compPsyche.Sexuality.CanFeelAttractionToGender(Gender.Male))
            {
                bool hasBodyP = bodyPreference[0].intKey != 0;
                bool hasBodyD = bodyPreference[1].intKey != 0;

                if (hasBodyP || hasBodyD)
                {
                    bodyDrawerCache.Add(($"  ♂ {subtitle}:", string.Empty));
                }
                if (hasBodyP)
                {
                    bodyDrawerCache.Add(($"    <color=#ff0000>♥</color> {LabelDict[bodyPreference[0].intKey]}", string.Empty));
                }
                if (hasBodyD)
                {
                    bodyDrawerCache.Add(($"    <color=#555555>♥</color> {LabelDict[bodyPreference[1].intKey]}", string.Empty));
                }
            }

            // Female Body Preferences (Index 2 = Preferred, 3 = Disliked)
            if (compPsyche.Sexuality.CanFeelAttractionToGender(Gender.Female))
            {
                bool hasBodyP = bodyPreference[2].intKey != 0;
                bool hasBodyD = bodyPreference[3].intKey != 0;

                if (hasBodyP || hasBodyD)
                {
                    bodyDrawerCache.Add(($"  ♀ {subtitle}:", string.Empty));
                }
                if (hasBodyP)
                {
                    bodyDrawerCache.Add(($"    <color=#ff0000>♥</color> {LabelDict[bodyPreference[2].intKey]}", string.Empty));
                }
                if (hasBodyD)
                {
                    bodyDrawerCache.Add(($"    <color=#555555>♥</color> {LabelDict[bodyPreference[3].intKey]}", string.Empty));
                }
            }

            return bodyDrawerCache;
        }

        public override void DrawViewer(Rect rect, Pawn pawn)
        {
            var rectWidth = rect.width;
            var y = rect.y;
            Rect titleRect = new Rect(rect.x, rect.y, rectWidth, rowHeight);
            Widgets.Label(titleRect, "RPS_BodyPreferenceReport".Translate());
            y += rowHeight;

            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true)
            {
                Rect NoRect = new Rect(titleRect.x, y, rectWidth, rowHeight);
                Widgets.Label(NoRect, "  " + "RPS_NoPreference".Translate());
                return;
            }

            var cachedData = GetBodyDrawerCache(pawn);
            if (cachedData == null)
            {
                Rect NoRect = new Rect(titleRect.x, y, rectWidth, rowHeight);
                Widgets.Label(NoRect, "  " + "RPS_NoPreference".Translate());
                return;
            }
            if (cachedData.Count == 0)
            {
                Rect NoRect = new Rect(titleRect.x, y, rectWidth, rowHeight);
                Widgets.Label(NoRect, "  " + "RPS_NoPref".Translate());
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

        // Static values
        public static readonly float innerPadding = 5f;
        public static readonly float titleHeight = 35f;
        public static readonly float titleContentSpacing = 5f;

        private static readonly Dictionary<int, string> BodyEditKeys = new Dictionary<int, string>
        {
            { 0, "RPS_MBodyP".Translate() },
            { 1, "RPS_MBodyD".Translate() },
            { 2, "RPS_FBodyP".Translate() },
            { 3, "RPS_FBodyD".Translate() }
        };

        public override void DrawEditor(Rect rect, Pawn pawn, bool EditEnabled)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;

            Text.Anchor = TextAnchor.MiddleCenter;
            string titleString = "RPS_BodyType".Translate();
            Vector2 titleTextSize = Text.CalcSize(titleString);
            Rect titleRect = new Rect(rect.x, rect.y, rect.width, titleTextSize.y);
            Widgets.Label(titleRect, titleString);
            Text.Anchor = TextAnchor.UpperLeft;

            Rect viewRect = new Rect(rect.x, titleRect.yMax + titleContentSpacing, rect.width, rect.height - (titleRect.height + titleContentSpacing));
            float y = viewRect.y;

            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true) return;
            var bodyPreference = compPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_BodyTypePreference);
            if (bodyPreference == null) return;

            float rowWidth = viewRect.width;
            for (int i = 0; i < bodyPreference.Count; i++)
            {
                Rect rowRect = new Rect(rect.x, y, rowWidth, rowHeight);
                Widgets.Label(rowRect, BodyEditKeys[i] + ": " + LabelDict[bodyPreference[i].intKey]);

                if (EditEnabled && Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        int capturedIndex = i;

                        foreach (var kv in LabelDict)
                        {
                            int bodyIndex = kv.Key;
                            string bodyKey = kv.Value;
                            Action action = delegate
                            {
                                // XOR 1 toggles between 0<->1 (Male Prefer/Dislike) and 2<->3 (Female Prefer/Dislike)
                                int otherIndex = capturedIndex ^ 1;
                                if (bodyPreference[otherIndex].intKey == bodyIndex)
                                {
                                    bodyPreference[capturedIndex].stringKey = "None";
                                    bodyPreference[capturedIndex].intKey = 0;

                                    bodyPreference[otherIndex].stringKey = "None";
                                    bodyPreference[otherIndex].intKey = 0;
                                }
                                else
                                {
                                    bodyPreference[capturedIndex].stringKey = bodyKey;
                                    bodyPreference[capturedIndex].intKey = bodyIndex;
                                }
                            };
                            options.Add(new FloatMenuOption(LabelDict[bodyIndex], action));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                        Event.current.Use();
                    }
                }
                y += rowHeight;

                // Adds extra space after finishing the Male categories (index 1) before showing Female types
                if (i == 1)
                    y += titleContentSpacing;
            }
        }
        public override void PostLoadAdjustment(Dictionary<string, List<PrefEntry>> _preference)
        {
        }
        public override void ClearViewerCache()
        {
            bodyDrawerCache = null;
        }
        public override void ClearEditorCache()
        {
        }
    }
}
