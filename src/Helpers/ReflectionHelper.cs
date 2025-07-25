using ModConfigMenu;
using System;
using System.Linq;
using System.Reflection;

namespace QM_PathOfQuasimorph.PoQHelpers
{
    public static class ReflectionHelper
    {
        public static T ShallowClone<T>(T original) where T : class, new()
        {
            Type type = typeof(T);
            T clone = new T();

            FieldInfo[] fields = type.GetFields(BindingFlags.Public |
                                               BindingFlags.NonPublic |
                                               BindingFlags.Instance |
                                               BindingFlags.FlattenHierarchy);

            foreach (FieldInfo field in fields)
            {
                Plugin.Logger.Log($"field {field.Name} ");
                field.SetValue(clone, field.GetValue(original));
            }

            return clone;
        }

        public static T CloneViaProperties<T>(T original) where T : class, new()
        {
            if (original == null) return null;

            Type type = typeof(T);
            T clone = new T();

            // Get all public instance properties, including inherited ones
            PropertyInfo[] properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .ToArray();

            foreach (PropertyInfo prop in properties)
            {
                try
                {
                    object value = prop.GetValue(original);
                    prop.SetValue(clone, value);
                }
                catch (Exception ex)
                {
                    // Some props may throw (e.g. during get), just skip
                    UnityEngine.Debug.LogWarning($"[QM] Failed to copy property {prop.Name}: {ex.Message}");
                }
            }

            return clone;
        }
    }
}