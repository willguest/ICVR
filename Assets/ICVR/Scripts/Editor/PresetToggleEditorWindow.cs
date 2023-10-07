using System.IO;
using UnityEditor;
using UnityEngine;


namespace ICVR.Settings {

    public class PresetToggleEditorWindow : EditorWindow
    {
        public delegate void SettingsChanged(string presetName);
        public event SettingsChanged OnSettingsChanged;


        // This list will hold the names of the .preset files
        private string[] presetFiles;

        // This list will hold the state (on/off) of each .preset file
        private bool[] presetStates;

        [MenuItem("Window/Preset Toggle")]
        public static void ShowWindow()
        {
            GetWindow<PresetToggleEditorWindow>("Preset Toggle");
        }

        private void OnEnable()
        {
            // Get the path to the folder containing the .preset files
            string folderPath = "Assets/ICVR/Settings/Presets";

            // Get the names of the .preset files in the folder
            presetFiles = Directory.GetFiles(folderPath, "*.preset");

            // Initialize the presetStates list with the same length as presetFiles
            presetStates = new bool[presetFiles.Length];

            // Set the default state of each preset to false (off)
            for (int i = 0; i < presetStates.Length; i++)
            {
                presetStates[i] = false;
            }
        }

        private void OnGUI()
        {
            // Iterate over each .preset file
            for (int i = 0; i < presetFiles.Length; i++)
            {

                GUILayout.BeginHorizontal();

                // Display the name of the .preset file
                GUILayout.Label(Path.GetFileNameWithoutExtension(presetFiles[i]));
                GUILayout.FlexibleSpace();

                EditorGUI.BeginChangeCheck();
                presetStates[i] = EditorGUILayout.Toggle(presetStates[i], GUILayout.Width(25));

                if (EditorGUI.EndChangeCheck())
                {
                    Debug.Log(Path.GetFileNameWithoutExtension(presetFiles[i]) + " is " + presetStates[i].ToString());
                    OnSettingsChanged.Invoke(Path.GetFileNameWithoutExtension(presetFiles[i]));
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}