using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{

    public Ease openEaseType;
    public Ease closeEaseType;

    public GameObject popUp;
    public Image overlay;

    private float duration = 0.5f;
    private Color defaultColor;

    public static event Action<PopUpType> OnClosePopUp;

    public enum PopUpType
    {
        ERROR,
        OK,
        SIMPLECLOSE,
        LEVEL_CLOSE,
        LEVEL_SUBMIT,
        NONE
    }

    public PopUpType currentPopType;


    private void OnEnable()
    {
        defaultColor = overlay.color;
        popUp.transform.localScale = Vector3.zero;
        overlay.DOColor(new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.78f), duration);
        popUp.transform.DOScale(Vector3.one, duration).SetEase(openEaseType).OnComplete(() => { });
    }

    public void ClosePopUp()
    {
        popUp.transform.localScale = Vector3.one;
        overlay.DOColor(new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0), duration);
        popUp.transform.DOScale(Vector3.zero, duration).SetEase(closeEaseType).OnComplete(() =>
        {
            if (OnClosePopUp != null)
                OnClosePopUp(currentPopType);
            gameObject.SetActive(false);
        });
    }
    public void ClosePopUp(string type)
    {
        popUp.transform.localScale = Vector3.one;
        overlay.DOColor(new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0), duration);
        popUp.transform.DOScale(Vector3.zero, duration).SetEase(closeEaseType).OnComplete(() =>
        {
            if (OnClosePopUp != null)
                OnClosePopUp((PopUpType)Enum.Parse(typeof(PopUpType), type, true));
            gameObject.SetActive(false);
        });
    }

}
