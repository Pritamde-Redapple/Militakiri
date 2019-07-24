using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    public static UIManager instance;

    public GameObject overlay;

    public UIPage currentPage;
    public bool fromChoosePage = true;
    public bool eraseData = false;
    private void Awake()
    {
        instance = this;
        Screen.SetResolution(621, 1104, false);
        if(eraseData)
        {
            Database.DeleteEverything();
            return;
        }
        if (PopupCanvas.Instance == null)
        {
            if(Database.GetBool(Database.Key.IS_LOGGEDIN))
            {
                TransitionTo(UIPage.PageType.SPLASH_CHOOSE);
            }
            else
            {
                TransitionTo(UIPage.PageType.SPLASH_LOGIN);
            }

        }
        else
        {
            TransitionTo(UIPage.PageType.CHOOSE);
        }






        //    if (PopupCanvas.Instance == null)
        //    if(fromChoosePage)
        //        TransitionTo(UIPage.PageType.CHOOSE);
        //    else
        //        TransitionTo(UIPage.PageType.SPLASH_LOGIN);
        //else
        //    TransitionTo(UIPage.PageType.CHOOSE);
    }
    private void Start()
    {
       

    }
    private void Update()
    {
        
    }

    public void TransitionTo(UIPage.PageType _eTo)
    {
        StartCoroutine(TransitionToCo(_eTo));
    }


    IEnumerator TransitionToCo(UIPage.PageType _eTo)
    {
        overlay.transform.SetAsLastSibling();
        overlay.SetActive(true);
        if (currentPage != null)
        {
            currentPage.OnExit();
        }
        yield return new WaitForSeconds(0.75f);
        GameObject _obj = Resources.Load<GameObject>("Screen/" + _eTo.ToString());
        GameObject _loadedObject = Instantiate(_obj, this.transform);
        currentPage = _loadedObject.GetComponent<UIPage>();
        currentPage.OnEnter();
    }
}
