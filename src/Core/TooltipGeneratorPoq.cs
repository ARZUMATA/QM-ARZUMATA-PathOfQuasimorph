using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Records;
using QM_PathOfQuasimorph.PoqHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using static QM_PathOfQuasimorph.Controllers.CreaturesControllerPoq;
using static QM_PathOfQuasimorph.Controllers.CreaturesControllerPoq.CreatureDataPoq;
using static QM_PathOfQuasimorph.Controllers.MagnumPoQProjectsController;
using static UnityEngine.Rendering.DebugUI;

namespace QM_PathOfQuasimorph.Core
{
    internal class TooltipGeneratorPoq
    {
        static TooltipFactory _factory;
        static PropertiesTooltip _tooltip;
        static ItemTooltipBuilder _tooltipBuilder;
        private static Logger _logger = new Logger(null, typeof(TooltipGeneratorPoq));

        private static readonly Dictionary<string, string> DifferenceColorMap = new Dictionary<string, string>
        {
            // #2196F3  // Material Design blue
            // #F44336  // Material Design red
            // #444444  // Gray

            { "positive", "#2196F3" },   // Positive or inverted positive
            { "negative", "#F44336" },    // Negative or inverted negative
            { "equal", "#444444" }
        };

        public static void HandlePoqTooltip()
        {
            InputController instance = SingletonMonoBehaviour<InputController>.Instance;
            var isShiftKeyDown = Input.GetKeyDown(KeyCode.LeftShift);
            var isShiftKeyUp = Input.GetKeyUp(KeyCode.LeftShift);

            // We need to check only for tooltips with extra text.
            if (SingletonMonoBehaviour<TooltipFactory>.Instance.IsTooltipWithAdditHintActive)
            {
                if (isShiftKeyDown)
                {
                    _factory = SingletonMonoBehaviour<TooltipFactory>.Instance;
                    _factory._state.Resolve(_factory._itemTooltipBuilder);
                    _tooltipBuilder = _factory._itemTooltipBuilder;

                    var wrappedItem = MetadataWrapper.SplitItemUid(_factory._lastShowedItem.Id);

                    if (!RecordCollection.MetadataWrapperRecords.TryGetValue(_factory._lastShowedItem.Id, out MetadataWrapper wrapper))
                    {
                        if (MetadataWrapper.IsPoqItemUid(_factory._lastShowedItem.Id))
                        {
                            _logger.Log("HandlePoqTooltip: trying to get poq item but record is missing.");
                            foreach (var rec in RecordCollection.MetadataWrapperRecords)
                            {
                                _logger.Log($"rec {rec.Key}");
                                _logger.Log($"\t\t {rec.Value.ReturnItemUid()}");

                            }

                            _logger.Log("HandlePoqTooltip: trying to get poq item but record is missing.");

                            throw new Exception($"HandlePoqTooltip: trying to get poq item but record is missing.");
                        }
                    }
                    else
                    {
                        _logger.Log($"wrappedItem.CustomId {wrappedItem.ReturnItemUid()}");

                        if (wrappedItem.PoqItem)
                        {
                            _tooltip = _factory.BuildEmptyTooltip();
                            //_tooltip.SetCaption1(Localization.Get("item." + wrappedItem.ReturnItemUid() + ".name"), _factory.FirstLetterColor);

                            PropertiesTooltipHelper.SetCaption1(_tooltip, Localization.Get("item." + wrappedItem.ReturnItemUid() + ".name"), _factory.FirstLetterColor, RaritySystem.Colors[wrappedItem.RarityClass]);

                            PropertiesTooltipHelper.SetCaption2(_tooltip, Localization.Get("item." + wrappedItem.ReturnItemUid() + ".shortdesc"), RaritySystem.Colors[wrappedItem.RarityClass]);

                            //_tooltip.SetCaption2(Localization.Get("item." + wrappedItem.ReturnItemUid() + ".shortdesc"));

                            //_tooltip.SetCaption1Right(wrappedItem.RarityClass.ToString().WrapInColor(RaritySystem.Colors[wrappedItem.RarityClass].Replace("#", string.Empty)));
                            _factory.AddPanelToTooltip().SetValue(wrappedItem.RarityClass.ToString().WrapInColor(RaritySystem.Colors[wrappedItem.RarityClass].Replace("#", string.Empty)));
                            //_factory.AddPanelToTooltip().SetValue("Difference");

                            //_factory._tooltip.MakeRed();
                            _factory.AddCompareBlock(_factory._lastShowedItem);
                            //_factory.AddCompareBlock(_factory._lastShowedItem);

                            InitItemComparsion(_factory._lastShowedItem as PickupItem, wrappedItem.Id);

                            _factory._tooltip.IsAdditionalTooltip = true;
                        }
                    }
                }

                if (isShiftKeyUp)
                {
                    SingletonMonoBehaviour<TooltipFactory>.Instance.RestoreItemTooltip();
                }
            }
        }

        static string GetDifferenceColor(float difference, bool invert = false)
        {
            if (difference == 0)
            {
                return DifferenceColorMap["equal"];
            }

            bool isPositive = difference >= 0;

            if (invert)
            {
                isPositive = !isPositive;
            }

            string result = isPositive ? "positive" : "negative";
            return DifferenceColorMap[result];
        }

        static string GetDifferenceSign(float difference, bool invert = false)
        {
            if (difference == 0)
            {
                return "=";
            }

            bool isPositive = difference >= 0;
            if (invert)
            {
                isPositive = !isPositive;
            }


            return isPositive ? "+" : "-";
        }

