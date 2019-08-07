using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MultiplayerManager : Singleton<MultiplayerManager>
{
    public static UnityAction<Constants.PlayerTag> SetPlayerTag;

    public static Constants.PlayerTag MyPlayerTag;
    
    private void Start()
    {
        SetPlayerTag += SetLocalPlayerTag;
        SceneManager.activeSceneChanged += ChangedActiveScene;
    }

    private void ChangedActiveScene(Scene arg0, Scene arg1)
    {
        if(arg1.name == "BoardScene")
        {

        }
    }

    public void SetLocalPlayerTag(Constants.PlayerTag playerTag)
    {
        MyPlayerTag = playerTag;
     //   Debug.Log("Player Tag: "+ playerTag.ToString());
    }

    public static Constants.PlayerTag GetPlayerTag()
    {  
        return MyPlayerTag;
    }

    public void SwitchToGameScene()
    {
        Invoke("ChangeScene", 3);
    }

    void ChangeScene()
    {        
        SceneManager.LoadScene(1);
    }
}


