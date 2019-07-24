using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splash_Login : UIPage {

    private void Start()
    {
        Invoke("LoadRegister", 2f);
    }

    void LoadRegister()
    {
        UIManager.instance.TransitionTo(PageType.LOGIN);
    }

}