        static string FormatDifference(float difference, bool invertColor = false, bool invertSign = false, bool addsign = true)
        {
            string sign = GetDifferenceSign(difference, invertSign);
            string color = GetDifferenceColor(difference, invertColor);
            if (sign == "=")
            {
                return $"<color={color}>{difference.ToString()}</color>";


            }
            else
            {
                return $"<color={color}>{(addsign ? sign : string.Empty)}{difference.ToString()}</color>";
            }
        }

        static string FormatDifference(string label, float difference, bool invertColor = false, bool invertSign = false, bool addsign = true)
        {
            string sign = GetDifferenceSign(difference, invertSign);
            string color = GetDifferenceColor(difference, invertColor);
            if (sign == "=")
            {
                return $"<color={color}>{label}</color>";


            }
            else
            {
                return $"<color={color}>{(addsign ? sign : string.Empty)}{label}</color>";
            }
        }

        private static void InitItemComparsion(PickupItem item, string genericId)
        {
            if (_factory._lastShowedItem.Is<BreakableItemRecord>())
            {
                InitBreakable(item.Record<BreakableItemRecord>(), genericId, item);
            }

            if (_factory._lastShowedItem.Is<WeaponRecord>())
            {
                InitWeapon(item.Record<WeaponRecord>(), genericId, item);
                InitTraits(item.Record<WeaponRecord>(), genericId, item);
            }

            if (_factory._lastShowedItem.Is<ResistRecord>())
            {
                InitArmor(item.Record<ResistRecord>(), genericId, item);
            }

            if (_factory._lastShowedItem.Is<AugmentationRecord>())
            {
                InitAugmentation(item.Record<AugmentationRecord>(), genericId, item);
            }

            if (_factory._lastShowedItem.Is<ImplantRecord>())
            {
                InitImplant(item.Record<ImplantRecord>(), genericId, item);
            }

            if (_factory._lastShowedItem.Is<ItemRecord>())
            {
                InitWeight(item.Record<ItemRecord>(), genericId, item);
            }
        }

        private static void InitImplant(ImplantRecord implantRecord, string genericId, PickupItem item)
        {
            var genericRecord = Data.Items.GetSimpleRecord<ImplantRecord>(genericId, true);

            _logger.Log($"genericRecord ImplantRecord is null {genericRecord == null}");

            foreach (var effect in implantRecord.ImplicitBonusEffects)
            {
                var hasGeneric = genericRecord.ImplicitBonusEffects.TryGetValue(effect.Key, out float effectGeneric);
                var effectDifference = (float)Math.Round(effect.Value - (hasGeneric ? effectGeneric : 0), 2);

                if (effectDifference != 0)
                {
                    _logger.Log($"bonus effect {effect.Key}");
                    WoundEffectRecord record = Data.WoundEffects.GetRecord(effect.Key, true);
                    _logger.Log($"TooltipIconTag {record.TooltipIconTag}");

                    var value = $"{FormatHelper.FormatValue((float)Math.Round(effect.Value, 2), record.ValueFormat)} ({FormatDifference(FormatHelper.FormatValue((float)Math.Round(effectDifference, 2), record.ValueFormat), effectDifference, addsign: false)})".WrapInColor(Colors.AltGreen);

                    var isResist = record.TooltipIconTag.Contains("resist");
                    var iconName = isResist ? ($"damage_{effect.Key.Replace("resist_", string.Empty)}_resist") : $"{record.TooltipIconTag}_green";

                    _factory.AddPanelToTooltip().SetIcon(iconName).
                    LocalizeName($"woundeffect.{effect.Key}.desc")
                    .SetValue(value, true)
                    .SetTextColor(Colors.Green)
                    .SetComparsionValue((hasGeneric ? FormatHelper.FormatValue(effectGeneric, record.ValueFormat) : "0"));
                }
            }

            foreach (var effect in implantRecord.ImplicitPenaltyEffects)
            {
                var hasGeneric = genericRecord.ImplicitPenaltyEffects.TryGetValue(effect.Key, out float effectGeneric);
                var effectDifference = (float)Math.Round(effect.Value - (hasGeneric ? effectGeneric : 0), 2);

                if (effectDifference != 0)
                {
                    _logger.Log($"penalty effect {effect.Key}");
                    WoundEffectRecord record = Data.WoundEffects.GetRecord(effect.Key, true);
                    _logger.Log($"TooltipIconTag {record.TooltipIconTag}");

                    var value = $"{FormatHelper.FormatValue((float)Math.Round(effect.Value, 2), record.ValueFormat)} ({FormatDifference(FormatHelper.FormatValue((float)Math.Round(effectDifference, 2), record.ValueFormat), effectDifference, addsign: false)})".WrapInColor(Colors.LightRed);

                    var isResist = record.TooltipIconTag.Contains("resist");
                    var iconName = isResist ? ($"damage_{effect.Key.Replace("resist_", string.Empty)}_red") : $"{record.TooltipIconTag}_red";

                    _factory.AddPanelToTooltip().SetIcon(iconName).
                     LocalizeName($"woundeffect.{effect.Key}.desc")
                     .SetValue(value, true)
                     .SetTextColor(Colors.LightRed)
                     .SetComparsionValue((hasGeneric ? FormatHelper.FormatValue(effectGeneric, record.ValueFormat) : "0"));
                }

            }
        }

