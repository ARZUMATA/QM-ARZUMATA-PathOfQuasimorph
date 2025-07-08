using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System.Globalization;

namespace QM_PathOfQuasimorph.Core
{
    internal class TooltipGeneratorPoq
    {
        static TooltipFactory _factory;
        static PropertiesTooltip _tooltip;
        static ItemTooltipBuilder _tooltipBuilder;

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

                    var wrappedItem = MagnumProjectWrapper.SplitItemUid(_factory._lastShowedItem.Id);
                    Plugin.Logger.Log($"wrappedItem.CustomId {wrappedItem.ReturnItemUid()}");

                    if (wrappedItem.PoqItem)
                    {
                        _tooltip = _factory.BuildEmptyTooltip();
                        _tooltip.SetCaption1(Localization.Get("item." + wrappedItem.ReturnItemUid() + ".name"), _factory.FirstLetterColor);
                        _tooltip.SetCaption2(Localization.Get("item." + wrappedItem.ReturnItemUid() + ".shortdesc"));
                        _tooltip.SetCaption1Right(wrappedItem.RarityClass.ToString(CultureInfo.InvariantCulture).WrapInColor(RaritySystem.Colors[wrappedItem.RarityClass].Replace("#", string.Empty)));
                        //_factory.AddPanelToTooltip().SetValue("Difference");

                        //_factory._tooltip.MakeRed();
                        _factory.AddCompareBlock(_factory._lastShowedItem);
                        //_factory.AddCompareBlock(_factory._lastShowedItem);

                        if (_factory._lastShowedItem.Is<WeaponRecord>())
                        {
                            InitItemComparsionWeapon(_factory._lastShowedItem as PickupItem, wrappedItem.Id);
                        }
                        else if (_factory._lastShowedItem.Is<BreakableItemRecord>())
                        {
                            InitItemComparsionArmor(_factory._lastShowedItem as PickupItem, wrappedItem.Id);
                        }

                        _factory._tooltip.IsAdditionalTooltip = true;
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
            // #2196F3  // Material Design blue
            // #F44336  // Material Design red

            bool isPositive = difference >= 0;
            if (invert)
            {
                isPositive = !isPositive;
            }

            return isPositive ? "2196F3" : "F44336";
        }

        static string GetDifferenceSign(float difference, bool invert = false)
        {
            bool isPositive = difference >= 0;
            if (invert)
            {
                isPositive = !isPositive;
            }

            return isPositive ? "+" : "-";
        }

        static string FormatDifference(string label, float difference, bool invertColor = false, bool invertSign = false)
        {
            string sign = GetDifferenceSign(difference, invertSign);
            string color = GetDifferenceColor(difference, invertColor);

            return $"<color=#{color}>{sign}{label}</color>";
        }

        private static void InitItemComparsionWeapon(PickupItem item, string genericId)
        {
            InitBreakable(item.Record<BreakableItemRecord>(), genericId, item);
            InitWeapon(item.Record<WeaponRecord>(), genericId, item);
            InitTraits(item.Record<WeaponRecord>(), genericId, item);
            InitWeight(item.Record<ItemRecord>(), genericId, item);
        }

        private static void InitItemComparsionArmor(PickupItem item, string genericId)
        {
            InitBreakable(item.Record<BreakableItemRecord>(), genericId, item);
            InitArmor(item.Record<ResistRecord>(), genericId, item);
            InitWeight(item.Record<ItemRecord>(), genericId, item);
        }


