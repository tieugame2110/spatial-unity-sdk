using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace SpatialSys.UnitySDK.Editor
{
    public static class EditorUtility
    {
        private const string AUTH_TOKEN_KEY = "SpatialSDK_Token";

        public const string MIN_UNITY_VERSION_STR = "2021.3.8f1";
        public const string MAX_UNITY_VERSION_STR = "2021.3.21f1";
        private static readonly Version MIN_UNITY_VERSION = GetParsedUnityVersion(MIN_UNITY_VERSION_STR);
        private static readonly Version MAX_UNITY_VERSION = GetParsedUnityVersion(MAX_UNITY_VERSION_STR);
        private static readonly Version CURRENT_UNITY_VERSION = GetParsedUnityVersion(Application.unityVersion);

        public static bool isUsingSupportedUnityVersion => CURRENT_UNITY_VERSION != null && CURRENT_UNITY_VERSION >= MIN_UNITY_VERSION && CURRENT_UNITY_VERSION <= MAX_UNITY_VERSION;
        public static bool isAuthenticated => !string.IsNullOrEmpty(GetSavedAuthToken());

        public static readonly HashSet<string> defaultTags = new HashSet<string> {
            "Untagged",
            "Respawn",
            "Finish",
            "EditorOnly",
            "MainCamera",
            "Player",
            "GameController",
        };

        public static readonly HashSet<string> strippableEditorComponents = new HashSet<string> {
            "ProBuilderMesh",
            "ProBuilderShape",
            "BezierShape",
            "PolyShape",
            "Entity",
        };

        public static Version GetParsedUnityVersion(string versionString)
        {
            try
            {
                return new Version(versionString.Replace('f', '.'));
            }
            catch
            {
                Debug.LogError($"Failed to parse Unity version string '{versionString}'; Expected format: X.X.XfX");
                return null;
            }
        }

        public static string GetSavedAuthToken()
        {
            return EditorPrefs.GetString(AUTH_TOKEN_KEY);
        }

        public static void SaveAuthToken(string token)
        {
            EditorPrefs.SetString(AUTH_TOKEN_KEY, token);
        }

        public static bool TryGetDateTimeFromEditorPrefs(string key, out DateTime result)
        {
            string dateTicks = EditorPrefs.GetString(key, defaultValue: null);
            if (long.TryParse(dateTicks, out long dateTicksLong))
            {
                result = new DateTime(dateTicksLong);
                return true;
            }

            result = DateTime.UnixEpoch;
            return false;
        }

        public static void SetDateTimeToEditorPrefs(string key, DateTime value)
        {
            EditorPrefs.SetString(key, value.Ticks.ToString());
        }

        public static bool CheckStringDistance(string a, string b, int distanceThreshold)
        {
            // Check how many characters are different between the two strings, and see if it exceeds the threshold.
            int lengthDifference = Mathf.Abs(a.Length - b.Length);

            if (lengthDifference >= distanceThreshold)
                return true;

            int minLength = Mathf.Min(a.Length, b.Length);
            int currDiff = lengthDifference;

            for (int i = 0; i < minLength; i++)
            {
                if (a[i] != b[i])
                {
                    currDiff++;

                    if (currDiff >= distanceThreshold)
                        return true;
                }
            }

            return false;
        }

        public static void OpenDocumentationPage()
        {
            Help.BrowseURL(PackageManagerUtility.documentationUrl);
        }

        public static string CreateFolderHierarchy(params string[] folders)
        {
            string path = "Assets";
            foreach (string folder in folders)
            {
                string newPath = path + $"/{folder}";

                if (!AssetDatabase.IsValidFolder(newPath))
                    AssetDatabase.CreateFolder(path, folder);

                path = newPath;
            }

            return path;
        }

        public static string GetAssetBundleName(UnityEngine.Object asset)
        {
            if (asset == null)
                return null;

            string assetPath = AssetDatabase.GetAssetPath(asset);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
                return null;

            return importer.assetBundleName;
        }

        /// <summary>
        /// Returns true if it successfully set the asset's bundle name.
        /// </summary>
        public static bool SetAssetBundleName(UnityEngine.Object asset, string bundleName)
        {
            if (asset == null)
                return false;

            string assetPath = AssetDatabase.GetAssetPath(asset);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
                return false;

            importer.assetBundleName = bundleName;
            importer.SaveAndReimport();
            return true;
        }

        public static void OpenSandboxInBrowser()
        {
            Application.OpenURL($"https://{SpatialAPI.SPATIAL_ORIGIN}/sandbox");
        }

        public static IEnumerable<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
            foreach (var t in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(t);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    yield return asset;
                }
            }
        }

        public static bool TryGetCustomAttribute<T>(this MethodInfo method, out T attribute) where T : System.Attribute
        {
            attribute = method.GetCustomAttribute<T>();
            return attribute != null;
        }

        /// <summary>
        /// Returns true if the method's parameter signature matches up with the parameter `targetTypes` list:
        /// <para>1. The amount of parameters must be the same length as `targetTypes`</para>
        /// <para>2. The order of parameters must match</para>
        /// <para>3. The type must be assignable to each type in `targetTypes`</para>
        /// </summary>
        public static bool ValidateParameterTypes(this MethodInfo method, params System.Type[] targetTypes)
        {
            if (method == null || targetTypes == null)
                return false;

            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length != targetTypes.Length)
                return false;

            for (int i = 0; i < targetTypes.Length; i++)
            {
                if (!targetTypes[i].IsAssignableFrom(parameters[i].ParameterType))
                    return false;
            }

            return true;
        }

        public static string FormatNumber(this int num)
        {
            // Add comma-separators (e.g. 1,000,000)
            return num.ToString("N0");
        }

        public static string FormatNumber(this long num)
        {
            // Add comma-separators (e.g. 1,000,000)
            return num.ToString("N0");
        }

        public static string AbbreviateNumber(int number)
        {
            if (number < 1000)
            {
                return number.ToString();
            }
            else if (number < 10000)
            {
                return (number / 1000f).ToString("0.#") + "K";
            }
            else if (number < 1000000)
            {
                return (number / 1000).ToString() + "K";
            }
            else if (number < 10000000)
            {
                return (number / 1000000f).ToString("0.#") + "M";
            }
            else
            {
                return (number / 1000000).ToString() + "M";
            }
        }

        public static string GetComponentNamesWithInstanceCountString(IEnumerable<Component> components)
        {
            IEnumerable<Type> componentTypes = components.Select(cmp => cmp.GetType());

            return string.Join(
                '\n',
                componentTypes
                    .Distinct()
                    .Select(type => {
                        int typeInstanceCount = componentTypes.Count(t => t == type);
                        return $"- {typeInstanceCount} instance(s) of {type.Name}";
                    })
            );
        }

        public static bool IsPlatformModuleInstalled(BuildTarget targetPlatform)
        {
            // Internal editor class must be accessed through reflection
            var moduleManager = Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
            MethodInfo isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", BindingFlags.Static | BindingFlags.NonPublic);
            string moduleName = (string)getTargetStringFromBuildTarget.Invoke(null, new object[] { targetPlatform });
            return (bool)isPlatformSupportLoaded.Invoke(null, new object[] { moduleName });
        }

        public static bool IsEditorOnlyType(this Type type)
        {
            return type != null && type.GetCustomAttributes(typeof(EditorOnlyAttribute), inherit: true).Length > 0;
        }

        public static bool IsStripabbleEditorOnlyType(this Type type)
        {
            return strippableEditorComponents.Contains(type.Name);
        }

        /// <summary>
        /// Removes the `target` component and any dependent components on the game object (if necessary) recursively.
        /// </summary>
        public static void RemoveComponentAndDependents(Component target)
        {
            if (target == null)
                return; // Component or script is missing. Use RemoveMonoBehavioursWithMissingScript instead.

            Type targetType = target.GetType();
            GameObject attachedGO = target.gameObject;
            bool isLastComponent = attachedGO.GetComponents(targetType).Length == 1;

            // Remove any dependent components before removing the target component.
            // We only need to do this when it's the last component instance on the object.
            if (isLastComponent)
            {
                IEnumerable<Type> otherAttachedComponentTypes = attachedGO.GetComponents<Component>()
                    .Select(comp => comp.GetType())
                    .Distinct()
                    .Where(type => type != targetType);

                foreach (Type t in otherAttachedComponentTypes)
                {
                    if (t.RequiresComponentType(targetType))
                    {
                        Component[] dependentComps = target.GetComponents(t);
                        foreach (Component c in dependentComps)
                            RemoveComponentAndDependents(c);
                    }
                }
            }

            UnityEngine.Object.DestroyImmediate(target);
        }

        /// <summary>
        /// Returns true if this type requires `componentTypeToCheck` type (via the RequireComponent attribute)
        /// </summary>
        public static bool RequiresComponentType(this Type targetComponentType, Type componentTypeToCheck)
        {
            if (targetComponentType == null || componentTypeToCheck == null)
                return false;

            IEnumerable<RequireComponent> allRequireComponentAttributes = targetComponentType.GetCustomAttributes<RequireComponent>(inherit: true);
            foreach (RequireComponent requireAttr in allRequireComponentAttributes)
            {
                if (requireAttr.m_Type0 == componentTypeToCheck || requireAttr.m_Type1 == componentTypeToCheck || requireAttr.m_Type2 == componentTypeToCheck)
                    return true;
            }

            return false;
        }
    }
}