        private static void InitAugmentation(AugmentationRecord augmentationRecord, string genericId, PickupItem item)
        {
            var genericRecord = Data.Items.GetSimpleRecord<AugmentationRecord>(genericId, true);

            _logger.Log($"genericRecord AugmentationRecord is {genericRecord == null}");

            WoundSlotRecord[] records = (from id in augmentationRecord.WoundSlotIds select Data.WoundSlots.GetRecord(id, true)).ToArray<WoundSlotRecord>();
            WoundSlotRecord[] recordsGeneric = (from id in genericRecord.WoundSlotIds select Data.WoundSlots.GetRecord(id, true)).ToArray<WoundSlotRecord>();

            Dictionary<string, float> bonusEffects = WoundSystem.AggregateWoundEffects<WoundSlotRecord>(records, (WoundSlotRecord r) => r.ImplicitBonusEffects);
            Dictionary<string, float> penaltyEffects = WoundSystem.AggregateWoundEffects<WoundSlotRecord>(records, (WoundSlotRecord r) => r.ImplicitPenaltyEffects);
            Dictionary<string, float> coreEffects = WoundSystem.AggregateWoundEffects<WoundSlotRecord>(records, (WoundSlotRecord r) => r.CoreEffects);

            Dictionary<string, float> bonusEffectsGeneric = WoundSystem.AggregateWoundEffects<WoundSlotRecord>(recordsGeneric, (WoundSlotRecord r) => r.ImplicitBonusEffects);
            Dictionary<string, float> penaltyEffectsGeneric = WoundSystem.AggregateWoundEffects<WoundSlotRecord>(recordsGeneric, (WoundSlotRecord r) => r.ImplicitPenaltyEffects);
            Dictionary<string, float> coreEffectsGeneric = WoundSystem.AggregateWoundEffects<WoundSlotRecord>(records, (WoundSlotRecord r) => r.CoreEffects);

            foreach (var effect in coreEffects)
            {
                var hasGeneric = coreEffectsGeneric.TryGetValue(effect.Key, out float effectGeneric);
                var effectDifference = (float)Math.Round(effect.Value - (hasGeneric ? effectGeneric : 0), 2);

                _logger.Log($"core effect {effect.Key}");
                _logger.Log($"core effectDifference {effectDifference}");

                if (effectDifference != 0)
                {
                    WoundEffectRecord record = Data.WoundEffects.GetRecord(effect.Key, true);
                    _logger.Log($"TooltipIconTag {record.TooltipIconTag}");

                    var value = $"{FormatHelper.FormatValue((float)Math.Round(effect.Value, 2), record.ValueFormat)} ({FormatDifference(FormatHelper.FormatValue((float)Math.Round(effectDifference, 2), record.ValueFormat), effectDifference, addsign: false)})".WrapInColor(Colors.AltGreen);

                    var isResist = record.TooltipIconTag.Contains("resist");
                    var iconName = isResist ? ($"damage_{effect.Key.Replace("resist_", string.Empty)}_resist") : $"{record.TooltipIconTag}_green";

                    _factory.AddPanelToTooltip().SetIcon(iconName).
                    LocalizeName($"woundeffect.{effect.Key}.desc")
                    .SetValue(value, true)
                    .SetTextColor(Colors.Green)
                    .SetComparsionValue((hasGeneric ? FormatHelper.FormatValue(effectGeneric, record.ValueFormat) : "0"));

                }
            }

            foreach (var effect in bonusEffects)
            {
                var hasGeneric = bonusEffectsGeneric.TryGetValue(effect.Key, out float effectGeneric);
                var effectDifference = (float)Math.Round(effect.Value - (hasGeneric ? effectGeneric : 0), 2);

                if (effectDifference != 0)
                {
                    _logger.Log($"bonus effect {effect.Key}");
                    WoundEffectRecord record = Data.WoundEffects.GetRecord(effect.Key, true);
                    _logger.Log($"TooltipIconTag {record.TooltipIconTag}");

                    var value = $"{FormatHelper.FormatValue((float)Math.Round(effect.Value, 2), record.ValueFormat)} ({FormatDifference(FormatHelper.FormatValue((float)Math.Round(effectDifference, 2), record.ValueFormat), effectDifference, addsign: false)})".WrapInColor(Colors.AltGreen);

                    var isResist = record.TooltipIconTag.Contains("resist");
                    var iconName = isResist ? ($"damage_{effect.Key.Replace("resist_", string.Empty)}_resist") : $"{record.TooltipIconTag}_green";

                    _factory.AddPanelToTooltip().SetIcon(iconName).
                    LocalizeName($"woundeffect.{effect.Key}.desc")
                    .SetValue(value, true)
                    .SetTextColor(Colors.Green)
                    .SetComparsionValue((hasGeneric ? FormatHelper.FormatValue(effectGeneric, record.ValueFormat) : "0"));
                }
            }

            foreach (var effect in penaltyEffects)
            {
                var hasGeneric = penaltyEffectsGeneric.TryGetValue(effect.Key, out float effectGeneric);
                var effectDifference = (float)Math.Round(effect.Value - (hasGeneric ? effectGeneric : 0), 2);

                if (effectDifference != 0)
                {
                    _logger.Log($"penalty effect {effect.Key}");
                    WoundEffectRecord record = Data.WoundEffects.GetRecord(effect.Key, true);
                    _logger.Log($"TooltipIconTag {record.TooltipIconTag}");

                    var value = $"{FormatHelper.FormatValue((float)Math.Round(effect.Value, 2), record.ValueFormat)} ({FormatDifference(FormatHelper.FormatValue((float)Math.Round(effectDifference, 2), record.ValueFormat), effectDifference, addsign: false)})".WrapInColor(Colors.LightRed);

                    var isResist = record.TooltipIconTag.Contains("resist");
                    var iconName = isResist ? ($"damage_{effect.Key.Replace("resist_", string.Empty)}_red") : $"{record.TooltipIconTag}_red";

                    _factory.AddPanelToTooltip().SetIcon(iconName).
                     LocalizeName($"woundeffect.{effect.Key}.desc")
                     .SetValue(value, true)
                     .SetTextColor(Colors.LightRed)
                     .SetComparsionValue((hasGeneric ? FormatHelper.FormatValue(effectGeneric, record.ValueFormat) : "0"));
                }
            }
        }

