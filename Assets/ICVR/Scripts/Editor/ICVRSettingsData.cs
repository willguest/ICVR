using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


[System.Serializable]
public class ICVRSettingsObject
{
    public string Id;
    public string FileName = string.Empty;
    public string FilePath = string.Empty;
    public bool PresetState = false;

}


[FilePath("Assets/ICVR/Settings/ICVRSettingsData.asset", FilePathAttribute.Location.ProjectFolder)]
public class ICVRSettingsData : ScriptableSingleton<ICVRSettingsData>
{

    [SerializeField]
    public List<ICVRSettingsObject> ICVRSettings;


    int checkAsset(ICVRSettingsData asset)
    {
        var list = asset.ICVRSettings;
        return list.Count;
    } 


    public void Initialise()
    {
        var asset = AssetDatabase.LoadMainAssetAtPath(GetFilePath()) as ICVRSettingsData;

        if (checkAsset(asset) < 1)
        {
            Debug.Log("making empty preset list");
            MakeEmptyDataAsset();
            AssetDatabase.Refresh();
        }
        else
        {
            ICVRSettings = asset.ICVRSettings;
        }
    }


    void MakeEmptyDataAsset()
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

        Save(true);
        Debug.Log("Saved " + presetFiles.Length + " presets to: " + GetFilePath());
    }



    public void UpdateAsset(ICVRSettingsObject so, bool newValue)
    {
        foreach (ICVRSettingsObject o in ICVRSettings)
        {
            if (o.FileName == so.FileName)
            {
                Debug.Log("Found " + so.Id + " also called " + o.FileName);
                o.Id = so.Id;
                o.FilePath = so.FilePath;
                o.PresetState = so.PresetState;
            }
        }

        Save(true);
        Debug.Log("Saved to: " + GetFilePath());
    }


}