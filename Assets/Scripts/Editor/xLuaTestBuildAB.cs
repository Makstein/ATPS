using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class xLuaTestBuildAB
    {
        [MenuItem("Assets/Build AB")]
        public static void BuildAllAB()
        {
            const string dir = "AssetBundles";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // ChunkBasedCompression: use lz4
            BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
            Debug.Log("打包完成");
        }
    }
}