        private static void InitArmor(ResistRecord recordPoq, string genericId, PickupItem item)
        {
            var genericRecord = Data.Items.GetSimpleRecord<ResistRecord>(genericId, true);

            _logger.Log($"genericRecord ResistRecord is {genericRecord == null}");

            // blunt 5 pierce 0 lacer 0 fire 0 cold 5 poison 0 shock 0 beam 0
            for (int i = 0; i < genericRecord.ResistSheet.Count; i++)
            {
                var resistPoq = recordPoq.ResistSheet[i];
                var resistGeneric = genericRecord.ResistSheet[i];
                var resistDifference = (float)Math.Round(resistPoq.resistPercent - resistGeneric.resistPercent, 2);

                if (resistDifference != 0)
                {
                    var value = $"{Math.Round(resistPoq.resistPercent, 2).ToString()} ({FormatDifference(Math.Abs(resistDifference).ToString(), resistDifference)})".WrapInColor(Colors.Green);

                    _factory.AddPanelToTooltip().SetIcon($"damage_{recordPoq.ResistSheet[i].damage}_resist").
                     LocalizeName($"woundeffect.resist_{recordPoq.ResistSheet[i].damage}.desc")
                     .SetValue(value, true)
                     .SetComparsionValue(resistGeneric.resistPercent.ToString());
                }
            }
        }

        private static void InitTraits(WeaponRecord weaponRecord, string genericId, PickupItem item)
        {
            var genericRecord = Data.Items.GetSimpleRecord<WeaponRecord>(genericId, true);
            var component = item.Comp<WeaponComponent>();

            foreach (var id in genericRecord.Traits)
            {
                //_logger.Log($"InitTraits genericRecord: {id}");

                // We are missing generic traits. Add with strikedout effect.
                if (!weaponRecord.Traits.Contains(id))
                {
                    ItemTraitRecord record = Data.ItemTraits.GetRecord(id);
                    _factory.AddPanelToTooltip().SetIcon(record.TooltipIconTag).SetName($"<s>{Localization.Get("trait." + id)}</s>")
                        //.LocalizeName("trait." + id)
                        .SetNameColor(record.IsNegative ? Helpers.DarkenColor(Colors.Red, 0.75f) : Helpers.DarkenColor(Colors.Yellow, 0.75f));
                }
            }

            foreach (var id in weaponRecord.Traits)
            {
                //_logger.Log($"InitTraits {id}");

                if (genericRecord.Traits.Contains(id))
                {
                    continue;
                }

                ItemTraitRecord record = Data.ItemTraits.GetRecord(id);
                _factory.AddPanelToTooltip().SetIcon(record.TooltipIconTag).SetName($"{Localization.Get("trait." + id)}")
                    //.LocalizeName("trait." + id)
                    .SetNameColor(record.IsNegative ? Colors.Red : Colors.Yellow);
            }

            //foreach (ItemTrait itemTrait in component.Traits)
            //{
            //    if (genericRecord.Traits.Contains(itemTrait.TraitId))
            //    {

            //        continue;
            //    }

            //    _tooltipBuilder.AddItemTraitToTooltip(itemTrait.TraitId);
            //}

            //<strikethrough>
            
        }

        private static void InitWeight(ItemRecord itemRecord, string genericId, PickupItem item)
        {
            if (item.TotalWeight > 0)
            {
                var genericRecord = Data.Items.GetSimpleRecord<ItemRecord>(genericId, true);

                float singleWeightPoq = itemRecord.Weight;
                float singleWeightGeneric = genericRecord.Weight;
                float weightDifference = singleWeightPoq - singleWeightGeneric;

                if (weightDifference != 0)
                {
                    var value = $"{FormatHelper.ToWeight(singleWeightPoq)} ({FormatDifference(Math.Abs(Math.Round(weightDifference, 2)).ToString(), weightDifference, true)})  ".WrapInColor(Colors.Green);

                    _factory.AddPanelToTooltip().SetIcon("common_weight").LocalizeName("tooltip.ItemWeight")
                        .SetValue(value, true)
                        .SetComparsionValue(FormatHelper.ToWeight(singleWeightGeneric));
                }
            }
        }

