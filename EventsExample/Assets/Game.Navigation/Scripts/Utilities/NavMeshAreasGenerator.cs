using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace UnityEngine.AI
{
    #region Auto-Generated Content

    [Flags]
    public enum NavMeshAreas
    {
        None = 0,
        Walkable = 1,
        NotWalkable = 2,
        Jump = 4,
        All = ~0
    }

    #endregion

#if UNITY_EDITOR

    /// <summary>
    ///     Auto-updates the <see cref="NavMeshAreas" /> enum in this file if it has changed, when scripts are compiled or
    ///     assets saved.
    /// </summary>
    public static class NavMeshAreasGenerator
    {
        private const string EnumValuesToken = "#EnumValues";
        private const string HashSettingsKey = "NavMeshAreasHash";

        private static readonly string ContentTemplate = $@"        public enum {nameof(NavMeshAreas)}
        {{
            None = 0,               
            {EnumValuesToken}
            All = ~0,
        }}
        ";

        private static void Update([CallerFilePath] string executingFilePath = "")
        {
            var areaNames = GameObjectUtility.GetNavMeshAreaNames();
            var lastHash = EditorPrefs.GetInt(HashSettingsKey);
            var newHash = GetAreaHash(areaNames);

            if (newHash != lastHash)
            {
                Debug.Log($"{nameof(NavMeshAreas)} have changed, updating enum: '{executingFilePath}'");
                GenerateFile(areaNames, newHash, executingFilePath);
            }
        }

        private static int GetAreaHash(string[] areaNames)
        {
            var input = areaNames.Aggregate((a, b) => a + b);
            var hash = 0;
            foreach (var t in input)
                hash = (hash << 5) + hash + t;
            return hash;
        }

        private static void GenerateFile(string[] areaNames = default, int hash = 0, string outputPath = null)
        {
            if (areaNames == null)
                areaNames = GameObjectUtility.GetNavMeshAreaNames();

            if (hash == 0)
                hash = GetAreaHash(areaNames);

            var values = GetAreaEnumValuesAsText(ref areaNames);
            var newEnumText = ContentTemplate.Replace(EnumValuesToken, values);
            var output = ReplaceEnumInFile(nameof(NavMeshAreas), File.ReadAllLines(outputPath), newEnumText);

            CreateScriptAssetWithContent(outputPath, string.Concat(output));
            EditorPrefs.SetInt(HashSettingsKey, hash);
            AssetDatabase.Refresh();
        }

        private static string GetAreaEnumValuesAsText(ref string[] areaNames)
        {
            var increment = 0;
            var output = new StringBuilder();
            var seenKeys = new HashSet<string>();

            foreach (var name in areaNames)
            {
                var enumKey = string.Concat(name.Where(char.IsLetterOrDigit));
                var value = 1 << NavMesh.GetAreaFromName(name);

                output.Append(seenKeys.Contains(name) ? $"{enumKey + increment++} = {value}, " : $"{enumKey} = {value}, ");

                seenKeys.Add(enumKey);
            }

            return output.ToString();
        }

        private static string ReplaceEnumInFile(string enumName, string[] fileLines, string newEnum)
        {
            int enumStartLine = 0, enumEndLine = 0;
            var result = new StringBuilder();
            for (var i = 0; i < fileLines.Length; i++)
            {
                var line = fileLines[i];
                if (line.Trim().StartsWith("public enum " + enumName))
                {
                    enumStartLine = i;
                    break;
                }

                result.AppendLine(line);
            }

            if (enumStartLine > 0)
            {
                for (var i = enumStartLine + 1; i < fileLines.Length; i++)
                    if (fileLines[i].Contains("}"))
                    {
                        enumEndLine = i;
                        break;
                    }

                result.Append(newEnum);
                for (var i = enumEndLine + 1; i < fileLines.Length; i++)
                    result.AppendLine(fileLines[i]);
            }

            return result.ToString();
        }

        /// <summary>
        ///     Create a new script asset.
        ///     UnityEditor.ProjectWindowUtil.CreateScriptAssetWithContent (2019.1)
        /// </summary>
        /// <param name="pathName">the path to where the new file should be created</param>
        /// <param name="templateContent">the text to put inside</param>
        /// <returns></returns>
        private static Object CreateScriptAssetWithContent(string pathName, string templateContent)
        {
            templateContent = SetLineEndings(templateContent, EditorSettings.lineEndingsForNewScripts);
            var fullPath = Path.GetFullPath(pathName);
            var encoding = new UTF8Encoding(true);
            File.WriteAllText(fullPath, templateContent, encoding);
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }

        /// <summary>
        ///     Ensure correct OS specific line endings for saving file content.
        ///     UnityEditor.ProjectWindowUtil.SetLineEndings (2019.1)
        /// </summary>
        /// <param name="content">a string to have line endings checked</param>
        /// <param name="lineEndingsMode">the type of line endings to use</param>
        /// <returns>a cleaned string</returns>
        private static string SetLineEndings(string content, LineEndingsMode lineEndingsMode)
        {
            string replacement;
            switch (lineEndingsMode)
            {
                case LineEndingsMode.OSNative:
                    replacement = Application.platform == RuntimePlatform.WindowsEditor ? "\r\n" : "\n";
                    break;
                case LineEndingsMode.Unix:
                    replacement = "\n";
                    break;
                case LineEndingsMode.Windows:
                    replacement = "\r\n";
                    break;
                default:
                    replacement = "\n";
                    break;
            }

            content = Regex.Replace(content, "\\r\\n?|\\n", replacement);
            return content;
        }

        /// <summary>
        ///     Hook that runs the enum generator whenever scripts are compiled.
        /// </summary>
        [DidReloadScripts]
        private static void UpdateOnScriptCompile()
        {
            Update();
        }

        /// <summary>
        ///     Enables manually running the enum generator from the menus.
        /// </summary>
        [MenuItem("Tools/Update NavMeshAreas")]
        private static void UpdateOnMenuCommand()
        {
            UpdateOnScriptCompile();
        }

        /// <summary>
        ///     Hook that runs the enum generator whenever assets are saved.
        /// </summary>
        private class UpdateOnAssetModification : UnityEditor.AssetModificationProcessor
        {
            public static string[] OnWillSaveAssets(string[] paths)
            {
                Update();
                return paths;
            }
        }
    }

    /// <summary>
    ///     Flags enum dropdown GUI for selecting <see cref="NavMeshAreas" /> properties in the inspector
    /// </summary>
    [CustomPropertyDrawer(typeof(NavMeshAreas))]
    public class NavMeshAreasDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            var oldValue = (Enum) fieldInfo.GetValue(property.serializedObject.targetObject);
            var newValue = EditorGUI.EnumFlagsField(position, label, oldValue);
            if (!newValue.Equals(oldValue))
                property.intValue = (int) Convert.ChangeType(newValue, fieldInfo.FieldType);
            EditorGUI.EndProperty();
        }
    }