        private static void InitArmor(ResistRecord recordPoq, string genericId, PickupItem item)
        {
            var genericRecord = Data.Items.GetSimpleRecord<ResistRecord>(genericId, true);

            Plugin.Logger.Log($"genericRecord ResistRecord is {genericRecord == null}");

            // blunt 5 pierce 0 lacer 0 fire 0 cold 5 poison 0 shock 0 beam 0
            for (int i = 0; i < genericRecord.ResistSheet.Count; i++)
            {
                var resistPoq = recordPoq.ResistSheet[i];
                var resistGeneric = genericRecord.ResistSheet[i];
                var resistDifference = (float)Math.Round(resistPoq.resistPercent - resistGeneric.resistPercent, 2);

                if (resistDifference != 0)
                {
                    //var value = $"{FormatHelper.To100Percent(resistPoq.resistPercent, false).ToString(CultureInfo.InvariantCulture)} ({FormatDifference(FormatHelper.To100Percent(resistDifference, false).ToString(CultureInfo.InvariantCulture), resistDifference)})".WrapInColor(Colors.Green);
                    var value = $"{Math.Round(resistPoq.resistPercent,2).ToString(CultureInfo.InvariantCulture)} ({FormatDifference(resistDifference.ToString(CultureInfo.InvariantCulture), resistDifference)})".WrapInColor(Colors.Green);

                    _factory.AddPanelToTooltip().SetIcon($"damage_{recordPoq.ResistSheet[i].damage}_resist").
                     LocalizeName($"woundeffect.resist_{recordPoq.ResistSheet[i].damage}.desc")
                     .SetValue(value, true)
                     .SetComparsionValue(resistGeneric.resistPercent.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private static void InitTraits(WeaponRecord weaponRecord, string genericId, PickupItem item)
        {
            var genericRecord = Data.Items.GetSimpleRecord<WeaponRecord>(genericId, true);
            var component = item.Comp<WeaponComponent>();

            foreach (ItemTrait itemTrait in component.Traits)
            {
                if (genericRecord.Traits.Contains(itemTrait.TraitId))
                {
                    continue;
                }

                _tooltipBuilder.AddItemTraitToTooltip(itemTrait.TraitId);
            }
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
                    var value = $"{FormatHelper.ToWeight(singleWeightPoq)} ({FormatDifference(Math.Abs(Math.Round(weightDifference, 2)).ToString(CultureInfo.InvariantCulture), weightDifference, true)})  ".WrapInColor(Colors.Green);

                    _factory.AddPanelToTooltip().SetIcon("common_weight").LocalizeName("tooltip.ItemWeight")
                        .SetValue(value, true)
                        .SetComparsionValue(FormatHelper.ToWeight(singleWeightGeneric));
                }
            }
        }

        private static void InitWeapon(WeaponRecord recordPoq, string genericId, PickupItem item)
        {
            var component = item.Comp<WeaponComponent>();

            Plugin.Logger.Log($"component {component == null}");
            Plugin.Logger.Log($"genericId {genericId}");
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
                    value = $"{string.Format("{0}-{1}", damagePoq.Item1, damagePoq.Item2).ToString(CultureInfo.InvariantCulture)} ({FormatDifference(string.Format("{0}-{1}", dmgDifferenceMin, dmgDifferenceMax).ToString(CultureInfo.InvariantCulture), dmgDifferenceMax)})".WrapInColor(Colors.Green);

                    _factory.AddPanelToTooltip().SetIcon(icon).LocalizeName(tag)
                    .SetValue(value, true)
                    .SetComparsionValue(damageGeneric.Item5);
                }

                // Crit damage
                var critDifference = damagePoq.Item3 - damageGeneric.Item3;

                if (critDifference != 0)
                {
                    value = $"{FormatHelper.To100Percent(damagePoq.Item3, false).ToString(CultureInfo.InvariantCulture)} ({FormatDifference(FormatHelper.To100Percent(critDifference, false).ToString(CultureInfo.InvariantCulture), critDifference)})".WrapInColor(Colors.Green);

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
                    value = $"{FormatHelper.To100Percent(accuracy, false).ToString(CultureInfo.InvariantCulture)} ({FormatDifference(FormatHelper.To100Percent(accuracyDifference, false).ToString(CultureInfo.InvariantCulture), accuracyDifference)})".WrapInColor(Colors.Green);

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
                    value = $"{FormatHelper.To100Percent(critChance, false).ToString(CultureInfo.InvariantCulture)} ({FormatDifference(FormatHelper.To100Percent(critChanceDifference).ToString(CultureInfo.InvariantCulture), critChanceDifference, true)})".WrapInColor(Colors.Green);

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
                        value = $"{string.Format("{0:0.0;0.0}°", scatterAngle).ToString(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)} ({FormatDifference(string.Format("{0:0.0;0.0}°", Math.Abs(scatterAngleDifference)).ToString(CultureInfo.InvariantCulture), scatterAngleDifference, true)})".WrapInColor(Colors.Green);

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
                    value = $"{val.ToString(CultureInfo.InvariantCulture)} ({FormatDifference(val.ToString(CultureInfo.InvariantCulture), rangeDiff.y)})".WrapInColor(Colors.Green);
                }
                else
                {
                    if (rangeDiff.x != 0 || rangeDiff.y != 0)
                    {
                        showRange = true;
                    }

                    val = string.Format("{0}-{1}", weaponRange.x, weaponRange.y);
                    value = $"{val.ToString(CultureInfo.InvariantCulture)} ({FormatDifference(val.ToString(CultureInfo.InvariantCulture), rangeDiff.y)})".WrapInColor(Colors.Green);
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
                    value = $"{recordPoq.ThrowRange.ToString(CultureInfo.InvariantCulture)} ({FormatDifference(throwDifference.ToString(CultureInfo.InvariantCulture), throwDifference)})".WrapInColor(Colors.Green);
                    if (throwDifference != 0)
                    {
                        _factory.AddPanelToTooltip().SetIcon("common_throwrange").LocalizeName("tooltip.ThrowRange")
                            .SetValue(value, true)
                            .SetComparsionValue(genericRecord.ThrowRange.ToString(CultureInfo.InvariantCulture));
                    }

                }

                // Reload duration
                if (!string.IsNullOrEmpty(recordPoq.RequiredAmmo))
                {
                    int reloadDurationPoq = Mathf.Max(recordPoq.ReloadDuration, 1);
                    int reloadDurationGeneric = Mathf.Max(genericRecord.ReloadDuration, 1);
                    var reloadDifference = reloadDurationPoq - reloadDurationGeneric;

                    value = $"{string.Format("{0} {1}", reloadDurationPoq, Localization.Get("ui.label.actionpoints_short"))}({FormatDifference(Math.Abs(reloadDifference).ToString(CultureInfo.InvariantCulture), reloadDifference, true)})".WrapInColor(Colors.Green);
                    if (reloadDifference != 0)
                    {
                        _factory.AddPanelToTooltip().SetIcon("common_time").LocalizeName("tooltip.ReloadDuration")
                            .SetValue(value, true)
                            .SetComparsionValue(reloadDurationGeneric.ToString(CultureInfo.InvariantCulture));
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
                        value = $"{magCapacityPoq} ({FormatDifference(magCapacityDifference.ToString(CultureInfo.InvariantCulture), magCapacityDifference)})".WrapInColor(Colors.Green);

                        _factory.AddPanelToTooltip().SetIcon(iconMagCapacity).LocalizeName("item." + str + ".name")
                            .SetValue(value, true)
                            .SetComparsionValue(magCapacityGeneric.ToString(CultureInfo.InvariantCulture));
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

            Plugin.Logger.Log($"breakableComponent.MaxDurability {recordPoq.MaxDurability}");
            Plugin.Logger.Log($"genericRecord.MaxDurability {genericRecord.MaxDurability}");

            if (durabilityDifference != 0)
            {
                var value = $"{recordPoq.MaxDurability.ToString(CultureInfo.InvariantCulture)} ({FormatDifference(durabilityDifference.ToString(CultureInfo.InvariantCulture), durabilityDifference)})".WrapInColor(Colors.Green);

                _factory.AddPanelToTooltip().SetIcon("common_condition").LocalizeName("tooltip.Condition")
                .SetValue(recordPoq.Unbreakable == true ? "∞" : value, true)
                .SetComparsionValue(genericRecord.MaxDurability.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
