using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;


namespace ICVR.Settings {

    public class PresetToggleEditorWindow : EditorWindow
    {

        private ICVRSettingsData ICVRSettingsData;

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

        private void CreateGUI()
        {
            ICVRSettingsData = ICVRSettingsData.instance;
            ICVRSettingsData.Initialise(); 
        }



        private void OnGUI()
        {
            GUILayout.TextArea("IMPORTANT: \nChanges made are irreversible. \n" +
                    "When checked, use preset files to make changes:\n" +
                    "Assets/ICVR/Settings/Presets");

            foreach (var sObj in ICVRSettingsData.instance.ICVRSettings)
                {

                GUILayout.BeginHorizontal();

                GUILayout.Label(sObj.FileName);
                GUILayout.FlexibleSpace();
                 
                EditorGUI.BeginChangeCheck();

                sObj.PresetState = EditorGUILayout.Toggle(sObj.PresetState, GUILayout.Width(20));

                if (EditorGUI.EndChangeCheck())
                {
                    if (sObj.PresetState)
                    {
                        Debug.Log(sObj.FileName + " is " + sObj.PresetState); 
                        ICVRSettingsData.UpdateAsset(sObj, sObj.PresetState);
                        UpdateSettings(sObj.FilePath, IdentifyManager(sObj.FileName));
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Apply All"))
            {
                bool isConfirmed = EditorUtility.DisplayDialog("Confirm", "Are you sure you want to apply all presets?", "OK", "Cancel");

                if (isConfirmed)
                {
                    foreach (var sObj in ICVRSettingsData.instance.ICVRSettings.ToArray())
                    {
                        if (!sObj.PresetState)
                        {
                            Debug.Log(sObj.FileName + " is " + sObj.PresetState);
                            ICVRSettingsData.UpdateAsset(sObj, sObj.PresetState);
                            string manager = IdentifyManager(sObj.FileName);
                            UpdateSettings(sObj.FilePath, manager);
                        }
                    }
                }
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
                default:
                    return string.Empty;
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