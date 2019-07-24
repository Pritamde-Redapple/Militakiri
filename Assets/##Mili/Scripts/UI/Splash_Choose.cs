using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splash_Choose : UIPage {

    private void Start()
    {
        Invoke("LoadChoose", 2f);
    }

    void LoadRegister()
    {
        UIManager.instance.TransitionTo(PageType.LOGIN);
    }

    void LoadChoose()
    {
        UIManager.instance.TransitionTo(PageType.CHOOSE);
    }
}
