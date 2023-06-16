using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XLua;

namespace UI.xLuaTest
{
    public class xLuaHotLoad : MonoBehaviour
    {
        private const string path = @"http://localhost/AssetBundles/xluatest";
        private const string newPath = "HotScripts" + @"/luaScript.lua.txt";
        private const string hotPath = "HotScripts";
        public AssetBundle AB;
        public Slider ProgressSlider;
        public Text ProgressText;

        private IEnumerator GetAssetBundle(Action callback)
        {
            var www = UnityWebRequestAssetBundle.GetAssetBundle(path);
            www.SendWebRequest();
            while (!www.isDone)
            {
                Debug.Log(www.downloadProgress);
                ProgressSlider.value = www.downloadProgress;
                ProgressText.text = Mathf.Floor(www.downloadProgress * 100) + "%";
                yield return 1;
            }

            if (www.isDone)
            {
                ProgressText.text = 100 + "%";
                ProgressSlider.value = 1;
            }

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.result);
            }
            else
            {
                AB = DownloadHandlerAssetBundle.GetContent(www);
                var hot = AB.LoadAsset<TextAsset>("luaScript.lua.txt");

                if (!Directory.Exists(hotPath))
                {
                    Directory.CreateDirectory(hotPath);
                }
                if (!File.Exists(newPath))
                {
                    File.Create(newPath).Dispose();
                }
                File.WriteAllText(newPath, hot.text);
                
                Debug.Log("下载资源成功！");
                callback();
            }
        }

        private void ExecuteHotFix()
        {
            LuaEnv luaEnv = new();
            luaEnv.AddLoader(MyLoader);
            luaEnv.DoString("require 'luaScript'");
        }

        private byte[] MyLoader(ref string filePath)
        {
            var textString = File.ReadAllText(hotPath + @"/" + filePath + ".lua.txt");
            return Encoding.UTF8.GetBytes(textString);
        }
        
        public void OnHotLoadBtnClick()
        {
            Debug.Log("开始下载更新");
            StartCoroutine(GetAssetBundle(ExecuteHotFix));
        }
    }
}