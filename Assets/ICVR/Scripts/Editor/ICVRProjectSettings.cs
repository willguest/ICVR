#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ICVR.Settings
{
    [InitializeOnLoad]
    public class ICVRProjectSettings : Editor
    {
        static AddRequest Request;
        static ListRequest ListRequest;  
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

        public void IncludePackage(string assetId)
        {
            if (!projectPackages.Contains(assetId))
            {
                Request = Client.Add(assetId);
                EditorApplication.update += Progress;
            }
            else
            {
                Debug.Log(assetId + " is already installed");
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