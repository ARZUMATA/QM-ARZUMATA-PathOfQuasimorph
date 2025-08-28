using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.PoqHelpers;
using QM_PathOfQuasimorph.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static QM_PathOfQuasimorph.Controllers.CreaturesControllerPoq;
using static UnityEngine.Rendering.DebugUI;

namespace QM_PathOfQuasimorph.Core
{
    internal class TooltipGeneratorPoq
    {
        private const float FLOAT_TOLERANCE = 0.001f;
        private static readonly Dictionary<string, string> DifferenceColorMap = new Dictionary<string, string>
        {
            { "positive", "#2196F3" },      // #2196F3  // Material Design Blue
            { "negative", "#F44336" },      // #F44336  // Material Design Red
            { "equal", "#444444" }          // #444444  // Gray
        };

        static TooltipFactory _factory;
        private static Logger _logger = new Logger(null, typeof(TooltipGeneratorPoq));
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

                    var metadata = RecordCollection.MetadataWrapperRecords.GetOrAdd(_factory._lastShowedItem.Id, MetadataWrapper.SplitItemUid);

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
                        _logger.Log($"metadata.CustomId {metadata.ReturnItemUid()}");
                        _logger.Log($"metadata.IsMagnumProduced {metadata.IsMagnumProduced}");

                        if (metadata.PoqItem)
                        {
                            _tooltip = _factory.BuildEmptyTooltip();
                            //_tooltip.SetCaption1(Localization.Get("item." + metadata.ReturnItemUid() + ".name"), _factory.FirstLetterColor);

                            var localizedStringCaption2 = Localization.Get("item." + metadata.ReturnItemUid() + ".shortdesc");

                            if (localizedStringCaption2.Length > 35)
                            {
                                localizedStringCaption2 = $"<size=85%>{localizedStringCaption2}</size>";
                            }

                            PropertiesTooltipHelper.SetCaption1(_tooltip, Localization.Get("item." + metadata.ReturnItemUid() + ".name"), _factory.FirstLetterColor, RaritySystem.Colors[metadata.RarityClass]);

                            PropertiesTooltipHelper.SetCaption2(_tooltip, localizedStringCaption2, RaritySystem.Colors[metadata.RarityClass]);

                            //_tooltip.SetCaption2(Localization.Get("item." + metadata.ReturnItemUid() + ".shortdesc"));

                            //_tooltip.SetCaption1Right(metadata.RarityClass.ToString().WrapInColor(RaritySystem.Colors[metadata.RarityClass].Replace("#", string.Empty)));
                            _factory.AddPanelToTooltip().SetValue(metadata.RarityClass.ToString().WrapInColor(RaritySystem.Colors[metadata.RarityClass].Replace("#", string.Empty)));
                            //_factory.AddPanelToTooltip().SetValue("Difference");

                            //_factory._tooltip.MakeRed();
                            _factory.AddCompareBlock(_factory._lastShowedItem);
                            //_factory.AddCompareBlock(_factory._lastShowedItem);

                            InitItemComparsion(_factory._lastShowedItem as PickupItem, metadata);

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
                else
                {
                    HandlePoqTooltipMonsterRemove();
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

                //health
                var (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "health");

                AddNumericComparisonPanel(
                    icon: "common_health",
                    locKey: "tooltip.Health",
                    currentValue: newVal,
                    baseValue: oldVal,
                    unitFormat: "{0}"
                );

                // action points
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "actionPoints");
                AddNumericComparisonPanel(
                    icon: "common_action_points",
                    locKey: "tooltip.ActionPoints",
                    currentValue: newVal,
                    baseValue: oldVal,
                    unitFormat: "{0}"
                    );

                // Ranged Combat
                _factory.AddPanelToTooltip().SetValue(Localization.Get("ui.mercclass.range").WrapInColor(Helpers.HexStringToUnityColor("#FFFEC1")));

                // _basicRangeAccuracy
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "rangeAccuracy");

                AddNumericComparisonPanel(
                    icon: "common_accuracy",
                    locKey: "ui.mercclass.basicaccuracy",
                    currentValue: newVal,
                    baseValue: oldVal,
                    unitFormat: "{0:P0}"
                    );

                // _visionDistance
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "losLevel");

