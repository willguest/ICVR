using UnityEditor;
using UnityEngine;
using WebXR;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;

namespace ICVR
{
    [InitializeOnLoad]
    public class EditorSettingsManager : ScriptableObject
    {
        static EditorSettingsManager()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        void OnDestroy()
        {
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        static void OnAfterAssemblyReload()
        {
            TryUpdateWebXRSettings(); 
        }

        /// <summary>
        /// Used to override the WebXR Project Settings for compatibility with ICVRCameraSet. 
        /// The use of 'SessionState' means that the value survives assembly reloads, and is 
        /// cleared when Unity exists.
        /// </summary> 
        public static void TryUpdateWebXRSettings(bool forceUpdate = false)
        {
            if (!SessionState.GetBool("hasUpdatedSettings", false) || forceUpdate)
            {
                SessionState.SetBool("hasUpdatedSettings", true);

                // set 'Initialize XR on Startup' to true
                XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
                EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);

                if (buildTargetSettings)
                {
                    XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.WebGL);
                    settings.InitManagerOnStart = true;
                }
                else
                {
                    SessionState.SetBool("hasUpdatedSettings", false);
                }

                // TODO: assign loader and enable WebXR from here.
                // See https://forum.unity.com/threads/editor-programmatically-set-the-vr-system-in-xr-plugin-management.972285/

                // set WebXR plugin settings
                try
                {
                    EditorBuildSettings.TryGetConfigObject("WebXR.Settings", out WebXRSettings webXRSettings);
                    webXRSettings.VRRequiredReferenceSpace = WebXRSettings.ReferenceSpaceTypes.local;
                    webXRSettings.VROptionalFeatures = 0;

                    EditorUtility.SetDirty(webXRSettings);
                    AssetDatabase.SaveAssetIfDirty(webXRSettings);

                    Debug.Log("WebXR settings updated");
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}