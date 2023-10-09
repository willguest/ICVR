#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace ICVR.Settings
{

    [System.Serializable]
    public class ICVRSettingsObject
    {
        public string Id;
        public string FileName = string.Empty;
        public string FilePath = string.Empty;
        public bool PresetState = false;
    }

    [System.Serializable, FilePath("Assets/ICVR/Settings/ICVRSettingsData.asset", FilePathAttribute.Location.ProjectFolder)]
    public class ICVRSettingsData : ScriptableSingleton<ICVRSettingsData>
    {
        [SerializeField]
        public List<ICVRSettingsObject> ICVRSettings;


        int checkAsset(ICVRSettingsData asset)
        {
            var list = asset.ICVRSettings;
            return list.Count;
        }

        public bool Initialise()
        {
            if (instance == null || checkAsset(instance) < 1)
            {
                MakeEmptyDataAsset();
                return false;
            }
            else
            {
                return true;
            }
        }

        public void ModifyDataAsset(string fn, bool pstate)
        {
            List<ICVRSettingsObject> buffer = ICVRSettings.GetRange(0, ICVRSettings.Count);
            ICVRSettings.Clear();

            for (int i = 0; i < buffer.Count; i++)
            {
                bool marked = false;
                if (buffer[i].FileName == fn)
                {
                    marked = true;
                }

                var icvrso = new ICVRSettingsObject
                {
                    Id = buffer[i].Id,
                    FileName = buffer[i].FileName,
                    FilePath = buffer[i].FilePath,
                    PresetState = marked ? pstate : buffer[i].PresetState
                };
                ICVRSettings.Add(icvrso);
            }

            Save(true);
        }

        private void MakeEmptyDataAsset()
        {
            ICVRSettings = new List<ICVRSettingsObject>();

            // Get the path to the folder containing the .preset files
            string folderPath = "Assets/ICVR/Settings/Presets";

            // Get the names of the .preset files in the folder
            string[] presetFiles = Directory.GetFiles(folderPath, "*.preset");

            // Set the default state of each preset to false (off)
            for (int i = 0; i < presetFiles.Length; i++)
            {
                string name = Path.GetFileNameWithoutExtension(presetFiles[i]);
                string path = presetFiles[i];

                var icvrso = new ICVRSettingsObject
                {
                    Id = AssetDatabase.AssetPathToGUID(path),
                    FileName = name,
                    FilePath = path,
                    PresetState = false
                };

                ICVRSettings.Add(icvrso);
            }

            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Save(true);
            Debug.Log("Created " + ICVRSettings.Count + " presets in: " + GetFilePath());
        }

    }
}
#endif