                AddNumericComparisonPanel(
                    icon: "common_vision",
                    locKey: "ui.mercclass.visiondistance",
                    currentValue: newVal,
                    baseValue: oldVal,
                    unitFormat: "{0}"
                    );

                // _weaponsDamage
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "weaponsDamageBonus");

                AddNumericComparisonPanel(
                    icon: "common_damage",
                    locKey: "ui.mercclass.weaponsdamage",
                    currentValue: newVal,
                    baseValue: oldVal,
                    unitFormat: "{0:P0}"
                    );

                // Close Combat
                _factory.AddPanelToTooltip().SetValue(Localization.Get("ui.mercclass.melee").WrapInColor(Helpers.HexStringToUnityColor("#FFFEC1")));

                // _hitChance
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "hitChance");

                AddNumericComparisonPanel(
                    icon: "common_accuracy",
                    locKey: "ui.mercclass.hitchance",
                    currentValue: newVal,
                    baseValue: oldVal,
                    unitFormat: "{0:P0}"
                    );

                // _handsDamageMin Max
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "handsDamageMin");
                var (oldVal2, newVal2, diffVal2) = creatureData.GetCreatureStats(creatureData, "handsDamageMax");

                AddFloatRangeComparisonPanel(
                    icon: "common_damage_melee",
                    locKey: "ui.mercclass.handsdamage",
                    currentMin: newVal,
                    currentMax: newVal2,
                    baseMin: oldVal,
                    baseMax: oldVal2
                );

                // _meleeBoost
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "meleeDamageBonus");

                AddNumericComparisonPanel(
                    icon: "common_damage",
                    locKey: "ui.mercclass.meleeboost",
                    currentValue: newVal,
                    baseValue: oldVal,
                    unitFormat: "{0:P0}"
                );

                // _meleeCritChance
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "meleeCritChance");

                AddNumericComparisonPanel(
                     icon: "common_critchance",
                     locKey: "ui.mercclass.meleecritchance",
                     currentValue: newVal,
                     baseValue: oldVal,
                     unitFormat: "{0:P0}"
                 );

                // meleeCritDamage
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "meleeCritDamage");

                AddNumericComparisonPanel(
                    icon: "common_critdamage",
                    locKey: "tooltip.CritDamage",
                    currentValue: newVal,
                    baseValue: oldVal,
                    unitFormat: "{0:P0}"
                );

                // Defense
                _factory.AddPanelToTooltip().SetValue(Localization.Get("ui.mercclass.defense").WrapInColor(Helpers.HexStringToUnityColor("#FFFEC1")));

                // _dodgeChance
                (oldVal, newVal, diffVal) = creatureData.GetCreatureStats(creatureData, "dodge");
                AddNumericComparisonPanel(
                    icon: "common_dodge",
                    locKey: "ui.mercclass.dodgechance",
                    currentValue: newVal,
                    baseValue: oldVal,
                    unitFormat: "{0:P0}"
                );

                _factory._lastItemMousePos = Input.mousePosition;
            }

            if (IsKeyUp)
            {
                HandlePoqTooltipMonsterRemove();
            }
        }

        internal static void HandlePoqTooltipMonsterRemove()
        {
            SingletonMonoBehaviour<TooltipFactory>.Instance.HideTooltip();
        }

        internal void ApplyColors()
        {
            DifferenceColorMap["positive"] = Helpers.AlphaAwareColorToHex(Plugin.Config.DifferenceColor_Positive);
            DifferenceColorMap["negative"] = Helpers.AlphaAwareColorToHex(Plugin.Config.DifferenceColor_Negative);
            DifferenceColorMap["equal"] = Helpers.AlphaAwareColorToHex(Plugin.Config.DifferenceColor_Equal);
        }

        internal static void BuildSynthraformerTooltip(ItemTooltipBuilder __instance, SynthraformerRecord synRec, bool additional = false)
        {
            // Gotta build our own since we use repair record direvatives

            //Plugin.Logger.Log($"synRec.Type: {synRec.Type}");
            __instance._tooltip = __instance._factory.BuildEmptyTooltip(true, false);

            __instance._tooltip.SetCaption1(Localization.Get("item." + synRec.GetId() + ".name"), __instance._factory.FirstLetterColor);
            __instance._tooltip.SetCaption2(Localization.Get("item." + synRec.BaseId + ".shortdesc"));

            if (!additional)
            {
                // Quote
                __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"ui.quote.{synRec.GetId()}.quote.nahuatl")).SetNameColor(Colors.AltGreen);

                // Desc

                switch (synRec.Type)
                {
                    case SynthraformerController.SynthraformerType.Traits:

                        var value1 = $"{SynthraformerController.TRAIT_CLEAN_CHANCE * 100}%";

                        __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"item.{synRec.GetId()}.desc").SafeFormat(new object[]
                            {
                            $"{value1.WrapInColor(Color.yellow)}"
                            }
                            )).SetNameColor(Colors.DarkYellow);
                        break;

                    case SynthraformerController.SynthraformerType.Indestructible:
                        var value2 = $"{SynthraformerController.TRAIT_CLEAN_CHANCE * 100}%";

                        __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"item.{synRec.GetId()}.desc").SafeFormat(new object[]
                            {
                            $"{value2.WrapInColor(Color.yellow)}"
                            }
                            )).SetNameColor(Colors.DarkYellow);

                        break;


                    case SynthraformerController.SynthraformerType.Transmuter:

                        __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"item.{synRec.GetId()}.desc").SafeFormat(new object[]
                            {
                            $"{ItemRarity.Standard.ToString().WrapInColor(Color.yellow)}",
                            $"{(SynthraformerController.TRANSMUTER_VOID_ITEM_CHANCE * 100).ToString().WrapInColor(Color.yellow)}"
                            }
                            )).SetNameColor(Colors.DarkYellow);

                        break;

                    default:
                        __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"item.{synRec.GetId()}.desc")).SetNameColor(Colors.DarkYellow);
                        break;
                }
            }
            else
            {
                // Quote
                __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"ui.quote.{synRec.GetId()}.quote")).SetNameColor(Colors.AltGreen);
            }

            __instance._tooltip.ShowAdditionalBlock();
        }

        private static void InitAmmo(AmmoRecord ammoRecord, MetadataWrapper metadata, PickupItem item)
        {
            var genericRecord = GetBaseRecord(metadata.Id, metadata.IsMagnumProduced, ammoRecord);

            if (ammoRecord.BallisticType != genericRecord.BallisticType)
            {
                AddStaticComparisonPanel("common_info", "poq.ballistictype.label.tooltip", ammoRecord.BallisticType.ToString(), genericRecord.BallisticType.ToString());
            }

            if (ammoRecord.AmmoType != genericRecord.AmmoType)
            {
                AddStaticComparisonPanel("common_ammo", "poq.ammotype.label.tooltip", ammoRecord.AmmoType.ToString(), genericRecord.AmmoType.ToString());
            }

            if (ammoRecord.DmgType != genericRecord.DmgType)
            {
                AddStaticComparisonPanel($"damage_{ammoRecord.DmgType}", $"poq.damagetype.label.tooltip", Localization.Get($"ui.damage.{ammoRecord.DmgType}"), Localization.Get($"ui.damage.{genericRecord.DmgType}"));
            }
        }

        private static void InitAugmentation(AugmentationRecord augmentationRecord, MetadataWrapper metadata, PickupItem item)
        {
            _logger.Log($"InitAugmentation");

            var genericRecord = GetBaseRecord(metadata.Id, metadata.IsMagnumProduced, augmentationRecord);

            _logger.Log($"genericRecord AugmentationRecord is {genericRecord == null}");
            _logger.Log($"metadata.Id {metadata.Id}");

            WoundSlotRecord[] records = augmentationRecord.WoundSlotIds.Select(id => Data.WoundSlots.GetRecord(id, true)).ToArray();
            WoundSlotRecord[] recordsGeneric = genericRecord.WoundSlotIds.Select(id => Data.WoundSlots.GetRecord(id, true)).ToArray();

            var bonusEffects = WoundSystem.AggregateWoundEffects(records, r => r.ImplicitBonusEffects);
            var penaltyEffects = WoundSystem.AggregateWoundEffects(records, r => r.ImplicitPenaltyEffects);
            var coreEffects = WoundSystem.AggregateWoundEffects(records, r => r.CoreEffects);

            var bonusEffectsGeneric = WoundSystem.AggregateWoundEffects(recordsGeneric, r => r.ImplicitBonusEffects);
            var penaltyEffectsGeneric = WoundSystem.AggregateWoundEffects(recordsGeneric, r => r.ImplicitPenaltyEffects);
            var coreEffectsGeneric = WoundSystem.AggregateWoundEffects(recordsGeneric, r => r.CoreEffects);

            var woundNames = augmentationRecord.WoundSlotIds
                .Select(id =>
                {
                    // Split to get base (e.g. "MoonArm_uid" → "MoonArm")
                    var (baseId, bodyPart) = PoqHelpers.PoqHelpers.StripBodyPart(id.Split('_')[0]);
                    return (baseId, bodyPart);
                })
                .Where(result => !string.IsNullOrEmpty(result.bodyPart))
                .Select(result => $"{result.baseId} {result.bodyPart}") // Combine into "Moon Arm"
                .Distinct()
                .ToList();

            if (woundNames.Any())
            {
                _factory.AddPanelToTooltip()
                    .SetMultilineName(string.Join(", ", woundNames))
                    .SetNameColor(Colors.DarkGreen);
            }

            // Add all effects
            foreach (var effect in coreEffects)
            {
                float baseVal = coreEffectsGeneric.TryGetValue(effect.Key, out float baseValue) ? baseValue : 0f;
                AddWoundEffectComparisonPanel(effect.Key, effect.Value, baseVal, isBonus: true);
            }

            foreach (var effect in bonusEffects)
            {
                float baseVal = bonusEffectsGeneric.TryGetValue(effect.Key, out float baseValue) ? baseValue : 0f;
                AddWoundEffectComparisonPanel(effect.Key, effect.Value, baseVal, isBonus: true);
            }

            foreach (var effect in penaltyEffects)
            {
                float baseVal = penaltyEffectsGeneric.TryGetValue(effect.Key, out float baseValue) ? baseValue : 0f;
                AddWoundEffectComparisonPanel(effect.Key, effect.Value, baseVal, isBonus: false);
            }
        }

        private static void InitBackpackRecord(BackpackRecord backpackRecord, MetadataWrapper metadata, PickupItem item)
        {
            _logger.Log($"InitBackpackRecord");
            _logger.Log($"genericId: {metadata.Id}");

            var genericRecord = GetBaseRecord(metadata.Id, metadata.IsMagnumProduced, backpackRecord);

            AddNumericComparisonPanel(
                icon: "common_inventory_size",
                locKey: "tooltip.InventorySize",
                currentValue: backpackRecord.Height,
                baseValue: genericRecord.Height,
                unitFormat: "{0}"
            );

            AddNumericComparisonPanel(
                icon: "common_weight_mod",
                locKey: "tooltip.BackpackWeightMult",
                currentValue: backpackRecord.BackpackWeightMult,
                baseValue: genericRecord.BackpackWeightMult,
                unitFormat: "{0:P0}"  // → e.g. "90%"
            );

            AddNumericComparisonPanel(
                icon: "common_time",
                locKey: "tooltip.ReloadDuration",
                currentValue: backpackRecord.ReloadTurnMod,
                baseValue: genericRecord.ReloadTurnMod,
                unitFormat: "{0}"
            );
        }

        private static void InitBreakable(BreakableItemRecord recordPoq, MetadataWrapper metadata, PickupItem item)
        {
            var genericRecord = GetBaseRecord(metadata.Id, metadata.IsMagnumProduced, recordPoq);

            var durabilityDifference = recordPoq.MaxDurability - genericRecord.MaxDurability;

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

            if (unbreakable)
            {
                _factory.AddPanelToTooltip()
                    .SetIcon("common_condition")
                    .LocalizeName("tooltip.Condition")
                    .SetValue(Localization.Get("poq.ui.tooltip.unbreakable"), true)
                    .SetComparsionValue(genericRecord.MaxDurability.ToString());
            }
            else
            {
                AddNumericComparisonPanel(
                    icon: "common_condition",
                    locKey: "tooltip.Condition",
                    currentValue: recordPoq.MaxDurability,
                    baseValue: genericRecord.MaxDurability,
                    unitFormat: "{0}"
                );
            }
        }

        private static void InitImplant(ImplantRecord implantRecord, MetadataWrapper metadata, PickupItem item)
        {
            var genericRecord = GetBaseRecord(metadata.Id, metadata.IsMagnumProduced, implantRecord);

            _logger.Log($"genericRecord ImplantRecord is null {genericRecord == null}");

            foreach (var effect in implantRecord.ImplicitBonusEffects)
            {
                float baseVal = genericRecord.ImplicitBonusEffects.TryGetValue(effect.Key, out float baseValue) ? baseValue : 0f;
                AddWoundEffectComparisonPanel(effect.Key, effect.Value, baseVal, isBonus: true);
            }

            foreach (var effect in implantRecord.ImplicitPenaltyEffects)
            {
                float baseVal = genericRecord.ImplicitPenaltyEffects.TryGetValue(effect.Key, out float baseValue) ? baseValue : 0f;
                AddWoundEffectComparisonPanel(effect.Key, effect.Value, baseVal, isBonus: false);
            }
        }

        private static void InitItemComparsion(PickupItem item, MetadataWrapper metadata)
        {
            if (_factory._lastShowedItem.Is<BreakableItemRecord>())
            {
                InitBreakable(item.Record<BreakableItemRecord>(), metadata, item);
            }

            if (_factory._lastShowedItem.Is<AmmoRecord>())
            {
                InitAmmo(item.Record<AmmoRecord>(), metadata, item);
            }

            if (_factory._lastShowedItem.Is<WeaponRecord>())
            {
                InitWeapon(item.Record<WeaponRecord>(), metadata, item);
                InitTraits(item.Record<WeaponRecord>(), metadata, item);
            }

            if (_factory._lastShowedItem.Is<ResistRecord>())
            {
                InitResist(item.Record<ResistRecord>(), metadata, item);
            }

            if (_factory._lastShowedItem.Is<VestRecord>())
            {
                InitVest(item.Record<VestRecord>(), metadata, item);
            }

            if (_factory._lastShowedItem.Is<AugmentationRecord>())
            {
                InitAugmentation(item.Record<AugmentationRecord>(), metadata, item);
            }

            if (_factory._lastShowedItem.Is<ImplantRecord>())
            {
                InitImplant(item.Record<ImplantRecord>(), metadata, item);
            }

            if (_factory._lastShowedItem.Is<BackpackRecord>())
            {
                InitBackpackRecord(item.Record<BackpackRecord>(), metadata, item);
            }

            if (_factory._lastShowedItem.Is<ItemRecord>())
            {
                InitWeight(item.Record<ItemRecord>(), metadata, item);
            }
        }

        private static void InitResist(ResistRecord recordPoq, MetadataWrapper metadata, PickupItem item)
        {
            var genericRecord = GetBaseRecord(metadata.Id, metadata.IsMagnumProduced, recordPoq);

            _logger.Log($"genericRecord ResistRecord is {genericRecord == null}");

            // blunt 5 pierce 0 lacer 0 fire 0 cold 5 poison 0 shock 0 beam 0
            for (int i = 0; i < genericRecord.ResistSheet.Count; i++)
            {
                AddNumericComparisonPanel(
                   icon: $"damage_{recordPoq.ResistSheet[i].damage}",
                   locKey: $"woundeffect.resist_{recordPoq.ResistSheet[i].damage}.desc",
                   currentValue: recordPoq.ResistSheet[i].resistPercent,
                   baseValue: genericRecord.ResistSheet[i].resistPercent,
                   unitFormat: "{0:0.##}%"  // supports decimal + %
               );
            }
        }

        private static void InitTraits(WeaponRecord weaponRecord, MetadataWrapper metadata, PickupItem item)
        {
            var genericRecord = GetBaseRecord(metadata.Id, metadata.IsMagnumProduced, weaponRecord);

            var component = item.Comp<WeaponComponent>();

            foreach (var id in genericRecord.Traits)
            {
                // We are missing generic traits. Add with strikedout effect.
                if (!weaponRecord.Traits.Contains(id))
                {
                    ItemTraitRecord record = Data.ItemTraits.GetRecord(id);
                    _factory.AddPanelToTooltip().SetIcon(record.TooltipIconTag).SetName($"<s>{Localization.Get("trait." + id)}</s>")
                        .SetNameColor(record.IsNegative ? Helpers.DarkenColor(Colors.Red, 0.75f) : Helpers.DarkenColor(Colors.Yellow, 0.75f));
                }
            }

            foreach (var id in weaponRecord.Traits)
            {
                if (genericRecord.Traits.Contains(id))
                {
                    continue;
                }

                ItemTraitRecord record = Data.ItemTraits.GetRecord(id);
                _factory.AddPanelToTooltip().SetIcon(record.TooltipIconTag).SetName($"{Localization.Get("trait." + id)}")
                    .SetNameColor(record.IsNegative ? Colors.Red : Colors.Yellow);
            }
        }

        private static void InitVest(VestRecord vestRecord, MetadataWrapper metadata, PickupItem item)
        {
            _logger.Log($"InitVest");
            _logger.Log($"genericId: {metadata.Id}");

            var genericRecord = GetBaseRecord(metadata.Id, metadata.IsMagnumProduced, vestRecord);

            AddNumericComparisonPanel(
                icon: "common_inventory_size",
                locKey: "tooltip.VestSize",
                currentValue: vestRecord.SlotCapacity,
                baseValue: genericRecord.SlotCapacity,
                unitFormat: "{0}"
            );

            AddNumericComparisonPanel(
                icon: "common_time",
                locKey: "tooltip.ReloadDuration",
                currentValue: vestRecord.ReloadTurnMod,
                baseValue: genericRecord.ReloadTurnMod,
                unitFormat: "{0}"
            );
        }
        private static void InitWeapon(WeaponRecord recordPoq, MetadataWrapper metadata, PickupItem item)
        {
            _logger.Log($"InitWeapon");
            _logger.Log($"genericId: {metadata.Id}");

            var genericRecord = GetBaseRecord(metadata.Id, metadata.IsMagnumProduced, recordPoq);

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

                AddRangeComparisonPanel(
                    icon: icon,
                    locKey: tag,
                    currentMin: damagePoq.Item1,
                    currentMax: damagePoq.Item2,
                    baseMin: damageGeneric.Item1,
                    baseMax: damageGeneric.Item2
                );

                AddNumericComparisonPanel(
                    icon: "common_critdamage",
                    locKey: "tooltip.CritDamage",
                    currentValue: damagePoq.Item3,
                    baseValue: damageGeneric.Item3,
                    unitFormat: "{0:P0}"
                );

                AddNumericComparisonPanel(
                    icon: recordPoq.IsMelee ? "common_accuracy_melee" : "common_accuracy",
                    locKey: recordPoq.IsMelee ? "tooltip.MeleeAccuracy" : "tooltip.RangeAccuracy",
                    currentValue: _tooltipBuilder.GetWeaponAccuracy(recordPoq, null, null),
                    baseValue: _tooltipBuilder.GetWeaponAccuracy(genericRecord, null, null),
                    unitFormat: "{0:P0}"
                );

                AddNumericComparisonPanel(
                    icon: "common_critchance",
                    locKey: "tooltip.CritChance",
                    currentValue: _tooltipBuilder.GetWeaponCritChance(recordPoq, null, null),
                    baseValue: _tooltipBuilder.GetWeaponCritChance(genericRecord, null, null),
                    unitFormat: "{0:P0}"
                );

                // Melee stuff
                if (!recordPoq.IsMelee)
                {
                    float scatterAngle = _tooltipBuilder.GetScatterAngle(recordPoq, null, null);
                    float baseScatterAngle = _tooltipBuilder.GetScatterAngle(genericRecord, null, null);
                    AddNumericComparisonPanel(
                        icon: "common_scatterangle",
                        locKey: "tooltip.ScatterAngle",
                        currentValue: scatterAngle,
                        baseValue: baseScatterAngle,
                        unitFormat: "{0:0.0}°"  // formats to one decimal + degree symbol
                    );
                }

                // Weapon Range
                AddRangedStatComparisonPanel(
                    icon: "common_distance",
                    locKey: "tooltip.WeaponMaxDistance",
                    currentRange: _tooltipBuilder.GetWeaponDistance(recordPoq, null, null),
                    baseRange: _tooltipBuilder.GetWeaponDistance(genericRecord, null, null)
                    );


                // Melee Throw Range
                if (recordPoq.IsMelee && recordPoq.ThrowRange > 0)
                {
                    var throwDifference = recordPoq.ThrowRange - genericRecord.ThrowRange;
                    AddNumericComparisonPanel("common_throwrange", "tooltip.ThrowRange", recordPoq.ThrowRange, genericRecord.ThrowRange, "{0}");
                }

                // Reload duration
                if (!string.IsNullOrEmpty(recordPoq.RequiredAmmo))
                {
                    string ap = Localization.Get("ui.label.actionpoints_short");

                    AddNumericComparisonPanel(
                        icon: "common_time",
                        locKey: "tooltip.ReloadDuration",
                        currentValue: Mathf.Max(recordPoq.ReloadDuration, 1),
                        baseValue: Mathf.Max(genericRecord.ReloadDuration, 1),
                        unitFormat: "{0} " + Localization.Get("ui.label.actionpoints_short")
                    );
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

                    AddNumericComparisonPanel(
                        icon: iconMagCapacity,
                        locKey: "item." + str + ".name",
                        currentValue: recordPoq.MagazineCapacity,
                        baseValue: genericRecord.MagazineCapacity,
                        unitFormat: "{0}"
                    );
                }
            }
        }

        private static void InitWeight(ItemRecord itemRecord, MetadataWrapper metadata, PickupItem item)
        {
            if (item.TotalWeight <= 0)
            {
                return;
            }

            var genericRecord = GetBaseRecord(metadata.Id, metadata.IsMagnumProduced, itemRecord);

            float weight = itemRecord.Weight;
            float baseWeight = genericRecord.Weight;
            float diff = weight - baseWeight;

            AddNumericComparisonPanel(
                icon: "common_weight",
                locKey: "tooltip.ItemWeight",
                currentValue: itemRecord.Weight,
                baseValue: genericRecord.Weight,
                unitFormat: "{0:0.0} kg"
            );
        }

        private static void AddFloatRangeComparisonPanel(
        string icon,
        string locKey,
        float currentMin, float currentMax,
        float baseMin, float baseMax,
        string format = "0.0")
        {
            float diffMax = currentMax - baseMax;
            if (Math.Abs(diffMax) < 0.01f) return;

            string cMin = currentMin.ToString(format);
            string cMax = currentMax.ToString(format);
            string bMin = baseMin.ToString(format);
            string bMax = baseMax.ToString(format);

            string currentRange = $"{cMin}–{cMax}";
            string baseRange = $"{bMin}–{bMax}";
            string diffValue = FormatDifference(Math.Abs(diffMax).ToString(format), diffMax);
            string displayValue = $"{currentRange} ({diffValue})".WrapInColor(Colors.Green);

            _factory.AddPanelToTooltip()
                .SetIcon(icon)
                .LocalizeName(locKey)
                .SetValue(displayValue, true)
                .SetComparsionValue(baseRange);
        }

        private static void AddNumericComparisonPanel(
            string icon,
            string locKey,
            float currentValue,
            float baseValue,
            string unitFormat = "", // e.g. "{0:0.0}°", "{0} kg", "{0:P1}"
            bool invertColor = false,
            bool invertSign = false,
            bool addSignToValue = false)
        {
            float diff = currentValue - baseValue;

            if (Math.Abs(diff) < FLOAT_TOLERANCE) // float tolerance
            {
                return;
            }

            // Format values using the provided format string
            string fmt(string format, float val) => string.Format(format, val);

            string currentValueStr = fmt(unitFormat, currentValue);
            string baseValueStr = fmt(unitFormat, baseValue);
            string absDiffStr = fmt(unitFormat, Math.Abs(diff));

            // Special handling for percent: don't double-format
            if (unitFormat == "{0:P1}" || unitFormat == "{0:P0}")
            {
                // P includes % sign and x100 — so we pass raw diff
                absDiffStr = FormatHelper.To100Percent(Math.Abs(diff), false);
                baseValueStr = FormatHelper.To100Percent(baseValue, false);
                currentValueStr = FormatHelper.To100Percent(currentValue, false);
            }

            string diffFormatted = FormatDifference(absDiffStr, diff, invertColor, invertSign, addsign: true);
            string valueStr = $"{currentValueStr} ({diffFormatted})".WrapInColor(Colors.Green);

            _factory.AddPanelToTooltip()
            .SetIcon(icon)
            .LocalizeName(locKey)
            .SetValue(valueStr, true)
            .SetComparsionValue(baseValueStr);
        }

        private static void AddRangeComparisonPanel(
        string icon,
        string locKey,
        int currentMin, int currentMax,
        int baseMin, int baseMax)
        {
            int diffMax = currentMax - baseMax;
            if (Math.Abs(diffMax) < 1)
            {
                return;
            }

            string currentRange = $"{currentMin}-{currentMax}";
            string baseRange = $"{baseMin}-{baseMax}";
            string diffValue = FormatDifference(Math.Abs(diffMax).ToString(), diffMax);
            string displayValue = $"{currentRange} ({diffValue})".WrapInColor(Colors.Green);

            _factory.AddPanelToTooltip()
                .SetIcon(icon)
                .LocalizeName(locKey)
                .SetValue(displayValue, true)
                .SetComparsionValue(baseRange);
        }

        private static void AddRangedStatComparisonPanel(
            string icon,
            string locKey,
            Vector2Int currentRange,
            Vector2Int baseRange)
        {
            Vector2Int diff = currentRange - baseRange;
            if (diff.y == 0 && currentRange.x == baseRange.x) return;

            string Format(Vector2Int r) => r.x == 0 ? r.y.ToString() : $"{r.x}-{r.y}";

            string currentStr = Format(currentRange);
            string baseStr = Format(baseRange);
            string diffStr = FormatDifference(Math.Abs(diff.y).ToString(), diff.y);
            string displayValue = $"{currentStr} ({diffStr})".WrapInColor(Colors.Green);

            _factory.AddPanelToTooltip()
                .SetIcon(icon)
                .LocalizeName(locKey)
                .SetValue(displayValue, true)
                .SetComparsionValue(baseStr);
        }

        private static void AddStaticComparisonPanel(string icon, string locKey, string currentValue, string baseValue)
        {
            var panel = _factory.AddPanelToTooltip()
                .SetIcon(icon)
                .LocalizeName(locKey)
                .SetValue(currentValue, true);

            if (!string.Equals(currentValue, baseValue))
                panel.SetValue($"<color={DifferenceColorMap["positive"]}>{currentValue} (+)</color>", true)
                .SetComparsionValue(baseValue);
        }

        private static void AddWoundEffectComparisonPanel(
        string effectKey,
        float currentValue,
        float baseValue,
        bool isBonus)
        {
            if (Math.Abs(currentValue - baseValue) < FLOAT_TOLERANCE)
            {
                return;
            }

            WoundEffectRecord record = Data.WoundEffects.GetRecord(effectKey, true);
            if (record == null)
            {
                return;
            }

            string formattedValue = FormatHelper.FormatValue((float)Math.Round(currentValue, 2), record.ValueFormat);
            string formattedDiff = FormatHelper.FormatValue((float)Math.Round(Math.Abs(currentValue - baseValue), 2), record.ValueFormat);
            string diffValue = FormatDifference(formattedDiff, currentValue - baseValue, addsign: false);

            string valueStr = $"{formattedValue} ({diffValue})".WrapInColor(isBonus ? Colors.AltGreen : Colors.LightRed);

            bool isResist = record.TooltipIconTag.Contains("resist");
            string iconSuffix = isResist ? "resist" : (isBonus ? "green" : "red");
            string damageType = effectKey.Replace("resist_", string.Empty);
            string iconTag = isResist ? $"damage_{damageType}_{iconSuffix}" : $"{record.TooltipIconTag}_{iconSuffix}";

            _factory.AddPanelToTooltip()
                .SetIcon(iconTag)
                .LocalizeName($"woundeffect.{effectKey}.desc")
                .SetValue(valueStr, true)
                .SetTextColor(isBonus ? Colors.Green : Colors.LightRed)
                .SetComparsionValue(FormatHelper.FormatValue(baseValue, record.ValueFormat));
        }

        private static string FormatDifference(float difference, bool invertColor = false, bool invertSign = false, bool addsign = true)
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

        private static T GetBaseRecord<T>(string itemId, bool isMagnumProduced, T existingRecord) where T : ItemRecord
        {
            T baseRecord = Data.Items.GetSimpleRecord<T>(itemId, true);

            if (isMagnumProduced)
            {
                T magnumRecord = Data.Items.GetSimpleRecord<T>($"{itemId}_custom", true);

                if (magnumRecord != null)
                {
                    baseRecord = magnumRecord; // Only override if found
                }
            }

            if (baseRecord == null)
            {
                _logger.LogError($"Base record not found for {itemId}");
                baseRecord = existingRecord;
            }

            return baseRecord;
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
    }
}
