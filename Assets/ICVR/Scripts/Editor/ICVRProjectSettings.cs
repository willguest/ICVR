#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using WebXR;
using static WebXR.WebXRSettings;


namespace ICVR.Settings
{
    [InitializeOnLoad]
    public class ICVRProjectSettings : Editor
    {
        static AddRequest Request;
        static ListRequest ListRequest;

        static WebXRSettings WebXRSettings;
        static List<string> projectPackages;

        

        public void GetProjectPackages()
        {
            projectPackages = new List<string>();
            ListRequest = Client.List(true, false); 
            EditorApplication.update += PackageProgress;
        }

        public bool QueryPackageStatus(string packageName)
        {
            ListRequest = Client.List(true, false);

            while (!ListRequest.IsCompleted)
            {
                // Waiting for the request to complete
            }

            if (ListRequest.Status == StatusCode.Success)
            {

                foreach (var package in ListRequest.Result)
                {
                    if (package.name == packageName)
                    {
                        //Debug.Log(packageName + " is installed");
                        return true;
                    }
                }
            }
            else if (ListRequest.Status >= StatusCode.Failure)
            {
                Debug.Log(ListRequest.Error.message);
            }

            return false;

        }

        public static void IncludePackage(string assetId)
        {
            if (!projectPackages.Contains(assetId))
            {
                Request = Client.Add(assetId);
                EditorApplication.update += Progress;
            }
            else
            {
                Debug.Log(assetId + " already installed");
                if (assetId == "com.de-panther.webxr")
                {
                    UpdateWebXrSettings();
                }
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


        private static void Progress()
        {
            if (Request.IsCompleted)
            { 
                if (Request.Status == StatusCode.Success)
                {
                    Debug.Log("Installed: " + Request.Result.packageId);
                }
                else if (Request.Status >= StatusCode.Failure)
                {
                    Debug.Log(Request.Error.message);
                }
                EditorApplication.update -= Progress;
            }
        }

        private static void PackageProgress()
        {
            if (ListRequest.IsCompleted)
            {
                if (ListRequest.Status == StatusCode.Success)
                {
                    projectPackages = new List<string>();
                    foreach (var package in ListRequest.Result)
                    {
                        projectPackages.Add(package.name);
                    }
                    //Debug.Log("Found " + projectPackages.Count + " project packages"); 
                }
                else if (Request.Status >= StatusCode.Failure)
                {
                    Debug.Log(Request.Error.message);
                }
                EditorApplication.update -= PackageProgress;
            }
        }
    }

}

#endif