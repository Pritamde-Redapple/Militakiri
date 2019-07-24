using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvas : MonoBehaviour
{
    public static LoadingCanvas Instance;
    public Text txtLoadingText;
    public Image loadingIcon;
    public float animationDelay;
    public GameObject goLoadingCanvas;


    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }

    public void OnEnable()
    {
        txtLoadingText.text = "";
        goLoadingCanvas.SetActive(false);
    }

    public void ShowLoadingPopUp(string _LoadingTxt)
    {
        goLoadingCanvas.SetActive(true);
        txtLoadingText.text = _LoadingTxt;
        StopCoroutine("LoadingAnimation");
        StartCoroutine("LoadingAnimation");
    }
    public void ShowLoadingPopUp()
    {
        goLoadingCanvas.SetActive(true);
        StopCoroutine("LoadingAnimation");
        StartCoroutine("LoadingAnimation");
    }

    public void ShowOnlyInfo(string info)
    {
        goLoadingCanvas.SetActive(true);
        txtLoadingText.text = info;
        StopCoroutine("LoadingAnimation");
        loadingIcon.gameObject.SetActive(false);
        // StartCoroutine("LoadingAnimation");
    }

    public void HideLoadingPopUp()
    {
        goLoadingCanvas.SetActive(false);
    }

    IEnumerator LoadingAnimation()
    {
        
        while (true)
        {
            yield return new WaitForSeconds(animationDelay * Time.deltaTime);
            loadingIcon.gameObject.SetActive(true);
            loadingIcon.GetComponent<RectTransform>().transform.Rotate(new Vector3(0.0f, 0.0f, -200.0f * Time.deltaTime));
            //			loadingIcon.sprite = loadingSprites[i];
            //			i=(i+1)%loadingSprites.Length;
        }
    }
}
