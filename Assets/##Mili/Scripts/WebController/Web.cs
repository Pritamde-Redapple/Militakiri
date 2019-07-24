using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;

namespace GWebUtility
{
    public class Web : MonoBehaviour
    {
        public static bool debug = false;
        public enum RequestType { GET, POST };

#if UNITY_ANDROID || UNITY_IOS
        public enum ResponseType { TEXT, IMAGE, AUDIO };
#else
        public enum ResponseType { TEXT, IMAGE, AUDIO, VIDEO };
#endif

        public delegate void Success(Web web, Response response);
        public Success OnSuccess;
        public delegate void Fail(Web web, Response response);
        public Fail OnFail;

        #region All Private variables
        private bool _isDebug;
        private Response _debugData;
        private string _url;
        private RequestType _requestType;
        private Texture2D _refImage;
        private ResponseType _responseType;
        private WWWForm _webForm;

        private Dictionary<string, string> _headerData;
        private static int activeWebObjects;
        private bool _autoDestroy;
       
        private bool shouldUseCache = false;
        #endregion

        /// <summary>
        /// Create a Web instance.
        /// </summary>
        public static Web Create()
        {
            GameObject tempGo = new GameObject();
            tempGo.name = "Web_" + tempGo.GetInstanceID();
            return tempGo.AddComponent<Web>();
        }

        /// <summary>
        /// Sets the URL.
        /// </summary>
        /// <returns>Web instance</returns>
        /// <param name="url">URL.</param>
        /// <param name="requestType">(optional) Request type.{GET,POST} default is GET</param>
        /// <param name="responseType">(optional) Response type.{TEXT,IMAGE,AUDIO,VIDEO} default is TEXT ,VIDEO can't support in android and ios </param>

        public Web SetUrl(string url, RequestType requestType = RequestType.GET, ResponseType responseType = ResponseType.TEXT)
        {
            _url = url;
            _requestType = requestType;
            _responseType = responseType;

            if (requestType == RequestType.POST)
                _webForm = new WWWForm();
            if (debug)
                Debug.Log("URL :" + url);

            return this;
        }

        public Web SetReferenceImage(Texture2D refImage)
        {
            _refImage = refImage;
            return this;
        }


        /// <summary>
        /// Adds the field. If you want POST method
        /// </summary>
        /// <returns>The field.</returns>
        /// <param name="field">Field.</param>
        /// <param name="value">Value.</param>
        public Web AddField(string field, string value)
        {
            if (_webForm == null)
            {
                Debug.Log("Error :Change request type to Post");
                return this;
            }
            _webForm.AddField(field, value);
            return this;
        }

        public Web AddBinaryData(string field, byte[] data, string fileName)
        {
            if (_webForm == null)
            {
                Debug.Log("Error :Change request type to Post");
                return this;
            }
            _webForm.AddBinaryData(field, data, fileName);
            return this;
        }


       


        /// <summary>
        /// Sets the debug data.To test the out put without any webservice
        /// </summary>
        /// <returns>return response data of a web service </returns>
        /// <param name="responseText">Set  string which you want as out put . </param>
        /// <param name="responseImage">Set image as webservice out put </param>
        public Web SetDebugData(string responseText = "Debug Data", Texture2D responseImage = null)
        {
            _isDebug = true;
            _debugData = new Response();
            _debugData._image = responseImage;
            _debugData._textData = responseText;
            return this;
        }
        /// <summary>
        /// Connect to webservice 
        /// </summary>
        /// <param name="autoDestroy">If autoDestroy is true . The web instance will automatically destroy after success or failure  <c>true</c> auto destroy.</param>
        public Web Connect(bool autoDestroy = false)
        {
            _autoDestroy = autoDestroy;
            StartCoroutine("ConnectionCo");


            return this;
        }


        public Web SetCache()
        {
            shouldUseCache = true;
            return this;
        }

