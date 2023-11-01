#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace ICVR.Settings
{
    public class ICVRSettingsWindow : EditorWindow
    {
        private ICVRSettingsData settingsData;
        private ICVRProjectSettings projectSettings;

        private static string PRESET_PATH = "Assets/ICVR/Settings/Presets";

        private static string LIGHT_DATA_ASSET = "Assets/ICVR/Settings/Presets/ICVR_LightingSettings.preset";
        private static string LIGHT_SETTINGS = "Assets/ICVR/Settings/Presets/ICVR_Lighting.lighting";

        private static string TAG_MNGR_ASSET = "ProjectSettings/TagManager.asset";
        private static string PHYS_MNGR_ASSET = "ProjectSettings/DynamicsManager.asset";
        private static string GRAPH_MNGR_ASSET = "ProjectSettings/GraphicsSettings.asset";
        private static string QUAL_MNGR_ASSET = "ProjectSettings/QualitySettings.asset";
        private static string PLAY_MNGR_ASSET = "ProjectSettings/ProjectSettings.asset";
        private static string EDTR_MNGR_ASSET = "ProjectSettings/EditorSettings.asset";


        [MenuItem("Window/WebXR/ICVR Setup")]
        public static void ShowWindow()
        {
            GetWindow<ICVRSettingsWindow>("ICVR Setup");
        }

        private Dictionary<string, bool> packageStatus;
        private KeyValuePair<string, bool>[] presets;

        private bool isWebGL;
        private bool hasDataAsset = false;
        private bool hasDependencies = false;

        private string[] presetFiles;
        private bool[] presetStates;

        private void CreateGUI()
        {
            isWebGL = EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL;
            packageStatus = new Dictionary<string, bool>();

            // Create settings objects
            projectSettings = CreateInstance<ICVRProjectSettings>();
            projectSettings.GetProjectPackages();

            // Check dependency status
            hasDependencies = HasDependencies(false);

            // Get the properties of the .preset files
            presetFiles = Directory.GetFiles(PRESET_PATH, "*.preset");
            presetStates = new bool[presetFiles.Length];
            presets = new KeyValuePair<string, bool>[presetFiles.Length];

            // Start the settings data object
            settingsData = ICVRSettingsData.instance;
            hasDataAsset = settingsData.Initialise(presetFiles.Length);

            if (hasDataAsset)
            {
                for (int f = 0; f < presetFiles.Length; f++)
                {
                    var assetData = settingsData.ICVRSettings[f];
                    if (assetData.FilePath == presetFiles[f])
                    {
                        presets[f] = new KeyValuePair<string, bool>(assetData.FileName, assetData.PresetState);
                    }
                    else
                    {
                        presets[f] = new KeyValuePair<string, bool>(presetFiles[f], false);
                    }
                }
            }
        }

        private bool HasDependencies(bool useAsync = true)
        {
            bool one = CheckPackagePresence("com.unity.nuget.newtonsoft-json", useAsync);
            bool two = CheckPackagePresence("com.de-panther.webxr", useAsync);
            return (one && two);
        }
        private void OnGUI()
        {
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.normal.textColor = Color.white;
            titleStyle.fontStyle = FontStyle.Bold;

            var refStyle = new GUIStyle(GUI.skin.label);
            refStyle.normal.textColor = Color.white;
            refStyle.fontStyle = FontStyle.Bold;

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Build Target", titleStyle);
            EditorGUI.BeginDisabledGroup(isWebGL);
            if (GUILayout.Button("Set WebGL"))
            {
                EnsureWebGLBuildTarget();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(15, true);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Packages", titleStyle, GUILayout.Width(80));
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(hasDependencies);
            if (GUILayout.Button("↻", refStyle, GUILayout.Width(20)))
            {
                hasDependencies = HasDependencies();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            DisplayPackage("Newtonsoft Json", "com.unity.nuget.newtonsoft-json");
            DisplayPackage("WebXR Export", "com.de-panther.webxr");

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15, true);

            GUILayout.TextArea("IMPORTANT: Changes to settings are irreversible. " +
                        "When checked, preset files control relevant project settings. \n" +
                        "-> " + PRESET_PATH);

            EditorGUI.BeginDisabledGroup(!hasDependencies || SDSUtility.ContainsSymbol(BuildTargetGroup.WebGL, "ICVR"));
            if (GUILayout.Button("Enable ICVR Settings"))
            {
                SDSUtility.AddSymbol(BuildTargetGroup.WebGL, "ICVR");
            }
            EditorGUI.EndDisabledGroup();

            // disable settings until deps are in
            EditorGUI.BeginDisabledGroup(!SDSUtility.ContainsSymbol(BuildTargetGroup.WebGL, "ICVR"));

            EditorGUILayout.Space(15, true);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Settings", titleStyle);

            if (presets == null) return;

            // Set the default state of each preset to false (off)
            for (int i = 0; i < presets.Length; i++)
            {
                string filename = Path.GetFileNameWithoutExtension(presetFiles[i]);
                string shortName = filename.Substring(5, filename.Length - 5);
                presetStates[i] = presets[i].Value;

                GUILayout.BeginHorizontal();

                GUILayout.Label(shortName);
                GUILayout.FlexibleSpace();

                EditorGUI.BeginChangeCheck();

                presetStates[i] = EditorGUILayout.Toggle(presets[i].Value, GUILayout.Width(20));

                if (EditorGUI.EndChangeCheck())
                {
                    presets.SetValue(new KeyValuePair<string, bool>(filename, presetStates[i]), i);
                    settingsData.ModifyDataAsset(presets.Length, filename, presetStates[i]);

                    if (presetStates[i]) // turn on
                    {
                        // Update settings
                        string filepath = presetFiles[i]; 
                        string manager = IdentifyManager(filename);
                        if (!string.IsNullOrEmpty(manager))
                        {
                            UpdateSettings(filepath, manager);
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Apply All"))
            {
                bool isConfirmed = EditorUtility.DisplayDialog("Confirmation",
                    "Are you sure you want to apply all presets?", "OK", "Cancel");

                if (isConfirmed)
                {
                    for (int i = 0; i < presets.Length; i++)
                    {
                        if (!presetStates[i])
                        {
                            string filename = Path.GetFileNameWithoutExtension(presetFiles[i]);
                            presetStates[i] = true;

                            presets.SetValue(new KeyValuePair<string, bool>(filename, true), i);
                            settingsData.ModifyDataAsset(presets.Length, filename, presetStates[i]);

                            string filepath = presetFiles[i];
                            string manager = IdentifyManager(filename);
                            if (!string.IsNullOrEmpty(manager))
                            {
                                UpdateSettings(filepath, manager);
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUI.EndDisabledGroup();
        }

        void DisplayPackage(string label, string packageName)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(100));
            GUILayout.FlexibleSpace();

            if (packageStatus == null)
            {
                packageStatus = new Dictionary<string, bool>();
            }

            if (packageStatus.TryGetValue(packageName, out bool isPresent))
            {
                if (packageStatus[packageName])
                {
                    var greenStyle = new GUIStyle(GUI.skin.label);
                    greenStyle.normal.textColor = Color.green;
                    EditorGUILayout.LabelField("✔", greenStyle, GUILayout.MaxWidth(20));
                }
            }

            EditorGUI.BeginDisabledGroup(isPresent);
            if (GUILayout.Button("Install", GUILayout.Width(50)))
            {
                projectSettings.TryIncludePackage(packageName);
                hasDependencies = HasDependencies(false);

            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private bool CheckPackagePresence(string packageName, bool useAsync = true)
        {
            bool isPresent = useAsync ?
                projectSettings.CheckForPackage(packageName) :
                projectSettings.CheckPackSync(packageName);

            if (packageStatus.ContainsKey(packageName))
            {
                packageStatus[packageName] = isPresent;
            }
            else
            {
                packageStatus.Add(packageName, isPresent);
            }
            return isPresent;
        }

        public static void EnsureWebGLBuildTarget()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }
        }

        private string IdentifyManager(string presetName)
        {
            switch (presetName)
            {
                case "ICVR_Tags":
                    return TAG_MNGR_ASSET;
                case "ICVR_Physics":
                    return PHYS_MNGR_ASSET;
                case "ICVR_Graphics":
                    return GRAPH_MNGR_ASSET;
                case "ICVR_Quality":
                    return QUAL_MNGR_ASSET;
                case "ICVR_PlayerSettings":
                    return PLAY_MNGR_ASSET;
                case "ICVR_EditorSettings":
                    return EDTR_MNGR_ASSET;
                case "ICVR_LightingSettings":
                    UpdateLighting();
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        private void UpdateLighting()
        {
            var lightingDataAsset = AssetDatabase.LoadMainAssetAtPath(LIGHT_DATA_ASSET) as Preset;
            LightingSettings lightingSettings = AssetDatabase.LoadMainAssetAtPath(LIGHT_SETTINGS) as LightingSettings;
            SerializedObject lightManager = new SerializedObject(lightingSettings);

            try
            {
                lightingDataAsset.ApplyTo(lightManager.targetObject);

                Lightmapping.SetLightingSettingsForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), lightingSettings);
            }
            catch (Exception e)
            {
                Debug.Log("Lighting Error: " + e.ToString());
            };

            lightManager.ApplyModifiedProperties();
            lightManager.UpdateIfRequiredOrScript();
            Lightmapping.BakeAsync();
        }

        private static void UpdateSettings(string preset, string manager)
        {
            Preset settingsPreset = AssetDatabase.LoadMainAssetAtPath(preset) as Preset;
            SerializedObject settingsManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath(manager));
            settingsPreset.ApplyTo(settingsManager.targetObject);
            settingsManager.ApplyModifiedProperties();
            settingsManager.Update();
        }

    }
}
#endif