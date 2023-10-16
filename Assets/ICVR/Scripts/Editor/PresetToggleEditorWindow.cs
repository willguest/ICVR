#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using WebXR;
using static WebXR.WebXRSettings;

namespace ICVR.Settings {

    public class PresetToggleEditorWindow : EditorWindow
    {
        private ICVRSettingsData ICVRSettingsData;
        private ICVRProjectSettings ICVRProjectSettings;

        private static string PRESET_PATH = "Assets/ICVR/Settings/Presets";

        private static string LIGHT_DATA_ASSET = "Assets/ICVR/Settings/Presets/ICVR_LightingSettings.preset";
        private static string LIGHT_SETTINGS = "Assets/ICVR/Settings/Presets/ICVR_Lighting.lighting";

        private static string TAG_MNGR_ASSET = "ProjectSettings/TagManager.asset";
        private static string PHYS_MNGR_ASSET = "ProjectSettings/DynamicsManager.asset";
        private static string GRAPH_MNGR_ASSET = "ProjectSettings/GraphicsSettings.asset";
        private static string QUAL_MNGR_ASSET = "ProjectSettings/QualitySettings.asset";
        private static string PLAY_MNGR_ASSET = "ProjectSettings/ProjectSettings.asset";
        private static string EDTR_MNGR_ASSET = "ProjectSettings/EditorSettings.asset";


        [MenuItem("Window/WebXR/ICVR Settings")]
        public static void ShowWindow()
        {
            GetWindow<PresetToggleEditorWindow>("ICVR Settings");
        }

        static WebXRSettings WebXRSettings;
        private Dictionary<string, bool> packageStatus;
        KeyValuePair<string, bool>[] presets;
        bool hasDataAsset = false;

        string[] presetFiles;
        bool[] presetStates;


        private void CreateGUI()
        {
            // Create settings objects
            ICVRProjectSettings = CreateInstance<ICVRProjectSettings>();
            ICVRProjectSettings.GetProjectPackages();
            packageStatus = new Dictionary<string, bool>();

            // Get dependencies
            CheckPackagePresence("com.de-panther.webxr");
            CheckPackagePresence("com.unity.nuget.newtonsoft-json");

            // Start the settings scriptable object
            ICVRSettingsData = ICVRSettingsData.instance;
            hasDataAsset = ICVRSettingsData.Initialise();

            // Get the properties of the .preset files
            presetFiles = Directory.GetFiles(PRESET_PATH, "*.preset");
            presetStates = new bool[presetFiles.Length];
            presets = new KeyValuePair<string, bool>[presetFiles.Length];

            if (hasDataAsset) 
            {
                for (int f = 0; f < presetFiles.Length; f++)
                {
                    var assetData = ICVRSettingsData.ICVRSettings[f];
                    if (assetData.FilePath == presetFiles[f])
                    {
                        //Debug.Log("adding item from data asset");
                        presets[f] = new KeyValuePair<string, bool>(assetData.FileName, assetData.PresetState);
                    }
                    else
                    {
                        //Debug.Log("adding new item");
                        presets[f] = new KeyValuePair<string, bool>(presetFiles[f], false);
                    }
                }   
            }
        }


        private void OnGUI()
        {
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.normal.textColor = Color.white;
            titleStyle.fontStyle = FontStyle.Bold;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Packages", titleStyle);

            // Display the packages and their install buttons
            DisplayPackage("WebXR Export", "com.de-panther.webxr");
            DisplayPackage("Newtonsoft Json","com.unity.nuget.newtonsoft-json");
            
            EditorGUILayout.EndVertical(); 
            EditorGUILayout.Space(15, true);

            GUILayout.TextArea("IMPORTANT: Changes to settings are irreversible. " +
                    "When checked, use preset files to change project settings. \n" +
                    "-> " + PRESET_PATH);

            EditorGUILayout.Separator();
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
                    ICVRSettingsData.ModifyDataAsset(filename, presetStates[i]);

                    if (presetStates[i]) // turn on
                    {
                        string filepath = presetFiles[i];
                        string manager = IdentifyManager(filename);
                        if (!string.IsNullOrEmpty(manager))
                        {
                            UpdateSettings(filepath, manager);
                        }
                    }
                    else // turn off
                    {

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

                            //Debug.Log(filename + " is now " + presetStates[i]);

                            ICVRSettingsData.ModifyDataAsset(filename, presetStates[i]);
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
                ICVRProjectSettings.IncludePackage(packageName);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void CheckPackagePresence(string packageName)
        {
            bool isPresent = ICVRProjectSettings.QueryPackageStatus(packageName);
            packageStatus.Add(packageName, isPresent);
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
                case "ICVR_WebXR":
                    TryUpdateWebXrSettings();
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
                Debug.Log("Lighting Error:\n" + e.ToString()); 
            };

            lightManager.ApplyModifiedProperties();
            lightManager.UpdateIfRequiredOrScript();
        }

        public static void TryUpdateWebXrSettings()
        {
            if (EditorBuildSettings.TryGetConfigObject("WebXR.Settings", out WebXRSettings))
            {
                WebXRSettings = EditorUtility.InstanceIDToObject(WebXRSettings.GetInstanceID()) as WebXRSettings;
                WebXRSettings.VRRequiredReferenceSpace = ReferenceSpaceTypes.local;
                WebXRSettings.VROptionalFeatures = 0;
            }
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