        /// <summary>
        /// Sets the on success delegate.
        /// </summary>
        /// <returns>The on success delegate.</returns>
        /// <param name="OnSuccess">On success.</param>
        public Web SetOnSuccessDelegate(Success onSuccess)
        {
            this.OnSuccess = onSuccess;
            return this;
        }
        /// <summary>
        /// Sets the on failure delegate.
        /// </summary>
        /// <returns>The on failure delegate.</returns>
        /// <param name="onFail">On fail.</param>
        public Web SetOnFailureDelegate(Fail onFail)
        {
            this.OnFail = onFail;
            return this;
        }


#if UNITY_2017_1_OR_NEWER

        private UnityWebRequest _www ;
        
        private string _postData;
        public void Close()
        {
            _www.Dispose();
            _www = null;
            Destroy(gameObject);
        }
        public Web AddPostData(string data)
        {
            _postData = data;
            return this;
        }
        public Web AddHeader(string field, string value)
        {
           
            if (_postData != null)
            {
                _www = _www == null ? UnityWebRequest.Put(_url, Encoding.ASCII.GetBytes(_postData)) : _www;
            }
            else
            {
                _www = _www == null ? UnityWebRequest.Post(_url, _webForm) : _www;
            }
            _www.SetRequestHeader(field, value);
            return this;
        }
        IEnumerator ConnectionCo()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (OnFail != null)
                {
                    Response tempResponse = new Response();
                    tempResponse._textData = "check the connectivity";
                    tempResponse._errorData = "ERROR : NO ACTIVE CONNECTION";
                    OnFail(this, tempResponse);
                }
                yield break;
            }

            bool isUrlExist = false;
            string inUrl = _url;
            if (shouldUseCache)
            {

                isUrlExist = WebCacheManager.Instance.LoadUrl(out _url, inUrl);
            }

            if (_requestType == RequestType.GET)
            {
                if (_responseType == ResponseType.IMAGE)
                {
                    _www = UnityWebRequestTexture.GetTexture(_url);
                }
                else
                {
                    _www = UnityWebRequest.Get(_url);
                }
              
                _www.chunkedTransfer = false;
            }
            else
            {
                if (_postData != null)
                {
                    _www = _www == null ? UnityWebRequest.Put(_url, Encoding.ASCII.GetBytes(_postData)) : _www;
                }
                else
                {
                    _www = _www == null ? UnityWebRequest.Post(_url, _webForm) : _www;
                }
                _www.chunkedTransfer = false;

              
               
            }
            yield return _www.SendWebRequest();
            if (debug)
            {
               // GLog.Log("Web data :" + _www.text);
               // GLog.Log("Web Error :" + _www.error);
            }
            if (_www.error == null)
            {
                if (OnSuccess != null)
                {
                    switch (_responseType)
                    {
                        case ResponseType.TEXT:
                            Response tempResponse = new Response();
                            tempResponse._textData = _www.downloadHandler.text.ToString();
                            if (shouldUseCache)
                            {
                                if (!isUrlExist)
                                {
                                  //  WebCacheManager.Instance.AddInCache(tempResponse, ResponseType.TEXT, _url);

                                }
                            }
                            OnSuccess(this, tempResponse);
                            break;
                        case ResponseType.IMAGE:
                            tempResponse = new Response();
                            if (_refImage == null)
                            {
#if _D_I
                                //Debug.Log("The Reference Image is null............");
#endif
                                tempResponse._image = new Texture2D(1, 1, TextureFormat.RGB24, false);
                            }

                            else
                            {
#if _D_I
                               // Debug.Log("The Reference Image is not null............");
#endif
                                tempResponse._image = _refImage;
                            }
                           
                                if (_www != null)
                                {
                                tempResponse._image = ((DownloadHandlerTexture)_www.downloadHandler).texture;
                                }
                            

                                /*  if (_www != null)
                                      _www.LoadImageIntoTexture(tempResponse._image);*/
                                if (shouldUseCache)
                            {
                                if (!isUrlExist)
                                {
                                   // WebCacheManager.Instance.AddInCache(tempResponse, ResponseType.IMAGE, _url);
                                }
                            }
                            OnSuccess(this, tempResponse);
                            break;

                    }
                }
            }
            else
            {
                if (OnFail != null)
                {
                    Response tempResponse = new Response();
                    tempResponse._textData = _www.downloadHandler.text.ToString();
                    tempResponse._errorData = _www.error;
                    OnFail(this, tempResponse);
                }
            }

