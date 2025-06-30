using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {

        public static void UpdateKey(string lookupstr, string prefix, string suffix)
        {
            foreach (KeyValuePair<Localization.Lang, Dictionary<string, string>> languageToDict in Singleton<Localization>.Instance.db)
            {
                if (languageToDict.Value.ContainsKey(lookupstr))
                {
                    languageToDict.Value[lookupstr] = prefix + languageToDict.Value[lookupstr] + suffix;
                }
            }
        }

        // Other mods compatibility issue.
        // We created our own method using original one to avoid this.
        public static void InjectItemRecord(MagnumProject project)
        {
            // Default method copied here.
            // Plugin.Logger.Log($"\t MagnumDevelopmentSystem_InjectItemRecord_Patch Start");
            //Plugin.Logger.Log($"InjectItemRecord :: Prefix :: Start");

            string text = project.DevelopId + "_custom";
            text = magnumProjectsController.WrapProjectDateTime(project);
            //string text = project.DevelopId + "_custom";
            //Plugin.Logger.Log($"\t _InjectItemRecord project.DevelopId {project.DevelopId}");
            //Plugin.Logger.Log($"\t _InjectItemRecord text {text}");

            Localization.DuplicateKey("item." + project.DevelopId + ".name", "item." + text + ".name");
            Localization.DuplicateKey("item." + project.DevelopId + ".shortdesc", "item." + text + ".shortdesc");

            //TEST

            DigitInfo digits = DigitInfo.GetDigits(project.FinishTime.Ticks);
            var rarstr = (ItemRarity)digits.D6_Rarity;

            UpdateKey("item." + text + ".name", "Mod: ", $" {rarstr.ToString()}");
            //UpdateKey("item." + text + ".shortdesc", "Power", "of Doom");


            CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(project.DevelopId, true) as CompositeItemRecord;

            // PathOfQuasimorph ADD Start
            //Plugin.Logger.Log($"\t compositeItemRecord == null {compositeItemRecord == null}");
            // PathOfQuasimorph ADD End

            Data.Items.RemoveRecord(text);
            Data.ItemTransformation.RemoveRecord(text);
            ItemTransformationRecord record = Data.ItemTransformation.GetRecord(project.DevelopId, true);

            // PathOfQuasimorph ADD Start
            //Plugin.Logger.Log($"\t ItemTransformationRecord == null {record == null}");
            if (record == null)
            {
                //Plugin.Logger.Log($"\t ItemTransformationRecord null. Need add placeholder.");
                record = Data.ItemTransformation.GetRecord("broken_weapon", true); //Rusty Parts //We won't use it anyway. Don't care.
                //Plugin.Logger.Log($"\t ItemTransformationRecord text: {text}");
                //Plugin.Logger.Log($"\t ItemTransformationRecord record: {record}");
                //Plugin.Logger.Log($"\t ItemTransformationRecord record null?: {record == null}");
            }
            // PathOfQuasimorph ADD End

            Data.ItemTransformation.AddRecord(text, record.Clone(text));
            foreach (BasePickupItemRecord basePickupItemRecord in compositeItemRecord.Records)
            {
                WeaponRecord weaponRecord = basePickupItemRecord as WeaponRecord;
                if (weaponRecord != null)
                {
                    WeaponRecord weaponRecord2 = weaponRecord.Clone(text);
                    Data.Items.AddRecord(text, weaponRecord2);
                    project.ApplyModifications(weaponRecord2);
                }
                else
                {
                    ArmorRecord armorRecord = basePickupItemRecord as ArmorRecord;
                    if (armorRecord != null)
                    {
                        ArmorRecord armorRecord2 = armorRecord.Clone(text);
                        Data.Items.AddRecord(text, armorRecord2);
                        project.ApplyModifications(armorRecord2);
                    }
                    else
                    {
                        HelmetRecord helmetRecord = basePickupItemRecord as HelmetRecord;
                        if (helmetRecord != null)
                        {
                            HelmetRecord helmetRecord2 = helmetRecord.Clone(text);
                            Data.Items.AddRecord(text, helmetRecord2);
                            project.ApplyModifications(helmetRecord2);
                        }
                        else
                        {
                            LeggingsRecord leggingsRecord = basePickupItemRecord as LeggingsRecord;
                            if (leggingsRecord != null)
                            {
                                LeggingsRecord leggingsRecord2 = leggingsRecord.Clone(text);
                                Data.Items.AddRecord(text, leggingsRecord2);
                                project.ApplyModifications(leggingsRecord2);
                            }
                            else
                            {
                                BootsRecord bootsRecord = basePickupItemRecord as BootsRecord;
                                if (bootsRecord != null)
                                {
                                    Plugin.Logger.Log($"\t bootsRecord2");
                                    BootsRecord bootsRecord2 = bootsRecord.Clone(text);
                                    Data.Items.AddRecord(text, bootsRecord2);
                                    project.ApplyModifications(bootsRecord2);
                                }
                                else
                                {
                                    throw new NotImplementedException(string.Format("Failed create project {0}. No clone method for additional records: {1}.", project.DevelopId, basePickupItemRecord.GetType()));
                                }

                            }
                        }
                    }
                }
            }
        }

        public static void InjectProjectRecords(MagnumProjects projects)
        {
            Plugin.Logger.Log($" InjectProjectRecords : Postfix");

            foreach (MagnumProject magnumProject in projects.Values)
            {
                InjectProjectRecord(magnumProject);
            }
        }

        public static void InjectProjectRecord(MagnumProject project)
        {
            Plugin.Logger.Log($" InjectProjectRecord : Postfix");

            switch (project.ProjectType)
            {
                case MagnumProjectType.RangeWeapon:
                case MagnumProjectType.MeleeWeapon:
                case MagnumProjectType.Armor:
                case MagnumProjectType.Helmet:
                case MagnumProjectType.Boots:
                case MagnumProjectType.Leggings:
                    if (project.StartTime == DateTime.MinValue)
                    {
                        InjectItemRecord(project);
                    }
                    return;
                default:
                    return;
            }
        }




        [Hook(ModHookType.DungeonStarted)]
        public static void DungeonStarted(IModContext context)
        {
        }

        [Hook(ModHookType.DungeonFinished)]
        public static void DungeonFinished(IModContext context)
        {
            isInitialized = false;
        }

        [Hook(ModHookType.SpaceStarted)]
        public static void SpaceStarted(IModContext context)
        {
        }

        [Hook(ModHookType.SpaceFinished)]
        public static void SpaceFinished(IModContext context)
        {
            isInitialized = false;
        }


        [Hook(ModHookType.SpaceUpdateBeforeGameLoop)]
        public static void SpaceUpdateBeforeGameLoop(IModContext context)
        {
            if (!isInitialized)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Log($"Error in ColorTuner initialization: {ex.Message}");
                }
            }
        }

        [Hook(ModHookType.DungeonUpdateBeforeGameLoop)]
        public static void DungeonUpdateBeforeGameLoop(IModContext context)
        {
            if (!isInitialized)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Log($"Error in ColorTuner initialization: {ex.Message}");
                }
            }
        }








    }
}
