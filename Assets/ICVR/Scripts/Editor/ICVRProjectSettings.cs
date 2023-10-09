#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.Presets;
using UnityEngine;
using WebXR;
using static WebXR.WebXRSettings;


namespace ICVR.Settings
{
    [InitializeOnLoad]
    public class ICVRProjectSettings : Editor
    {
        private static PresetToggleEditorWindow presetToggleEditorWindow;

        //static PresetToggleEditorWindow setts;
        static AddRequest Request;
        static WebXRSettings WebXRSettings;

        private static string LIGHT_DATA_ASSET = "Assets/ICVR/Settings/Presets/ICVR_LightingSettings.preset";
        private static string LIGHT_SETTINGS = "Assets/ICVR/Settings/ICVR_Lighting.lighting";


        static ICVRProjectSettings()
        {
            Debug.Log("Updating ICVR Settings");

            //IncludeWebXR();
            //IncludePackage("Newtonsoft Json", "com.unity.nuget.newtonsoft-json");

            //UpdateLighting();

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
                EditorPrefs.SetBool(assetId, true);
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

        private static void UpdateLighting()
        {
            var lightingDataAsset = AssetDatabase.LoadMainAssetAtPath(LIGHT_DATA_ASSET) as Preset;
            var lightingSettings = AssetDatabase.LoadMainAssetAtPath(LIGHT_SETTINGS) as LightingSettings;

            try { lightingDataAsset.ApplyTo(lightingSettings); } catch (System.Exception e) { Debug.Log("light err"); };

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

}

#endif