#endif

    /// <summary>
    ///     A helper for flag operations with NavMeshAreas
    /// </summary>
    public struct AreaMask
    {
        public int Value { get; }

        public NavMeshAreas EnumValue => (NavMeshAreas) Value;

        public AreaMask(int value)
        {
            Value = value;
        }

        public AreaMask(NavMeshAreas areas)
        {
            Value = (int) areas;
        }

        public static implicit operator AreaMask(int value)
        {
            return new AreaMask(value);
        }

        public static implicit operator AreaMask(string name)
        {
            return new AreaMask(1 << NavMesh.GetAreaFromName(name));
        }

        public static implicit operator AreaMask(NavMeshAreas areas)
        {
            return new AreaMask((int) areas);
        }

        public static implicit operator NavMeshAreas(AreaMask flag)
        {
            return (NavMeshAreas) flag.Value;
        }

        public static implicit operator int(AreaMask flag)
        {
            return flag.Value;
        }

        public static bool operator ==(AreaMask a, int b)
        {
            return a.Value.Equals(b);
        }

        public static bool operator !=(AreaMask a, int b)
        {
            return !a.Value.Equals(b);
        }

        public static int operator +(AreaMask a, AreaMask b)
        {
            return a.Add(b.Value);
        }

        public static int operator -(AreaMask a, AreaMask b)
        {
            return a.Remove(b.Value);
        }

        public static int operator |(AreaMask a, AreaMask b)
        {
            return a.Add(b.Value);
        }

        public static int operator ~(AreaMask a)
        {
            return ~a.Value;
        }

        public static int operator +(int a, AreaMask b)
        {
            return a |= b.Value;
        }

        public static int operator -(int a, AreaMask b)
        {
            return a &= ~b.Value;
        }

        public static int operator |(int a, AreaMask b)
        {
            return a |= b.Value;
        }

        public static int operator +(AreaMask a, int b)
        {
            return a.Add(b);
        }

        public static int operator -(AreaMask a, int b)
        {
            return a.Remove(b);
        }

        public static int operator |(AreaMask a, int b)
        {
            return a.Add(b);
        }

        public bool HasFlag(AreaMask flag)
        {
            return (Value & flag.Value) == flag;
        }

        public bool HasFlag(int value)
        {
            return (Value & value) == value;
        }

        public AreaMask Add(AreaMask flag)
        {
            return Value | flag.Value;
        }

        public AreaMask Remove(AreaMask flag)
        {
            return Value & ~flag.Value;
        }

        public AreaMask Add(NavMeshAreas flags)
        {
            return Value | (int) flags;
        }

        public AreaMask Remove(NavMeshAreas flags)
        {
            return Value & ~(int) flags;
        }

        public bool Equals(AreaMask other)
        {
            return Value == other.Value;
        }

        public override string ToString()
        {
            return ((NavMeshAreas) Value).ToString();
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && obj is AreaMask other && Equals(other);
        }

        private static Array _areas;

        public static int GetIndex(NavMeshAreas area)
        {
            return Array.IndexOf(_areas ?? (_areas = Enum.GetValues(typeof(NavMeshAreas))), area) - 1;
        }
    }
}