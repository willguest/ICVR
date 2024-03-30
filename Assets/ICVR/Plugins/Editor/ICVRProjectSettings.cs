#if UNITY_EDITOR

#if EARLY_JSON
using Newtonsoft.Json;
#endif

using System.IO;
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
        
        public static List<string> projectPackages { get; private set; }

        public void GetProjectPackages()
        {
            projectPackages = new List<string>();
            ListRequest = Client.List(true, false); 
            EditorApplication.update += ListProgress;
        }

        public bool CheckForPackage(string assetId)
        {
            if (projectPackages.Count < 1) return false;

            foreach (string p in projectPackages)
            {
                if (p.Contains(assetId))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckPackSync(string packageName)
        {
            ListRequest = Client.List(true, false);
            while (!ListRequest.IsCompleted) {  }

            if (ListRequest.Status == StatusCode.Success)
            {
                foreach (var package in ListRequest.Result)
                {
                    if (package.name.Contains(packageName))
                    {   return true;    }
                }
            }
            else if (ListRequest.Status >= StatusCode.Failure)
            {   
                Debug.Log(ListRequest.Error.message);   
            }
            return false;
        }

        // TODO: change the way package are checked and loaded
        // (ensure 'com.unity.nuget.newtonsoft-json' is first)

        public void TryIncludePackage(string assetId)
        {
            bool foundPackage = CheckForPackage(assetId);
            if (assetId.Contains("com.de-panther"))
            {
                //Debug.Log("Adding scoped registry for " + assetId);
                AddScopedRegistry("com.de-panther");
            }

            if (!foundPackage)
            {
                Request = Client.Add(assetId);
                EditorApplication.update += AddProgress;

                if (assetId.Contains("newtonsoft-json"))
                {
                    SDSUtility.AddSymbol(BuildTargetGroup.WebGL, "EARLY_JSON");
                }
            }
            else
            {
                //Debug.Log(assetId + " is already installed");
                if (assetId.Contains("com.de-panther"))
                {
                    SDSUtility.RemoveSymbol(BuildTargetGroup.WebGL, "EARLY_JSON");
                }
            }
        }


        public void AddScopedRegistry(string packageId)
        {
#if EARLY_JSON
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages/manifest.json");
            var manifestJson = File.ReadAllText(manifestPath);

            var manifest = JsonConvert.DeserializeObject<ManifestJson>(manifestJson);
            ScopedRegistry pScopeRegistry = OpenUpmReg(packageId);

            if (manifest.scopedRegistries.Count == 0)
            {
                manifest.scopedRegistries.Add(pScopeRegistry);
                File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
                AssetDatabase.RefreshSettings();
            }
            else foreach (var pScope in manifest.scopedRegistries)
            {
                Debug.Log("index value: " + (System.Array.IndexOf(pScope.scopes, packageId)));
                if (System.Array.IndexOf(pScope.scopes, packageId) == -1)
                {
                    manifest.scopedRegistries.Add(pScopeRegistry);
                    File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
                }
                else
                {
                    Debug.Log("Manifest '" + pScope.name + "' " + " already contains scope: " + packageId);
                }
            }
#else
            Debug.Log("Newtonsoft Json not available, please install it and try again");
#endif
        }

        private static ScopedRegistry OpenUpmReg(string scope)
        {
            return new ScopedRegistry
            {
                name = "OpenUPM",
                url = "https://package.openupm.com",
                scopes = new string[] { scope }
            };
        }

        private static void AddProgress()
        {
            if (Request.IsCompleted)
            { 
                if (Request.Status == StatusCode.Success)
                {
                    Debug.Log("Installed " + Request.Result.packageId);

                    // a hack to allow newtonsoft json to be used after load, but only once
                    if (Request.Result.packageId.Contains("newtonsoft-json"))
                    {
                        SDSUtility.AddSymbol(BuildTargetGroup.WebGL, "EARLY_JSON");
                    }
                    else if (Request.Result.packageId.Contains("com.de-panther"))
                    {
                        SDSUtility.RemoveSymbol(BuildTargetGroup.WebGL, "EARLY_JSON");
                    }
                }
                else if (Request.Status >= StatusCode.Failure)
                {
                    Debug.Log(Request.Error.message);
                }
                EditorApplication.update -= AddProgress;
            }
        }

        private static void ListProgress()
        {
            if (ListRequest.IsCompleted)
            {
                if (ListRequest.Status == StatusCode.Success)
                {
                    projectPackages = new List<string>();
                    foreach (var package in ListRequest.Result)
                    {
                        if (!projectPackages.Contains(package.name))
                        {
                            projectPackages.Add(package.name);
                        }
                    }
                }
                else if (Request.Status >= StatusCode.Failure)
                {
                    Debug.Log(Request.Error.message);
                }

                EditorApplication.update -= ListProgress;
            }
        }
    }

}

#endif