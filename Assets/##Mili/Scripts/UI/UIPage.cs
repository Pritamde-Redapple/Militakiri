using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class UIPage : MonoBehaviour
{

    public enum PageType
    {
        SPLASH_LOGIN,
        SPLASH_CHOOSE,
        LOGIN,
        MAINMENU,
        GAMEPLAY,
        REGISTER,
        CHOOSE
    }
    public PageType currentPageType;

    public Animator animator;
    public string closeTrigger;

    public virtual void OnEnter()
    {
        UIManager.instance.overlay.SetActive(false);
    }

    public virtual void OnExit()
    {
        // transform.GetChild(0).gameObject.SetActive(false);
        if (animator != null)
        {
            // Debug.Log(closeTrigger);
            this.animator.SetTrigger(closeTrigger);
        }
        Invoke("OnDestroy", 1f);
    }

    private void OnDestroy()
    {
        Destroy(this.gameObject);
    }

    public virtual void PlayAnimation()
    {

    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void Exit()
    {
        //UIController.instance.TransitionTo(UIPage.PageType.LOGIN);
    }

   

}