        private static void InitWeapon(WeaponRecord recordPoq, string genericId, PickupItem item)
        {
            _logger.Log($"genericId {genericId}");
            var genericRecord = Data.Items.GetSimpleRecord<WeaponRecord>(genericId, true);
            bool grenadeLauncher = recordPoq.WeaponClass == WeaponClass.GrenadeLauncher;
            string value;

            if (!grenadeLauncher)
            {
                ValueTuple<int, int, float, float, string, string> damagePoq = _tooltipBuilder.GetWeaponDamage(recordPoq, null, null, item);
                ValueTuple<int, int, float, float, string, string> damageGeneric = _tooltipBuilder.GetWeaponDamage(genericRecord, null, null, item);

                string tag = "tooltip.Damage";
                string icon = "common_damage";
                if (!string.IsNullOrEmpty(damagePoq.Item6))
                {
                    tag = "ui.damage." + damagePoq.Item6;
                    icon = "damage_" + damagePoq.Item6;
                }

                // Damage
                // string item4 = (reloadDurationPoq > 1) ? string.Format("{0}-{1} x {2}", magCapacityPoq, num5, reloadDurationPoq) : string.Format("{0}-{1}", magCapacityPoq, num5);
                var dmgDifferenceMin = damagePoq.Item1 - damageGeneric.Item1;
                var dmgDifferenceMax = damagePoq.Item2 - damageGeneric.Item2;

                if (dmgDifferenceMin != 0 || dmgDifferenceMax != 0)
                {
                    value = $"{string.Format("{0}-{1}", damagePoq.Item1, damagePoq.Item2).ToString()} ({FormatDifference(string.Format("{0}-{1}", Math.Abs(dmgDifferenceMin), Math.Abs(dmgDifferenceMax)).ToString(), dmgDifferenceMax)})".WrapInColor(Colors.Green);

                    _factory.AddPanelToTooltip().SetIcon(icon).LocalizeName(tag)
                    .SetValue(value, true)
                    .SetComparsionValue(damageGeneric.Item5);
                }

                // Crit damage
                var critDifference = damagePoq.Item3 - damageGeneric.Item3;

                if (critDifference != 0)
                {
                    value = $"{FormatHelper.To100Percent(damagePoq.Item3, false).ToString()} ({FormatDifference(FormatHelper.To100Percent(Math.Abs(critDifference), false).ToString(), critDifference)})".WrapInColor(Colors.Green);

                    _factory.AddPanelToTooltip().SetIcon("common_critdamage").LocalizeName("tooltip.CritDamage")
                    .SetValue(value, true)
                    .SetComparsionValue(FormatHelper.To100Percent(damageGeneric.Item3, false));
                }

                // Accuracy
                float accuracy = _tooltipBuilder.GetWeaponAccuracy(recordPoq, null, null);
                float eqAccuracy = _tooltipBuilder.GetWeaponAccuracy(genericRecord, null, null);
                var accuracyDifference = (float)Math.Round(accuracy - eqAccuracy, 2);

                if (accuracyDifference != 0)
                {
                    value = $"{FormatHelper.To100Percent(accuracy, false).ToString()} ({FormatDifference(FormatHelper.To100Percent(Math.Abs(accuracyDifference), false).ToString(), accuracyDifference)})".WrapInColor(Colors.Green);

                    _factory.AddPanelToTooltip().SetIcon(recordPoq.IsMelee ? "common_accuracy_melee" : "common_accuracy").
                    LocalizeName(recordPoq.IsMelee ? "tooltip.MeleeAccuracy" : "tooltip.RangeAccuracy")
                    .SetValue(value, true)
                    .SetComparsionValue(FormatHelper.To100Percent(eqAccuracy, false));
                }

                // Crit chance
                float critChance = _tooltipBuilder.GetWeaponCritChance(recordPoq, null, null);
                float eqCritChance = _tooltipBuilder.GetWeaponCritChance(genericRecord, null, null);
                var critChanceDifference = (float)Math.Round(critChance - eqCritChance, 2);

                if (critChanceDifference != 0)
                {
                    value = $"{FormatHelper.To100Percent(critChance, false).ToString()} ({FormatDifference(FormatHelper.To100Percent(critChanceDifference).ToString(), critChanceDifference, true)})".WrapInColor(Colors.Green);

                    _factory.AddPanelToTooltip().SetIcon("common_critchance").LocalizeName("tooltip.CritChance")
                    .SetValue(value, true)
                    .SetComparsionValue(FormatHelper.To100Percent(eqCritChance, false));
                }

                // Melee stuff
                if (!recordPoq.IsMelee)
                {
                    float scatterAngle = _tooltipBuilder.GetScatterAngle(recordPoq, null, null);
                    float eqScatterAngle = _tooltipBuilder.GetScatterAngle(genericRecord, null, null);
                    var scatterAngleDifference = (float)Math.Round(scatterAngle - eqScatterAngle, 2);

                    if (scatterAngleDifference != 0)
                    {
                        value = $"{string.Format("{0:0.0;0.0}°", scatterAngle).ToString().ToString()} ({FormatDifference(string.Format("{0:0.0;0.0}°", Math.Abs(scatterAngleDifference)).ToString(), scatterAngleDifference, true)})".WrapInColor(Colors.Green);

                        _factory.AddPanelToTooltip().SetIcon("common_scatterangle").LocalizeName("tooltip.ScatterAngle")
                        .SetValue(value, true)
                        .SetComparsionValue(string.Format("{0:0.0;0.0}°", eqScatterAngle));
                    }
                }

                // Weapon Range
                Vector2Int weaponRange = _tooltipBuilder.GetWeaponDistance(recordPoq, null, null);
                Vector2Int eqWeaponRange = _tooltipBuilder.GetWeaponDistance(genericRecord, null, null);
                Vector2Int rangeDiff = weaponRange - eqWeaponRange;
                bool showRange = false;

                //if (rangeDiff.x != 0 || rangeDiff.y == 0)
                string val, eqWeaponRangeText;

                if (weaponRange.x == 0)
                {
                    if (rangeDiff.y != 0)
                    {
                        showRange = true;
                    }

                    val = string.Format("{0}", weaponRange.y);
                    value = $"{val.ToString()} ({FormatDifference(val.ToString(), rangeDiff.y)})".WrapInColor(Colors.Green);
                }
                else
                {
                    if (rangeDiff.x != 0 || rangeDiff.y != 0)
                    {
                        showRange = true;
                    }

                    val = string.Format("{0}-{1}", weaponRange.x, weaponRange.y);
                    value = $"{val.ToString()} ({FormatDifference(val.ToString(), rangeDiff.y)})".WrapInColor(Colors.Green);
                }

                if (eqWeaponRange.x == 0)
                {
                    eqWeaponRangeText = string.Format("{0}", eqWeaponRange.y);
                }
                else
                {
                    eqWeaponRangeText = string.Format("{0}-{1}", eqWeaponRange.x, eqWeaponRange.y);
                }

                if (showRange)
                {
                    _factory.AddPanelToTooltip().SetIcon("common_distance").LocalizeName("tooltip.WeaponMaxDistance")
                        .SetValue(value, true)
                        .SetComparsionValue(eqWeaponRangeText);
                }

                // Melee Throw Range
                if (recordPoq.IsMelee && recordPoq.ThrowRange > 0)
                {
                    var throwDifference = recordPoq.ThrowRange - genericRecord.ThrowRange;
                    value = $"{recordPoq.ThrowRange.ToString()} ({FormatDifference(throwDifference.ToString(), throwDifference)})".WrapInColor(Colors.Green);
                    if (throwDifference != 0)
                    {
                        _factory.AddPanelToTooltip().SetIcon("common_throwrange").LocalizeName("tooltip.ThrowRange")
                            .SetValue(value, true)
                            .SetComparsionValue(genericRecord.ThrowRange.ToString());
                    }

                }

                // Reload duration
                if (!string.IsNullOrEmpty(recordPoq.RequiredAmmo))
                {
                    int reloadDurationPoq = Mathf.Max(recordPoq.ReloadDuration, 1);
                    int reloadDurationGeneric = Mathf.Max(genericRecord.ReloadDuration, 1);
                    var reloadDifference = reloadDurationPoq - reloadDurationGeneric;

                    value = $"{string.Format("{0} {1}", reloadDurationPoq, Localization.Get("ui.label.actionpoints_short"))}({FormatDifference(Math.Abs(reloadDifference).ToString(), reloadDifference, true)})".WrapInColor(Colors.Green);
                    if (reloadDifference != 0)
                    {
                        _factory.AddPanelToTooltip().SetIcon("common_time").LocalizeName("tooltip.ReloadDuration")
                            .SetValue(value, true)
                            .SetComparsionValue(reloadDurationGeneric.ToString());
                    }

                }

                // Magazine Capacity
                if (!string.IsNullOrEmpty(recordPoq.RequiredAmmo))
                {
                    string str = grenadeLauncher ? recordPoq.DefaultGrenadeId : recordPoq.DefaultAmmoId;
                    string iconMagCapacity = grenadeLauncher ? "ammo_grenade" : ("ammo_" + recordPoq.RequiredAmmo.ToLower());

                    if (grenadeLauncher && item != null)
                    {
                        LauncherComponent launcherComponent = item.Comp<LauncherComponent>();
                        if (launcherComponent.LoadedGrenadesIds.Count > 0)
                        {
                            str = launcherComponent.LoadedGrenadesIds[0];
                        }
                    }

                    int magCapacityPoq = recordPoq.MagazineCapacity;
                    int magCapacityGeneric = genericRecord.MagazineCapacity;
                    var magCapacityDifference = magCapacityPoq - magCapacityGeneric;

                    if (magCapacityDifference != 0)
                    {
                        value = $"{magCapacityPoq} ({FormatDifference(Math.Abs(magCapacityDifference).ToString(), magCapacityDifference)})".WrapInColor(Colors.Green);

                        _factory.AddPanelToTooltip().SetIcon(iconMagCapacity).LocalizeName("item." + str + ".name")
                            .SetValue(value, true)
                            .SetComparsionValue(magCapacityGeneric.ToString());
                    }

                    // Firerate
                    // We can't apply that in magnum project. Skip.

                }
            }
        }

