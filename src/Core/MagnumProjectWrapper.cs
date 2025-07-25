using MGSC;
using System;
using System.Runtime.CompilerServices;
using static Rewired.Demos.GamepadTemplateUI.GamepadTemplateUI;

namespace QM_PathOfQuasimorph.Core
{
    public class MagnumProjectWrapper
    {
        public string Id { get; set; }
        public string CustomId { get; set; }
        public string BoostedString { get; set; }
        public ItemRarity RarityClass { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public bool PoqItem { get; set; }
        public bool SerializedStorage { get; set; }

        public MagnumProjectWrapper(MagnumProject newProject)
        {
            // Generate metadata
            this.Id = newProject.DevelopId;

            // This is our project based on time.
            this.PoqItem = IsPoqProject(newProject);

            if (PoqItem)
            {
                CustomId = $"{Id}_custom_poq";
                var digitInfo = DigitInfo.GetDigits(newProject.FinishTime.Ticks);
                RarityClass = (ItemRarity)digitInfo.Rarity;
                SerializedStorage = digitInfo.IsSerialized;
                StartTime = newProject.StartTime;
                FinishTime = newProject.FinishTime;
            }
            else
            {
                CustomId = $"{Id}_custom";
                RarityClass = ItemRarity.Standard;
                SerializedStorage = false;
                StartTime = newProject.StartTime;
                FinishTime = newProject.FinishTime;
            }
        }

        public MagnumProjectWrapper()
        {
        }

        public MagnumProjectWrapper(string id, bool poqItem, DateTime startTime, DateTime finishTime)
        {
            this.Id = id;
            this.PoqItem = poqItem;
            this.StartTime = startTime;
            this.FinishTime = finishTime;

            this.CustomId = poqItem ? $"{id}_custom_poq" : $"{id}_custom";

            var digitInfo = DigitInfo.GetDigits(finishTime.Ticks);
            this.RarityClass = (ItemRarity)digitInfo.Rarity;
            this.SerializedStorage = digitInfo.IsSerialized;
        }

        public string ReturnItemUid(bool originalId = false)
        {
            if (originalId)
            {
                return Id;
            }

            if (!PoqItem)
            {
                return CustomId;
            }

            return $"{CustomId}_{StartTime.Ticks}_{FinishTime.Ticks}";
        }

        // Avoid object creation — fast path
        public static string GetPoqItemId(MagnumProject project)
        {
            var id = project.DevelopId;
            var isPoq = IsPoqProject(project);

            if (!isPoq)
                return $"{id}_custom";

            var startTime = project.StartTime;
            var finishTime = project.FinishTime;
            return $"{id}_custom_poq_{startTime.Ticks}_{finishTime.Ticks}";
        }

        // Lightweight checkers — no object allocation

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPoqItemUid(string uid)
        {
            return uid?.Contains("_custom_poq") == true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetRarityClass(string uid, out ItemRarity rarity)
        {
            if (IsPoqItemUid(uid))
            {
                var parts = uid.Split('_');
                if (parts.Length >= 7)
                {
                    var finishStr = parts[parts.Length - 1];
                    if (long.TryParse(finishStr, out long finishTicks))
                    {
                        var digitInfo = DigitInfo.GetDigits(finishTicks);
                        rarity = (ItemRarity)digitInfo.Rarity;
                        return true;
                    }
                }
            }

            rarity = ItemRarity.Standard;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStartTime(string uid, out DateTime startTime)
        {
            startTime = default(DateTime);
            if (!IsPoqItemUid(uid)) return false;

            var parts = uid.Split('_');
            if (parts.Length >= 7)
            {
                var startStr = parts[parts.Length - 2];
                if (long.TryParse(startStr, out long ticks))
                {
                    startTime = new DateTime(ticks);
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetFinishTime(string uid, out DateTime finishTime)
        {
            finishTime = default(DateTime);
            if (!IsPoqItemUid(uid)) return false;

            var parts = uid.Split('_');
            if (parts.Length >= 6)
            {
                var finishStr = parts[parts.Length - 1];
                if (long.TryParse(finishStr, out long ticks))
                {
                    finishTime = new DateTime(ticks);
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetSerializedStorage(string uid, out bool isSerialized)
        {
            isSerialized = false;
            if (!IsPoqItemUid(uid)) return false;

            if (TryGetFinishTime(uid, out var finishTime))
            {
                var digitInfo = DigitInfo.GetDigits(finishTime.Ticks);
                isSerialized = digitInfo.IsSerialized;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetBaseId(string uid, out string baseId)
        {
            if (uid == null)
            {
                baseId = null;
                return false;
            }

            baseId = uid.Contains("_custom_poq")
                ? uid.Substring(0, uid.IndexOf("_custom_poq"))
                : uid.Replace("_custom", "");

            return !string.IsNullOrEmpty(baseId);
        }

        // Reuse existing logic safely, only instantiate when necessary
        public static MagnumProjectWrapper SplitItemUid(string uid)
        {
            if (string.IsNullOrEmpty(uid)) return null;

            if (IsPoqItemUid(uid))
            {
                var idx = uid.IndexOf("_custom_poq");
                if (idx <= 0) return null;

                var realId = uid.Substring(0, idx);
                var suffix = uid.Substring(idx + "_custom_poq_".Length);
                var suffixParts = suffix.Split('_');

                if (suffixParts.Length >= 2 &&
                    long.TryParse(suffixParts[0], out long startTicks) &&
                    long.TryParse(suffixParts[1], out long finishTicks))
                {
                    return new MagnumProjectWrapper(
                        id: realId,
                        poqItem: true,
                        startTime: new DateTime(startTicks),
                        finishTime: new DateTime(finishTicks)
                    );
                }
            }

            // Handle non-PoQ item
            var baseId = uid.Replace("_custom", "");
            return new MagnumProjectWrapper
            {
                Id = baseId,
                CustomId = uid,
                RarityClass = ItemRarity.Standard,
                StartTime = DateTime.MinValue,
                FinishTime = DateTime.MinValue,
                PoqItem = false,
                SerializedStorage = false
            };
        }

        // Static helper to detect project type without allocation
        public static bool IsPoqProject(MagnumProject project)
        {
            // We can actually have rarity of any kind
            //return DigitInfo.GetDigits(project.FinishTime.Ticks).Rarity > 0;
            if (project.StartTime.Ticks == MagnumPoQProjectsController.MAGNUM_PROJECT_START_TIME && project.FinishTime.Ticks > 0)
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemRarity GetRarityClass(string uid)
        {
            if (TryGetFinishTime(uid, out var finishTime))
            {
                return DigitInfo.GetRarityClass(finishTime.Ticks);
            }
            return ItemRarity.Standard;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSerializedStorage(string uid)
        {
            return TryGetFinishTime(uid, out var finish) &&
                   DigitInfo.IsSerializedStorage(finish.Ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSerializedStorage(long ticks)
        {
            return DigitInfo.IsSerializedStorage(ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBoostedParam(string uid)
        {
            return TryGetFinishTime(uid, out var finish)
                ? DigitInfo.GetBoostedParam(finish.Ticks)
                : 0;
        }
    }
}
