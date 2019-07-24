using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : UIPage {

	public void PlayClicked()
    {
        UIManager.instance.TransitionTo(PageType.CHOOSE);
    }
    public void TutorialClicked()
    {

    }
    public void LeaderboardClicked()
    {

    }
    public void OtherClicked()
    {

    }
    public void SubscriptionClicked()
    {

    }
    public void FriendListClicked()
    {

    }

    public void Logout()
    {
        UIManager.instance.TransitionTo(PageType.SPLASH_LOGIN);
    }
}
