using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class Choose : UIPage {

    public Sprite[] baseImageForBoard;
    public Sprite[] baseImageForGameMode;

    public Image[] playerTypeImages;
    private int playerTypeSelected;
    public Image[] boardTypeImages;
    public Image[] gameModeTypeImages;

    public Image[] colorValueForBothUser;
    public Image[] colorImageForBothUser;
    private int[] colorIndexForBothUser;
    private int currentSelectedUser = -1;
    private int currentSelectedColorIndex = -1;

    [Header("Material")]
    public List<Material> allMaterials;
    public List<Texture> defaultTextures;      

    public Transform allColorParent;
    public GameObject colorChoosePanel;
    public GameObject animationPanel;

    [Header("Difficulty Level")]
    public GameObject levelPopup;
    public Sprite selectedSprite;
    public Sprite deSelectedSprite;
    public List<Image> allLevel;
    private int selectedLevel = 0;

    private PawnAndSquareMaterialInfo pawnAndSquareMaterialInfo;
    private void Start()
    {
        PopUp.OnClosePopUp += ClosePopUp;
    }

    private void OnDisable()
    {
        PopUp.OnClosePopUp -= ClosePopUp;
    }
    public override void OnEnter()
    {
        base.OnEnter();
        pawnAndSquareMaterialInfo = new PawnAndSquareMaterialInfo();
        colorIndexForBothUser = new int[4];
        //colorIndexForBothUser[0] = GetColorData(0,0);
        //colorIndexForBothUser[1] = GetColorData(1, 0);
        //colorIndexForBothUser[2] = 0;
        //colorIndexForBothUser[3] = 1;

        for (int i = 0; i < 4; i++)
        {
            SetColorValueOrImageForUser(i, GetColorData(i, i%2));
        }

        LevelTypeClicked((int)Constants.currentLevelType);
        Constants.currentViewType = Constants.ViewType.THREE_D;
    }

    public void BackClicked()
    {
        UIManager.instance.TransitionTo(PageType.MAINMENU);
    }
    public void PlayerTypeClicked(int type)
    {
        for (int i = 0; i < 2; i++)
        {
          playerTypeImages[i].sprite = baseImageForBoard[0];
        }
        playerTypeImages[type].sprite = baseImageForBoard[1];
        playerTypeSelected = type;
    }
    public void BoardTypeClicked(int type)
    {
        for (int i = 0; i < 2; i++)
        {
            boardTypeImages[i].sprite = baseImageForBoard[0];
        }
        boardTypeImages[type].sprite = baseImageForBoard[1];

        Constants.currentBoardType = (Constants.BoardType)Enum.ToObject(typeof(Constants.BoardType), type);
    }

    public void Animationclicked()
    {
        animationPanel.SetActive(true);
    }
    public void GameModeTypeClicked(int type)
    {
        for (int i = 0; i < 2; i++)
        {
            gameModeTypeImages[i].sprite = baseImageForGameMode[0];
        }
        gameModeTypeImages[type].sprite = baseImageForGameMode[1];

        Constants.currentViewType = (Constants.ViewType)Enum.ToObject(typeof(Constants.ViewType), type);
    }

    public void ChooseColorClicked(int type)
    {
        currentSelectedUser = type;
        currentSelectedColorIndex = colorIndexForBothUser[type];
        ResetAllColorScale();
        colorChoosePanel.SetActive(true);
    }
    public void OkChooseColorClicked()
    {
        SetColorValueOrImageForUser(currentSelectedUser, currentSelectedColorIndex);
    }

    void SetColorValueOrImageForUser(int indexForUser, int colorIndex)
    {
       
        if(colorIndex > 34)
        {
            //Color image
            colorValueForBothUser[indexForUser].gameObject.SetActive(false);
            colorImageForBothUser[indexForUser].gameObject.SetActive(true);
            colorImageForBothUser[indexForUser].sprite
                = allColorParent.GetChild(colorIndex).GetChild(0).GetComponent<Image>().sprite;
        }
        else
        {
            colorValueForBothUser[indexForUser].gameObject.SetActive(true);
            colorImageForBothUser[indexForUser].gameObject.SetActive(false);
            colorValueForBothUser[indexForUser].color 
                = allColorParent.GetChild(colorIndex).GetChild(0).GetComponent<Image>().color;

        }
        pawnAndSquareMaterialInfo.indexforPawnAndSquare[indexForUser] = colorIndex;
    }

    void ResetAllColorScale()
    {
        for (int i = 0; i < allColorParent.childCount; i++)
        {
            allColorParent.GetChild(i).localScale = Vector3.one;
        }
    }
    public void ColorClicked()
    {
        ResetAllColorScale();
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        obj.transform.DOScale(Vector3.one * 1.1f, 0.1f).SetEase(Ease.OutBounce).OnComplete(() => { });
        currentSelectedColorIndex = obj.transform.GetSiblingIndex();
        // Debug.Log(.transform.GetSiblingIndex());
    }


    public void LevelTypeClicked(int index)
    {
        for (int i = 0; i < allLevel.Count; i++)
        {
            if (i == index)
                allLevel[i].sprite = selectedSprite;
            else
                allLevel[i].sprite = deSelectedSprite;
        }
        selectedLevel = index;
    }

    void LevelTypeCloseClicked()
    {
        LevelTypeClicked(0);
    }

    void LevelTypeSubmitClicked()
    {
        Constants.currentLevelType = (Constants.LevelType)selectedLevel;
        Constants.isAI = true;
        SceneManager.LoadScene(1);
    }

    public void PlayClicked()
    {
       if(pawnAndSquareMaterialInfo.indexforPawnAndSquare.Distinct().Count() != 4)
        {
            PopupCanvas.Instance.ShowAlertPopUp("Pawn and tile colors should be unique");
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            int colorIndex = pawnAndSquareMaterialInfo.indexforPawnAndSquare[i];
         //   Debug.Log("Color Index: "+ colorIndex);
            if (colorIndex < 34)
            {
                allMaterials[i].mainTexture = defaultTextures[i];
                allMaterials[i].color = allColorParent.GetChild(colorIndex).GetChild(0).GetComponent<Image>().color;
            }
            else
            {
                allMaterials[i].mainTexture = allColorParent.GetChild(colorIndex).GetChild(0).GetComponent<Image>().sprite.texture;
                allMaterials[i].color = Color.white;
                //allMaterials[i].color = allColorParent.GetChild(colorIndex).GetChild(0).GetComponent<Image>().color;
            }

            SaveColorData(i, colorIndex);
        }


        if (playerTypeSelected == 0) //this is for local  mode
        {
            levelPopup.SetActive(true);
        }
        else  //multiplayer mode
        {
            Constants.isAI = false;
            // SceneManager.LoadScene(2);
            LoadingCanvas.Instance.ShowLoadingPopUp("Searching Player! Please wait");
            ConnectServer();
        }
    }

    void ClosePopUp(PopUp.PopUpType type)
    {
        switch (type)
        {
            case PopUp.PopUpType.ERROR:
                break;
            case PopUp.PopUpType.OK:
                OkChooseColorClicked();
                break;
            case PopUp.PopUpType.SIMPLECLOSE:
                break;
            case PopUp.PopUpType.LEVEL_CLOSE:
                LevelTypeCloseClicked();
                break;
            case PopUp.PopUpType.LEVEL_SUBMIT:
                LevelTypeSubmitClicked();
                break;
            case PopUp.PopUpType.NONE:
                break;
            default:
                break;
        }
       
    }

    void SaveColorData(int index, int colorIndex)
    {
        PlayerPrefs.SetInt("ColorData " + index.ToString(), colorIndex);
    }
    int GetColorData(int index, int defaultValue)
    {
       return PlayerPrefs.GetInt("ColorData " + index.ToString(), defaultValue);
    }

    void ConnectServer()
    {
        SocketController.instance.AddUser();
       // SocketController.instance.GameRequest();
    }
}
