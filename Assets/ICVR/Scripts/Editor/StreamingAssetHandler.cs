#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace ICVR
{
    [CustomEditor(typeof(StreamingAsset))]
    public class StreamingAssetHandler : Editor
    {

        SerializedProperty extension;
        SerializedProperty filePaths;
        SerializedProperty streamingAssetFolder;

        const string kAssetPrefix = "Assets/StreamingAssets";

        void OnEnable()
        {
            extension = serializedObject.FindProperty("extension");
            filePaths = serializedObject.FindProperty("filePaths");
            streamingAssetFolder = serializedObject.FindProperty("streamingAssetFolder");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(streamingAssetFolder);
            EditorGUILayout.PropertyField(extension);

            if (streamingAssetFolder.objectReferenceValue == null)
            {
                return;
            }

            var Paths = Directory.GetFiles(
                AssetDatabase.GetAssetPath(
                    streamingAssetFolder.objectReferenceValue.GetInstanceID()),
                    "*." + extension.stringValue,
                    SearchOption.TopDirectoryOnly);


            int noEntries = Paths.Length;
            filePaths.arraySize = noEntries;

            for (int x = 0; x < noEntries; x++)
            {
                if (Paths[x].StartsWith(kAssetPrefix))
                {
                    Paths[x] = Paths[x].Substring(kAssetPrefix.Length);
                }

                if (filePaths.arraySize < noEntries)
                {
                    //filePaths.AddArrayItem(Paths[x]);
                    filePaths.InsertArrayElementAtIndex(filePaths.arraySize - 1);
                    Debug.Log("added new array element at " + (filePaths.arraySize - 1) + "(x=" + x + ")");
                }
                else
                {
                    SerializedProperty filepath = filePaths.GetArrayElementAtIndex(x);
                    filepath.stringValue = Paths[x];
                }

            }

            EditorGUILayout.PropertyField(filePaths, true); // draw property with it's children

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
