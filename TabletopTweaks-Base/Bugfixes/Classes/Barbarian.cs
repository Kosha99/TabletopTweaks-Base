﻿using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using System.Linq;
using TabletopTweaks.Core.MechanicsChanges;
using TabletopTweaks.Core.NewComponents.Prerequisites;
using TabletopTweaks.Core.Utilities;
using static TabletopTweaks.Base.Main;

namespace TabletopTweaks.Base.Bugfixes.Classes {
    class Barbarian {
        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        static class Barbarian_AlternateCapstone_Patch {
            static bool Initialized;
            [HarmonyPriority(Priority.Last)]
            static void Postfix() {
                if (Initialized) return;
                Initialized = true;
                PatchAlternateCapstone();
            }
            static void PatchAlternateCapstone() {
                if (Main.TTTContext.Fixes.AlternateCapstones.IsDisabled("Barbarian")) { return; }

                var MightyRage = BlueprintTools.GetBlueprintReference<BlueprintFeatureBaseReference>("06a7e5b60020ad947aed107d82d1f897");
                var BarbarianAlternateCapstone = NewContent.AlternateCapstones.Barbarian.BarbarianAlternateCapstone.ToReference<BlueprintFeatureBaseReference>();

                MightyRage.Get().TemporaryContext(bp => {
                    bp.AddComponent<PrerequisiteInPlayerParty>(c => {
                        c.CheckInProgression = true;
                        c.HideInUI = true;
                        c.Not = true;
                    });
                    bp.HideNotAvailibleInUI = true;
                    TTTContext.Logger.LogPatch(bp);
                });
                ClassTools.Classes.BarbarianClass.TemporaryContext(bp => {
                    bp.Progression.UIGroups
                        .Where(group => group.m_Features.Any(f => f.deserializedGuid == MightyRage.deserializedGuid))
                        .ForEach(group => group.m_Features.Add(BarbarianAlternateCapstone));
                    bp.Progression.LevelEntries
                        .Where(entry => entry.Level == 20)
                        .ForEach(entry => entry.m_Features.Add(BarbarianAlternateCapstone));
                    bp.Archetypes.ForEach(a => {
                        a.RemoveFeatures
                            .Where(remove => remove.Level == 20)
                            .Where(remove => remove.m_Features.Any(f => f.deserializedGuid == MightyRage.deserializedGuid))
                            .ForEach(remove => remove.m_Features.Add(BarbarianAlternateCapstone));
                    });
                    TTTContext.Logger.LogPatch("Enabled Alternate Capstones", bp);
                });
            }
        }
        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        static class BlueprintsCache_Init_Patch {
            static bool Initialized;

            static void Postfix() {
                if (Initialized) return;
                Initialized = true;
                TTTContext.Logger.LogHeader("Patching Barbarian");

                PatchBase();
                PatchInstinctualWarrior();
                PatchWreckingBlows();
                PatchCripplingBlows();
            }

            static void PatchBase() {
            }

            static void PatchInstinctualWarrior() {
                PatchCunningElusion();

                void PatchCunningElusion() {
                    if (TTTContext.Fixes.BaseFixes.IsDisabled("FixMonkAcBonusNames")) { return; }

                    var CunningElusionUnlockFeature = BlueprintTools.GetBlueprint<BlueprintFeature>("91c8b2e3abdb4e2e807fddb668b619f8");
                    var CunningElusionFeature = BlueprintTools.GetBlueprint<BlueprintFeature>("a71103ce28964f39b38442baa32a3031");
                    var InstinctualWarriorACBonusBuff = BlueprintTools.GetModBlueprint<BlueprintBuff>(TTTContext, "InstinctualWarriorACBonusBuff");

                    CunningElusionFeature.TemporaryContext(bp => {
                        bp.GetComponent<AddFacts>()?.TemporaryContext(c => {
                            c.m_Facts = new BlueprintUnitFactReference[] { InstinctualWarriorACBonusBuff.ToReference<BlueprintUnitFactReference>() };
                        });
                    });

                    TTTContext.Logger.LogPatch(CunningElusionFeature);
                }
            }

            static void PatchWreckingBlows() {
                if (TTTContext.Fixes.Barbarian.Base.IsDisabled("WreckingBlows")) { return; }
                var WreckingBlowsFeature = BlueprintTools.GetBlueprint<BlueprintFeature>("5bccc86dd1f187a4a99f092dc054c755");
                var PowerfulStanceEffectBuff = BlueprintTools.GetBlueprint<BlueprintBuff>("aabad91034e5c7943986fe3e83bfc78e");
                WreckingBlowsFeature.GetComponent<BuffExtraEffects>().m_CheckedBuff = PowerfulStanceEffectBuff.ToReference<BlueprintBuffReference>();
                TTTContext.Logger.LogPatch("Patched", WreckingBlowsFeature);
            }

            static void PatchCripplingBlows() {
                if (TTTContext.Fixes.Barbarian.Base.IsDisabled("CripplingBlows")) { return; }
                var CripplingBlowsFeature = BlueprintTools.GetBlueprint<BlueprintFeature>("0eec6efbb7f66e148817c9f51b804f08");
                var PowerfulStanceEffectBuff = BlueprintTools.GetBlueprint<BlueprintBuff>("aabad91034e5c7943986fe3e83bfc78e");
                CripplingBlowsFeature.GetComponent<BuffExtraEffects>().m_CheckedBuff = PowerfulStanceEffectBuff.ToReference<BlueprintBuffReference>();
                TTTContext.Logger.LogPatch("Patched", CripplingBlowsFeature);
            }
        }
    }
}
