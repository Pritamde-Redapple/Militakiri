using UnityEngine;
using System.Collections;
using SimpleJSON;
namespace GWebUtility
{

    public class WebCacheManager : MonoBehaviour
    {
        private static string PATH = Application.persistentDataPath + "/";
        private const string CACHE_FILE_NAME = "localCache.meta";
        private static WebCacheManager _instance;
        private JSONNode cacheNode;

        public static WebCacheManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject tempGo = new GameObject();
                    _instance = tempGo.AddComponent<WebCacheManager>();
                    _instance.name = "WebCacheManager";
                    //_instance.LoadCache(PATH + CACHE_FILE_NAME);
                    Debug.Log("s111111111");
                    return _instance;
                }
                else
                {
                    return _instance;
                }
            }
        }

        /*public void LoadCache(string path)
        {
#if !UNITY_WEBGL && !UNITY_WEBPLAYER
            IO io = IO.Create();
            if (io.Exists(path))
                cacheNode = JSONNode.LoadFromFile(path);
            else
                cacheNode = JSON.Parse("{\"cache\":{}}");
#endif
        }*/

     /*   public void AddInCache(Response response, Web.ResponseType responseType, string url)
        {
#if !UNITY_WEBGL && !UNITY_WEBPLAYER
            if (responseType == Web.ResponseType.IMAGE)
            {
                //save image data in local cache
                IO io = IO.Create();
                string path = PATH + System.DateTime.Now.ToString(@"ddMMyyyyHHmmssfffffff.png");
                io.Write(path, response.GetImage().EncodeToPNG());
                io.Close();
                AddUrl(url, path);

            }
            else if (responseType == Web.ResponseType.TEXT)
            {

                IO io = IO.Create();
                string path = PATH + System.DateTime.Now.ToString(@"ddMMyyyyHHmmssfffffff.txt");
                io.Write(path, response.GetText());
                io.Close();
                AddUrl(url, path);
            }
#endif
        }*/
        public bool LoadUrl(out string outUrl, string inUrl)
        {
            outUrl = "";
            if (IsUrlExist(inUrl))
            {
                outUrl = "file://" + cacheNode["cache"][inUrl]["path"].Value;
                Debug.Log("outUrl :" + outUrl);
                return true;

            }
            else
            {
                outUrl = inUrl;
                Debug.Log("outUrl :" + outUrl);
                return false;
            }
        }
        void OnApplicationQuit()
        {
            SaveCache(PATH + CACHE_FILE_NAME);
        }
        void SaveCache(string path)
        {
            //cacheNode.SaveToFile(path);
            Debug.Log(cacheNode.ToString());
        }

        void AddUrl(string url, string localUrl)
        {
            cacheNode["cache"].Add(url, JSON.Parse("{\"path\":\"" + localUrl + "\"}"));
            Debug.Log(cacheNode.ToString());
        }
        bool IsUrlExist(string url)
        {
            return cacheNode["cache"][url].ToString() != "";
        }
    }


}
