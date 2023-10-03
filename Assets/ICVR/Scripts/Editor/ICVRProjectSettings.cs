using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.Presets;
using UnityEngine;
using WebXR;
using static WebXR.WebXRSettings;

[InitializeOnLoad]
public class ICVRProjectSettings : Editor 
{
    static AddRequest Request;
    static WebXRSettings WebXRSettings;

    private static string TAGS_ASSET = "Assets/ICVR/Settings/Presets/ICVR_Tags.preset";
    private static string PHYS_ASSET = "Assets/ICVR/Settings/Presets/ICVR_Physics.preset";
    private static string GRAPH_ASSET = "Assets/ICVR/Settings/Presets/ICVR_Graphics.preset";
    private static string QUAL_ASSET = "Assets/ICVR/Settings/Presets/ICVR_Quality.preset";
    
    private static string PLAY_ASSET = "Assets/ICVR/Settings/Presets/ICVR_PlayerSettings.preset";
    private static string EDTR_ASSET = "Assets/ICVR/Settings/Presets/ICVR_EditorSettings.preset";

    private static string LIGHT_DATA_ASSET = "Assets/ICVR/Settings/Presets/ICVR_LightingSettings.preset";
    private static string LIGHT_SETTINGS = "Assets/ICVR/Settings/ICVR_Lighting.lighting";

    private static string TAG_MNGR_ASSET = "ProjectSettings/TagManager.asset";
    private static string PHYS_MNGR_ASSET = "ProjectSettings/DynamicsManager.asset";
    private static string GRAPH_MNGR_ASSET = "ProjectSettings/GraphicsSettings.asset";
    private static string QUAL_MNGR_ASSET = "ProjectSettings/QualitySettings.asset";

    private static string PLAY_MNGR_ASSET = "ProjectSettings/ProjectSettings.asset";
    private static string EDTR_MNGR_ASSET = "ProjectSettings/EditorSettings.asset";

    static ICVRProjectSettings()
    { 
        Debug.Log("Importing ICVR Settings");

        IncludeWebXR();
        IncludePackage("Newtonsoft Json", "com.unity.nuget.newtonsoft-json");
        
        UpdateSettings(TAGS_ASSET, TAG_MNGR_ASSET);
        UpdateSettings(PHYS_ASSET, PHYS_MNGR_ASSET);
        UpdateSettings(GRAPH_ASSET, GRAPH_MNGR_ASSET);
        UpdateSettings(QUAL_ASSET, QUAL_MNGR_ASSET);
        UpdateSettings(PLAY_ASSET, PLAY_MNGR_ASSET);
        UpdateSettings(EDTR_ASSET, EDTR_MNGR_ASSET);

        UpdateLighting();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void IncludeWebXR()
    {
        if (!EditorPrefs.GetBool("WebXRPresent"))
        {
            Request = Client.Add("com.de-panther.webxr");
            EditorApplication.update += WebXRProgress;
        }
        else
        {
            Debug.Log("WebXR already Installed");
            UpdateWebXrSettings();
        }
    }

    private static void IncludePackage(string packageName, string assetId) 
    {
        if (!EditorPrefs.GetBool(packageName))
        {
            Request = Client.Add(assetId);
            EditorApplication.update += Progress;
        }
        else
        {
            Debug.Log(packageName + " already Installed");
        }
    }

    private static void UpdateWebXrSettings()
    {
        bool gotSettings = EditorBuildSettings.TryGetConfigObject("WebXR.Settings", out WebXRSettings);

        if (gotSettings)
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

    private static void UpdateLighting()
    {
        var lightingDataAsset = AssetDatabase.LoadMainAssetAtPath(LIGHT_DATA_ASSET) as Preset;
        var lightingSettings = AssetDatabase.LoadMainAssetAtPath(LIGHT_SETTINGS) as LightingSettings;

        lightingDataAsset.ApplyTo(lightingSettings);
        Lightmapping.SetLightingSettingsForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), lightingSettings);
    }

    private static void WebXRProgress()
    {
        if (Request.IsCompleted)
        {
            if (Request.Status == StatusCode.Success)
            {
                Debug.Log("Installed: " + Request.Result.packageId);
                EditorPrefs.SetBool("WebXRPresent", true);
                UpdateWebXrSettings();
            }
            else if (Request.Status >= StatusCode.Failure)
            {
                Debug.Log(Request.Error.message);
            }
            EditorApplication.update -= WebXRProgress;
        }
    }

    private static void Progress()
    {
        if (Request.IsCompleted)
        {
            if (Request.Status == StatusCode.Success)
            {
                Debug.Log("Installed: " + Request.Result.packageId);
                EditorPrefs.SetBool("Newtonsoft Json", true);
            }
            else if (Request.Status >= StatusCode.Failure)
            {
                Debug.Log(Request.Error.message);
            }
            EditorApplication.update -= Progress;
        }
    }
}