using System;
using UnityEngine;
using Verse;

namespace Maux36.RimPsyche.Sexuality
{
    public class RimpsycheSexuality : Mod
    {
        public static RimpsycheSexualitySettings settings;
        public const string CoreRequirement = "1.0.27";
        public static string currentVersion;
        public RimpsycheSexuality(ModContentPack content) : base(content)
        {
            currentVersion = content.ModMetaData.ModVersion;
            settings = GetSettings<RimpsycheSexualitySettings>();
            if (!ModsConfig.IsActive("maux36.rimpsyche"))
            {
                Log.Error("[Rimpsyche Sexuality] Rimpsyche not loaded. The dependency was not met and the game will not run correctly");
            }
            var RimpsycheVersion = new Version(Rimpsyche.currentVersion);
            if (RimpsycheVersion < new Version(CoreRequirement))
                Log.Error($"[Rimpsyche - Sexuality] Sexuality version {currentVersion} requires Rimpsyche Core version {CoreRequirement} or above. Your Core ({RimpsycheVersion}) needs to be updated or you will experience errors. If Steam does not automatically updates your mod, you can try un-subbing and re-subbing to force the update.");
        }
        public override string SettingsCategory()
        {
            return "RimpsycheSexualitySettingCategory".Translate();
        }
        private static Vector2 scrollPosition = new Vector2(0f, 0f);
        private static float totalContentHeight = 400f;
        private const float ScrollBarWidthMargin = 18f;
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect outerRect = inRect.ContractedBy(10f);
            bool scrollBarVisible = totalContentHeight > outerRect.height;
            var scrollViewTotal = new Rect(0f, 0f, outerRect.width - (scrollBarVisible ? ScrollBarWidthMargin : 0), totalContentHeight);
            Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollViewTotal);

            var listing_Standard = new Listing_Standard();
            listing_Standard.Begin(new Rect(0f, 0f, scrollViewTotal.width, 9999f));

            listing_Standard.Gap(12f);
            listing_Standard.Label("RimpsycheKinseyDistributionSettings".Translate());
            Rect sliderArea = listing_Standard.GetRect(200f);
            DrawKinseyDistributionSliders(sliderArea);
            listing_Standard.Gap(12f);
            listing_Standard.CheckboxLabeled("RimpsycheUsePreferenceSystem".Translate(), ref RimpsycheSexualitySettings.usePreferenceSystem, "RimpsycheUsePreferenceSystemTooltip".Translate());
            listing_Standard.Gap(12f);
            listing_Standard.CheckboxLabeled("RimpsycheRomanceAttemptGenderDiff".Translate(), ref RimpsycheSexualitySettings.romanceAttemptGenderDiff, "RimpsycheRomanceAttemptGenderDiffTooltip".Translate());
            listing_Standard.Gap(12f);
            RimpsycheSexualitySettings.minRelAttraction = (float)Math.Round(listing_Standard.SliderLabeled("RimpsycheMinRelAttraction".Translate() + " (" + "Default".Translate() + " 0.7): " + RimpsycheSexualitySettings.minRelAttraction, RimpsycheSexualitySettings.minRelAttraction, 0.00f, 1f, tooltip: "RimpsycheMinRelAttractionTooltip".Translate()), 2);

            listing_Standard.End();
            Widgets.EndScrollView();
        }
        private void DrawKinseyDistributionSliders(Rect rect)
        {
            float total = 0f;
            foreach (var value in RimpsycheSexualitySettings.KinseyDistributionSetting)
            {
                total += value;
            }
            const int numSliders = 7;
            float spacing = 5f;
            float sliderWidth = 50f;
            float sliderHeight = rect.height - 100f; // Leave space for labels
            string pcText;

            for (int i = 0; i < numSliders; i++)
            {
                float x = rect.x + i * (sliderWidth + spacing);
                Widgets.Label(new Rect(x, rect.y, sliderWidth, 20f), i.ToString());
                Rect sliderRect = new Rect(x, rect.y + 20f, sliderWidth, sliderHeight);
                RimpsycheSexualitySettings.KinseyDistributionSetting[i] = (int)(GUI.VerticalSlider(
                    sliderRect,
                    RimpsycheSexualitySettings.KinseyDistributionSetting[i],
                    100f,
                    0f
                ));

                Rect valLabelRect = new Rect(x, sliderRect.yMax, sliderWidth, 20f);
                Widgets.Label(valLabelRect, RimpsycheSexualitySettings.KinseyDistributionSetting[i].ToString());
                Rect pcLabelRect = new Rect(x, valLabelRect.yMax, sliderWidth, 20f);
                if (total == 0f) pcText = "(" + (1f / 7f).ToStringPercent() + ")";
                else pcText = "(" + (RimpsycheSexualitySettings.KinseyDistributionSetting[i] / total).ToStringPercent() + ")";
                Widgets.Label(pcLabelRect, pcText);
            }
            Rect buttonRect = new Rect(rect.x, rect.yMax - 30f, 300f, 30f);
            if (Widgets.ButtonText(buttonRect, "RimpsycheKinseyDefaultSetting".Translate()))
            {
                //Slightly adjusted from https://yougov.co.uk/society/articles/12999-half-young-not-heterosexual
                for (int i = 0; i < numSliders; i++)
                {
                    RimpsycheSexualitySettings.KinseyDistributionSetting[i] = RimpsycheSexualitySettings.DefaultDistribution[i];
                }
            }
        }
    }
}