        private static void InitBreakable(BreakableItemRecord recordPoq, string genericId, PickupItem item)
        {
            var genericRecord = Data.Items.GetSimpleRecord<BreakableItemRecord>(genericId, true);

            // Max durability
            var durabilityDifference = recordPoq.MaxDurability - genericRecord.MaxDurability;

            //_logger.Log($"breakableComponent.MaxDurability {recordPoq.MaxDurability}");
            //_logger.Log($"genericRecord.MaxDurability {genericRecord.MaxDurability}");

            bool unbreakable = false;

            foreach (var component in item.Components)
            {
                var breakableItemComponent = component as BreakableItemComponent;

                if (breakableItemComponent != null)
                {
                    if (breakableItemComponent.Unbreakable)
                    {
                        unbreakable = true;
                        break;
                    }
                }
            }

            if (durabilityDifference != 0 || unbreakable)
            {
                var value = $"{recordPoq.MaxDurability.ToString()} ({FormatDifference(Math.Abs(durabilityDifference).ToString(), durabilityDifference)})".WrapInColor(Colors.Green);

                _factory.AddPanelToTooltip().SetIcon("common_condition").LocalizeName("tooltip.Condition")
                .SetValue(unbreakable == true ? Localization.Get("poq.ui.tooltip.unbreakable") : value, true)
                .SetComparsionValue(genericRecord.MaxDurability.ToString());
            }
        }

        internal static void HandlePoqTooltipMonsterRemove()
        {
            SingletonMonoBehaviour<TooltipFactory>.Instance.HideTooltip();
        }

        internal static void HandlePoqTooltipMonster(ObjHighlightController instance, CellPosition cellUnderCursor)
        {
            MapCell cell = instance._mapGrid.GetCell(cellUnderCursor, true);
            if (cell != null)
            {
                Monster monster = instance._creatures.GetMonster(cellUnderCursor.X, cellUnderCursor.Y);
                if (monster != null)
                {
                    HandlePoqTooltipMonster(monster);
                }
            }
        }

