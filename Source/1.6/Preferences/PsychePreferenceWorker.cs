using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class PsychePreferenceWorker : PreferenceWorker
    {
        private static readonly List<PersonalityDef> SourceDefs = DefDatabase<PersonalityDef>.AllDefsListForReading;
        private static readonly int nodeCount = SourceDefs.Count;
        private static readonly int[] IndexPool = Enumerable.Range(0, nodeCount).ToArray();
        private const int fullCount = 5;

        public PsychePreferenceWorker()
        {
            EditorHeight = personalityRowHeight * 11f;
        }

        public override void PostInit(){}

        public static List<PersonalityDef> RandomNodes()
        {
            for (int i = 0; i < fullCount; i++)
            {
                int randIndex = Rand.Range(i, nodeCount);
                (IndexPool[i], IndexPool[randIndex]) = (IndexPool[randIndex], IndexPool[i]);
            }
            List<PersonalityDef> result = new List<PersonalityDef>(fullCount);
            for (int i = 0; i < fullCount; i++)
            {
                result.Add(SourceDefs[IndexPool[i]]);
            }
            return result;
        }
        public override bool TryGenerate(Pawn pawn, out List<PrefEntry> pref)
        {
            pref = new List<PrefEntry>(fullCount);
            var relevantNodes = RandomNodes();
            //Log.Message($"generating preference for {pawn.Name}");
            for (int i = 0; i < relevantNodes.Count; i++)
            {
                var def = relevantNodes[i];
                float importance = Rand.Range(0f, 1f);
                float target = SexualityHelper.GetSkewedPreference(def.preferenceBias);
                //Log.Message($"generated {def.label} with scew: {def.preferenceBias} -> {target}");
                pref.Add(new PrefEntry(def.defName, def.shortHash, target, importance));
            }
            return true;
        }

        private static readonly float minF = -0.5f;
        private static readonly float posRangeInv = 3f; //0.3333
        public override float Evaluate(Pawn observer, Pawn target, float result, bool isRomantic)
        {
            if (RimpsycheSexualitySettings.usePreferenceSystem != true) return result;
            if (isRomantic != true) return result;
            var observerPsyche = observer.compPsyche();
            if (observerPsyche?.Enabled != true) return result;
            var targetPsyche = target.compPsyche();
            if (targetPsyche?.Enabled != true) return result;
            var psychePreference = observerPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_PsychePreference);
            if (psychePreference == null)
                return result;
            float value = 0f;
            var targetP = targetPsyche.Personality;
            for (int i = 0; i < psychePreference.Count; i++)
            {
                var pf = psychePreference[i];
                PersonalityDef personality = DefDatabase<PersonalityDef>.GetNamed(pf.stringKey, false);
                if (personality == null)
                {
                    Log.Warning($"Psyche Preference unable to load Personality def {pf.stringKey}");
                    //Logic to fix it.
                }
                var pv = targetP.GetPersonality(personality);
                value += Mathf.Max(1-Mathf.Abs(pv - pf.target) * posRangeInv, minF) * pf.importance; //-0.5~1 * 5 => -2.5~5
            }
            float sway = observerPsyche.Evaluate(SexualityFormula.PsychePrefAuthSway);// 0.04~0.16 (0.2*0.2~0.8*0.2)
            float preferenceFactor =  1f + value * sway; // 0.9~1.2 (low sway) || 0.6~1.8 (high sway)
            return result * preferenceFactor;
        }
        
        //Static values
        public static readonly float innerPadding = 5f;
        public static readonly float titleHeight = 35f;
        public static readonly float titleContentSpacing = 5f;
        private static float personalityLabelWidth => RimpsycheDatabase.maxPersonalityLabelWidth;
        //private static float personalityWidthDiff => 2f * (personalityLabelWidth - 130f);
        private static readonly float personalityRowHeight = 22f;
        private static readonly float personalityLabelPadding = 2f;
        private static readonly float personalityBarHeight = 4f;
        private static readonly Color barBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        private static readonly float verticalWidth = 20f;
        private static readonly float verticalPadding = 5f;

        public override float GetViewerHeight(Pawn pawn)
        {
            var cachedData = GetDrawerCache(pawn);
            if (cachedData == null) return personalityRowHeight * 2f;
            float result = personalityRowHeight * (cachedData.Count + 1f);
            return result;
        }

        private static List<string> drawerCache = null;
        private static List<string> GetDrawerCache(Pawn pawn)
        {
            if (drawerCache != null) return drawerCache;
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true)
                return null;
            var psychePreference = compPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_PsychePreference);
            if (psychePreference == null)
                return null;
            drawerCache = new();
            psychePreference = [.. psychePreference.Where(p => p.importance > 0).OrderByDescending(p => p.importance)];
            for (int i = 0; i < psychePreference.Count; i++)
            {
                var pref = psychePreference[i];
                if (string.IsNullOrEmpty(pref.stringKey)) continue;
                var def = DefDatabase<PersonalityDef>.GetNamed(psychePreference[i].stringKey);
                string colorHex = ColorUtility.ToHtmlStringRGB(Color.Lerp(Color.yellow, Color.green, pref.importance));
                string heart = $"<color=#{colorHex}>♥</color>";
                drawerCache.Add($"  {heart} {Rimpsyche_Utility.GetPersonalityDesc(def, pref.target)}");
            }
            return drawerCache;
        }
        public override void DrawViewer(Rect rect, Pawn pawn)
        {
            var rectWidth = rect.width;
            var y = rect.y;
            Rect titleRect = new Rect(rect.x, rect.y, rectWidth, personalityRowHeight);
            Widgets.Label(titleRect, "RPS_AttractionReport".Translate());
            y += personalityRowHeight;
            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true)
            {
                Rect NoRect = new Rect(titleRect.x, y, rectWidth, personalityRowHeight);
                Widgets.Label(NoRect, "  " + "RPS_NoPreference".Translate());
                return;
            }
            var cachedData = GetDrawerCache(pawn);
            if (cachedData == null || cachedData.Count == 0)
            {
                Rect NoRect = new Rect(titleRect.x, y, rectWidth, personalityRowHeight);
                Widgets.Label(NoRect, "  " + "RPS_NoPreference".Translate());
                return;
            }
            for (int i = 0; i < cachedData.Count; i++)
            {
                Rect ContentRect = new Rect(titleRect.x, y, rectWidth, personalityRowHeight);
                Widgets.Label(ContentRect, cachedData[i]);
                y += personalityRowHeight;
            }
        }

        //Internal Cache
        private static List<PersonalityDef> relevantDefsCache = null;
        private static HashSet<int> relevantDefsHash = null;
        private static List<PersonalityDef> GetRelevantDefs(List<PrefEntry> psychePrefs)
        {
            if(relevantDefsCache != null) return relevantDefsCache;
            relevantDefsCache = [];
            relevantDefsHash = new();
            for(int i = 0; i < psychePrefs.Count; i++)
            {
                var pf = psychePrefs[i];
                PersonalityDef personality = DefDatabase<PersonalityDef>.GetNamed(pf.stringKey, false);
                relevantDefsCache.Add(personality);
                relevantDefsHash.Add(personality.shortHash);
            }
            return relevantDefsCache;
        }
        private static List<PersonalityDef> potentialDefsCache = null;
        private static List<PersonalityDef> GetPotentialDefs(List<PrefEntry> psychePrefs)
        {
            if(potentialDefsCache != null) return potentialDefsCache;

            var relevantDefs = GetRelevantDefs(psychePrefs);
            potentialDefsCache = [];
            foreach(var personalityDef in  DefDatabase<PersonalityDef>.AllDefs)
            {
                if (relevantDefsHash.Contains(personalityDef.shortHash)) continue;
                potentialDefsCache.Add(personalityDef);
            }
            return potentialDefsCache;
        }

        public override void DrawEditor(Rect rect, Pawn pawn, bool EditEnabled)
        {
            TextAnchor oldAnchor = Text.Anchor;
            GameFont oldFont = Text.Font;
            //Rect innerRect = rect.ContractedBy(innerPadding);
            Text.Anchor = TextAnchor.MiddleCenter;
            string titleString = "RPC_Personality".Translate();
            Vector2 titleTextSize = Text.CalcSize(titleString);
            Rect titleRect = new Rect(rect.x, rect.y, rect.width, titleTextSize.y);
            Widgets.Label(titleRect, titleString);
            Text.Anchor = TextAnchor.UpperLeft;

            Rect viewRect = new Rect(rect.x, titleRect.yMax + titleContentSpacing, rect.width, rect.height - (titleRect.height + titleContentSpacing));
            float y = viewRect.y;

            var compPsyche = pawn.compPsyche();
            if (compPsyche?.Enabled != true) return;
            var psychePreference = compPsyche.Sexuality.GetPreference(DefOfRimpsycheSexuality.Rimpsyche_PsychePreference);
            if (psychePreference == null) return;
            var personalityDefList = GetRelevantDefs(psychePreference);
            Text.Font = GameFont.Tiny;

            float rowWidth = viewRect.width - verticalWidth;
            float leftRectX = rect.x + personalityLabelPadding;
            float rightRectX = rect.x + rowWidth - personalityLabelPadding - personalityLabelWidth;
            float vertRectX = rect.x + rowWidth;
            float vertBarX = vertRectX + (verticalWidth - personalityBarHeight) / 2f;
            float vertBarHeight = (personalityRowHeight - verticalPadding) * 2f;

            for (int i = 0; i < psychePreference.Count; i++)
            {
                var targetValue = psychePreference[i].target;
                var importanceValue = psychePreference[i].importance;
                var def = personalityDefList[i];
                var (leftLabel, rightLabel, leftColor, rightColor) = (def.low.CapitalizeFirst(), def.high.CapitalizeFirst(), Color.red, Color.green);

                Rect rowRect = new Rect(rect.x, y, rowWidth, personalityRowHeight * 2f);
                Rect vertRect = new Rect(vertRectX, y, verticalWidth, personalityRowHeight * 2f);
                if (!EditEnabled && Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                }
                string tooltipString = $"{def.label.CapitalizeFirst()}: {(targetValue * 100f).ToString("F1")}";
                TooltipHandler.TipRegion(rowRect, tooltipString);

                if (Mouse.IsOver(vertRect))
                {
                    Widgets.DrawHighlight(vertRect);
                    string importanceTooltipString = "RPS_Importance".Translate() + $": {(importanceValue * 100f).ToString("F1")}";
                    TooltipHandler.TipRegion(vertRect, importanceTooltipString);
                }
                float uppercenterY = rowRect.y + rowRect.height * 0.25f;
                float lowercenterY = rowRect.y + rowRect.height * 0.75f;
                // Left label
                Rect leftRect = new Rect(leftRectX, rowRect.y, personalityLabelWidth, personalityRowHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(leftRect, leftLabel);

                // Right label
                Rect rightRect = new Rect(rightRectX, rowRect.y, personalityLabelWidth, personalityRowHeight);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(rightRect, rightLabel);

                // Label
                Rect NodeRect = new Rect(rect.x, rowRect.y, rowWidth, personalityRowHeight);

                if (EditEnabled && Mouse.IsOver(NodeRect))
                {
                    Widgets.DrawHighlight(NodeRect);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        int capturedIndex = i;
                        foreach (PersonalityDef potentialDef in GetPotentialDefs(psychePreference))
                        {
                            PersonalityDef capturedDef = potentialDef;
                            Action action = delegate
                            {
                                psychePreference[capturedIndex].stringKey = capturedDef.defName;
                                relevantDefsCache = null;
                                relevantDefsHash = null;
                                potentialDefsCache = null;
                            };
                            options.Add(new FloatMenuOption(capturedDef.label.CapitalizeFirst(), action));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                        Event.current.Use();
                    }
                }
                float barPartHeight = NodeRect.yMax + (personalityRowHeight - personalityBarHeight) * 0.5f;

                if (EditEnabled)
                {
                    Rect sliderRect = new Rect(0, barPartHeight, rowRect.width, personalityRowHeight);
                    float newValue = Widgets.HorizontalSlider(sliderRect, targetValue, -1f, 1f);
                    if (newValue != targetValue)
                    {
                        psychePreference[i].target = newValue;
                    }
                    //Importance
                    psychePreference[i].importance = GUI.VerticalSlider(vertRect, psychePreference[i].importance, 1f, 0f);
                }
                else
                {
                    // Bar background
                    Rect barRect = new Rect(0, barPartHeight, rowRect.width, personalityBarHeight);
                    Widgets.DrawBoxSolid(barRect, barBackgroundColor);

                    // Value bar
                    float clamped = Mathf.Clamp(targetValue, -1f, 1f);
                    float halfBar = Mathf.Abs(clamped) * (rowRect.width) / 2f;
                    Rect valueRect = clamped >= 0
                        ? new Rect(rowRect.width * 0.5f, barRect.y, halfBar, personalityBarHeight)
                        : new Rect(rowRect.width * 0.5f - halfBar, barRect.y, halfBar, personalityBarHeight);

                    // Color based on intensity (small = yellow, strong = green)
                    float intensity = Mathf.Abs(clamped) * 2f;
                    Color barColor = Color.Lerp(Color.yellow, Color.green, intensity);
                    Widgets.DrawBoxSolid(valueRect, barColor);

                    //Importance
                    Rect vertBarRect = new Rect(vertBarX, vertRect.y + verticalPadding, personalityBarHeight, vertBarHeight);
                    Widgets.DrawBoxSolid(vertBarRect, barBackgroundColor);
                    Rect vertValueRect = new Rect(vertBarX, vertBarRect.y + vertBarHeight * (1- psychePreference[i].importance), personalityBarHeight, vertBarHeight * psychePreference[i].importance);
                    Color vertBarColor = Color.Lerp(Color.yellow, Color.green, psychePreference[i].importance);
                    Widgets.DrawBoxSolid(vertValueRect, vertBarColor);
                }

                y += 2f * personalityRowHeight;
            }
            Text.Anchor = oldAnchor;
            Text.Font = oldFont;
            return;
        }
        public override void PostLoadAdjustment(Dictionary<string, List<PrefEntry>> _preference)
        {
            if (_preference.TryGetValue("Rimpsyche_PsychePreference", out var psychePreference))
            {
                for (int i = 0; i < psychePreference.Count; i++)
                {
                    PersonalityDef p = DefDatabase<PersonalityDef>.GetNamed(psychePreference[i].stringKey, false);
                    if (p == null)
                    {
                        Log.Warning($"Psyche Preference unable to load Personality def {psychePreference[i].stringKey}");
                        //Logic to fix it.
                    }
                    else psychePreference[i].intKey = p.shortHash;
                }
            }
        }
        public override void ClearViewerCache()
        {
            drawerCache = null;
        }
        public override void ClearEditorCache()
        {
            relevantDefsCache = null;
            relevantDefsHash = null;
            potentialDefsCache = null;
        }
    }
}