            if (_autoDestroy)
            {
                Close();
            }
            //yield return null;
        }
    }

#endif


#if !UNITY_2017_1_OR_NEWER
        private WWW _www;
    private byte[] _postData;

    public Web AddHeader(string field, string value)
        {
            if (_headerData == null)
                _headerData = new Dictionary<string, string>();
            _headerData.Add(field, value);
            return this;
        }
        public void Close()
        {
            _www.Dispose();
            _www = null;
            Destroy(gameObject);
        }
    public Web AddPostData(byte[] data)
        {
            _postData = data;
            return this;
        }

        IEnumerator ConnectionCo()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (OnFail != null)
                {
                    Response tempResponse = new Response();
                    tempResponse._textData = "check the connectivity";
                    tempResponse._errorData = "ERROR : NO ACTIVE CONNECTION";
                    OnFail(this, tempResponse);
                }
                yield break;
            }

            bool isUrlExist = false;
            string inUrl = _url;
            if (shouldUseCache)
            {

                isUrlExist = WebCacheManager.Instance.LoadUrl(out _url, inUrl);
            }
            if (_requestType == RequestType.GET)
                _www = new WWW(_url);
            else
            {
                if (_headerData == null)
                {
                    if (_postData == null)
                        _www = new WWW(_url, _webForm);
                    else
                        _www = new WWW(_url, _postData);
                }
                else
                {
                    _www = new WWW(_url, _postData, _headerData);
                }
            }
            yield return _www;
            if (debug)
            {
                // GLog.Log("Web data :" + _www.text);
                //GLog.Log("Web Error :" + _www.error);
            }
            if (_www.error == null)
            {
                if (OnSuccess != null)
                {
                    switch (_responseType)
                    {
                        case ResponseType.TEXT:
                            Response tempResponse = new Response();
                            tempResponse._textData = _www.text;
                            if (shouldUseCache)
                            {
                                if (!isUrlExist)
                                {
                                    // WebCacheManager.Instance.AddInCache(tempResponse, ResponseType.TEXT, _url);

                                }
                            }
                            OnSuccess(this, tempResponse);
                            break;
                        case ResponseType.IMAGE:
                            tempResponse = new Response();
                            if (_refImage == null)
                                tempResponse._image = new Texture2D(1, 1, TextureFormat.RGB24, false);
                            else
                                tempResponse._image = _refImage;
                            if (_www != null)
                                _www.LoadImageIntoTexture(tempResponse._image);
                            if (shouldUseCache)
                            {
                                if (!isUrlExist)
                                {
                                    // WebCacheManager.Instance.AddInCache(tempResponse, ResponseType.IMAGE, _url);
                                }
                            }
                            OnSuccess(this, tempResponse);
                            break;
                    }
                }
            }
            else
            {
                if (OnFail != null)
                {
                    Response tempResponse = new Response();
                    tempResponse._textData = _www.text;
                    tempResponse._errorData = _www.error;
                    OnFail(this, tempResponse);
                }
            }

            if (_autoDestroy)
            {
                Close();
            }
            //yield return null;
        }

#endif

                            }

    public class Response
    {
        internal string _textData;
        internal string _errorData;
        internal Texture2D _image;
        public string GetText() { return _textData; }
        public Texture2D GetImage() { return _image; }
        public string GetError() { return _errorData; }
        public Response()
        {
            _textData = null;
            _errorData = null;
            _image = null;
        }
    }