        internal static void HandlePoqTooltipMonster(Creature monster)
        {
            InputController inputController = SingletonMonoBehaviour<InputController>.Instance;
            var IsKeyDown = inputController.IsKeyDown("HighlightAllItems", null, false);
            var IsKeyUp = inputController.IsKeyUp("HighlightAllItems", null, false);

            // We got entry that we can show
            if (IsKeyDown && PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.ContainsKey(monster.CreatureData.UniqueId))
            {
                CreatureDataPoq creatureData = null;

                if (PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.ContainsKey(monster.CreatureData.UniqueId))
                {
                    creatureData = PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq[monster.CreatureData.UniqueId];
                }
                else
                {
                    return;
                }


                //if (monster != null)// && SingletonMonoBehaviour<TooltipFactory>.Instance.IsTooltipWithAdditHintActive)
                _factory = SingletonMonoBehaviour<TooltipFactory>.Instance;
                _factory._state.Resolve(_factory._itemTooltipBuilder);

                _tooltip = _factory.BuildEmptyTooltip(true, true);
                _tooltip.MakeRed();
                _tooltip.SetCaption1(Localization.Get("monster." + monster.CreatureData.LocalizationId + ".name"), _factory.FirstLetterColor);
                //_tooltip.SetCaption1Right($"{monster.CreatureData.UniqueId} :: {PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq[monster.CreatureData.UniqueId].rarity.ToString().WrapInColor(CreaturesControllerPoq.MonsterMasteryColors[PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq[monster.CreatureData.UniqueId].rarity].Replace("#", string.Empty))}");
                _tooltip.SetCaption1Right($"ID: {monster.CreatureData.UniqueId}");
                //_factory.AddCompareBlock(new PickupItem());
                //_tooltip.SetCaption2(Localization.Get("item.ledgerBook.shortdesc"));
                _tooltip.SetCaption2($"{PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq[monster.CreatureData.UniqueId].rarity.ToString().WrapInColor(CreaturesControllerPoq.MonsterMasteryColors[PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq[monster.CreatureData.UniqueId].rarity].Replace("#", string.Empty)).ToUpper()}");
                _tooltip.SetWidth(160);

                if (creatureData.rarity == MonsterMasteryTier.None)
                {
                    return;
                }

                // CompareBlock
                _tooltip._equippedIcon.sprite = Helpers.FindSpriteByName("difficulty_skull");
                _tooltip._equippedIcon.color = new Color(0f, 0f, 0f, 0f);
                _tooltip._equippedIcon.type = Image.Type.Simple;
                _tooltip._equippedIcon.preserveAspect = true;
                _tooltip._compareBlock.SetActive(value: true);

                //foreach(var entry in Data.TooltipIcons.Entries)
                //{
                //    Console.WriteLine($"{entry.Tag} -=- {entry.SpriteName}");
                //}

                //health
                var (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "health");

                var valueBrackets = $"({FormatDifference(Math.Abs(diffVal))})";

                _factory.AddPanelToTooltip().SetIcon("common_health").LocalizeName($"tooltip.Health")
                    .SetValue($"{newVal} {valueBrackets}")
                    .SetComparsionValue(oldVal.ToString());

                // action points
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "actionPoints");

                valueBrackets = $"({FormatDifference(Math.Abs(diffVal))})";

                _factory.AddPanelToTooltip().SetIcon("common_action_points").LocalizeName($"tooltip.ActionPoints")
                  .SetValue($"{newVal} {valueBrackets}")
                  .SetComparsionValue(oldVal.ToString());

                // Ranged Combat
                _factory.AddPanelToTooltip().SetValue(Localization.Get("ui.mercclass.range").WrapInColor(Helpers.HexStringToUnityColor("#FFFEC1")));

                // _basicRangeAccuracy
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "rangeAccuracy");

                valueBrackets = $"({FormatDifference(FormatHelper.To100Percent(Math.Abs(diffVal), false), diffVal)})";

                _factory.AddPanelToTooltip().SetIcon("common_accuracy").LocalizeName($"ui.mercclass.basicaccuracy")
                    .SetValue($"{FormatHelper.To100Percent(newVal, false)} {valueBrackets}")
                    .SetComparsionValue(FormatHelper.To100Percent(oldVal, false).ToString());

                // _visionDistance
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "losLevel");

                valueBrackets = $"({FormatDifference(Math.Abs(diffVal))})";

                _factory.AddPanelToTooltip().SetIcon("common_vision").LocalizeName($"ui.mercclass.visiondistance")
                  .SetValue($"{newVal} {valueBrackets}")
                  .SetComparsionValue(oldVal.ToString());

                // _weaponsDamage
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "weaponsDamageBonus");

                valueBrackets = $"({FormatDifference(FormatHelper.To100Percent(Math.Abs(diffVal), false), diffVal)})";

                _factory.AddPanelToTooltip().SetIcon("common_damage").LocalizeName($"ui.mercclass.weaponsdamage")
                    .SetValue($"{FormatHelper.To100Percent(newVal, false)} {valueBrackets}")
                    .SetComparsionValue(FormatHelper.To100Percent(oldVal, false).ToString());

                // Close Combat
                _factory.AddPanelToTooltip().SetValue(Localization.Get("ui.mercclass.melee").WrapInColor(Helpers.HexStringToUnityColor("#FFFEC1")));

                // _hitChance
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "weaponsDamageBonus");

                valueBrackets = $"({FormatDifference(FormatHelper.To100Percent(Math.Abs(diffVal), false), diffVal)})";

                _factory.AddPanelToTooltip().SetIcon("common_accuracy").LocalizeName($"ui.mercclass.hitchance")
                    .SetValue($"{FormatHelper.To100Percent(newVal, false)} {valueBrackets}")
                    .SetComparsionValue(FormatHelper.To100Percent(oldVal, false).ToString());

                // _handsDamageMin Max
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "handsDamageMin");
                var (oldVal2, newVal2, diffVal2) = creatureData.GetCreatureStats(creatureData, "handsDamageMax");

                valueBrackets = $"({FormatDifference(string.Format("{0}-{1}", diffVal, diffVal2).ToString(), diffVal2)})";

                _factory.AddPanelToTooltip().SetIcon("common_damage_melee").LocalizeName($"ui.mercclass.handsdamage")
                    .SetValue($"{newVal} - {newVal2} {valueBrackets}")
                    .SetComparsionValue($"{oldVal} - {oldVal2}");

                // _meleeBoost
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "meleeDamageBonus");

                valueBrackets = $"({FormatDifference(FormatHelper.To100Percent(Math.Abs(diffVal), false), diffVal)})";

                _factory.AddPanelToTooltip().SetIcon("common_damage").LocalizeName($"ui.mercclass.meleeboost")
                    .SetValue($"{FormatHelper.To100Percent(newVal, false)} {valueBrackets}")
                    .SetComparsionValue(FormatHelper.To100Percent(oldVal, false).ToString());

                // _meleeCritChance
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "meleeCritChance");

                valueBrackets = $"({FormatDifference(FormatHelper.To100Percent(Math.Abs(diffVal), false), diffVal)})";

                _factory.AddPanelToTooltip().SetIcon("common_critchance").LocalizeName($"ui.mercclass.meleecritchance")
                    .SetValue($"{FormatHelper.To100Percent(newVal, false)} {valueBrackets}")
                    .SetComparsionValue(FormatHelper.To100Percent(oldVal, false).ToString());

                // meleeCritDamage
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "meleeCritDamage");

                valueBrackets = $"({FormatDifference(FormatHelper.To100Percent(Math.Abs(diffVal), false), diffVal)})";

                _factory.AddPanelToTooltip().SetIcon("common_critdamage").LocalizeName($"tooltip.CritDamage")
                    .SetValue($"{FormatHelper.To100Percent(newVal, false)} {valueBrackets}")
                    .SetComparsionValue(FormatHelper.To100Percent(oldVal, false).ToString());

                // Defense
                _factory.AddPanelToTooltip().SetValue(Localization.Get("ui.mercclass.defense").WrapInColor(Helpers.HexStringToUnityColor("#FFFEC1")));

                // _dodgeChance
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "dodge");

                valueBrackets = $"({FormatDifference(FormatHelper.To100Percent(Math.Abs(diffVal), false), diffVal)})";

                _factory.AddPanelToTooltip().SetIcon("common_dodge").LocalizeName($"ui.mercclass.dodgechance")
                    .SetValue($"{FormatHelper.To100Percent(newVal, false)} {valueBrackets}")
                    .SetComparsionValue(FormatHelper.To100Percent(oldVal, false).ToString());

                _factory._lastItemMousePos = Input.mousePosition;
            }

            if (IsKeyUp)
            {
                HandlePoqTooltipMonsterRemove();
            }
        }

        internal void ApplyColors()
        {
            DifferenceColorMap["positive"] = Helpers.AlphaAwareColorToHex(Plugin.Config.DifferenceColor_Positive);
            DifferenceColorMap["negative"] = Helpers.AlphaAwareColorToHex(Plugin.Config.DifferenceColor_Negative);
            DifferenceColorMap["equal"] = Helpers.AlphaAwareColorToHex(Plugin.Config.DifferenceColor_Equal);
        }

        internal void BuildSynthraformerTooltip(ItemTooltipBuilder __instance, SynthraformerRecord synRec, bool additional = false)
        {
            // Gotta build our own since we use repair record direvatives

            //Plugin.Logger.Log($"synRec.Type: {synRec.Type}");
            __instance._tooltip = __instance._factory.BuildEmptyTooltip(true, false);

            // Amplifiers have rarity
            if (synRec.Type == SynthraformerController.SynthraformerType.Amplifier)
            {
                PropertiesTooltipHelper.SetCaption1(__instance._tooltip, Localization.Get($"item.{synRec.GetId()}.name"), __instance._factory.FirstLetterColor, RaritySystem.Colors[synRec.Rarity]);
                PropertiesTooltipHelper.SetCaption2(__instance._tooltip, Localization.Get($"item.{synRec.BaseId}.shortdesc"), RaritySystem.Colors[synRec.Rarity]);

                // Rarity
                __instance._factory.AddPanelToTooltip().SetValue(synRec.Rarity.ToString().WrapInColor(RaritySystem.Colors[synRec.Rarity].Replace("#", string.Empty)));

                // Quote
                __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"ui.quote.{synRec.GetId(true)}.quote.nahuatl")).SetNameColor(Colors.AltGreen);
            }
            else
            {
                __instance._tooltip.SetCaption1(Localization.Get("item." + synRec.GetId() + ".name"), __instance._factory.FirstLetterColor);
                __instance._tooltip.SetCaption2(Localization.Get("item." + synRec.BaseId + ".shortdesc"));

                // Quote
                __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"ui.quote.{synRec.GetId()}.quote.nahuatl")).SetNameColor(Colors.AltGreen);
            }

            __instance._tooltip.ShowAdditionalBlock();

            if (additional)
            {
                // Desc
                __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"item.{synRec.GetId()}.desc")).SetNameColor(Colors.DarkYellow);
            }
        }
    }
}
