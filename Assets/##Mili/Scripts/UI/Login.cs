using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GWebUtility;
using SimpleJSON;

public class Login : UIPage {

    public InputField iUserName;
    public InputField iPassword;
    
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            iUserName.text = "p@p.com";
            iPassword.text = "password";
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            iUserName.text = "de@de.com";
            iPassword.text = "123456";
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            LoginClicked();
        }
    }

    public void LoginClicked()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("no internet");
            PopupCanvas.Instance.ShowAlertPopUp("Please check your internet connection!");
            return;
        }
        if (string.IsNullOrEmpty(iUserName.text)/* || validate.ValidateEmail(email.text) == Validate.ErrorCode.INVALID*/)
        {
            PopupCanvas.Instance.ShowAlertPopUp("Enter Mobile No or Email id");
            return;
        }
        else if (iPassword.text == "")
        {
            PopupCanvas.Instance.ShowAlertPopUp("Enter password");
        }
        else
        {
            LoginAPICall(iUserName.text, iPassword.text);
        }

    }

    #region LoginResponse
/*
    
    {
"status":"1",
"result":{
"id":"5cb42218a0f19e2d5a64af93",
"name":"pritam",
"username":"p@p.com",
"registration_type":"normal",
"score":0,
"player_id":"MILT-5cb42218a0f19e2d5a64af93",
"image":"http://18.219.52.107:3009/images/avatar/defaultimage.png",
"access_token":"0e385a0d-a5d5-422f-a85d-e08668115541"
},
"message":"Login Successfully"
} 
**/

#endregion

public void LoginAPICall(string userName, string password)
    {
        Debug.Log("Username: "+ userName + " Password: "+ password);
        LoadingCanvas.Instance.ShowLoadingPopUp("Loading...");
        // Debug.Log("the Name of the Api: " + Configuration.Instance.GetApi(Configuration.ApiKey.LOGIN));
        Web.Create()
           .SetUrl(Configuration.Instance.GetApi(Configuration.ApiKey.LOGIN), Web.RequestType.POST, Web.ResponseType.TEXT)
           .AddField(Constants.USER_NAME, userName)
           .AddField(Constants.PASSWORD, password)

          .SetOnSuccessDelegate((Web _web, Response _response) =>
          {
              Debug.Log(_response.GetText());
              LoadingCanvas.Instance.HideLoadingPopUp();

              JSONNode node = JSONNode.Parse(_response.GetText());
              Debug.Log("Login Response: "+ node.ToString());
              if (node["status"].Value == ErrorCode.SUCCESS_CODE)
              {

                  Database.playerId = node["result"]["id"].Value;
                  Database.PutString(Database.Key.ACCESS_TOKEN, node["result"]["access_token"].Value);
                  Database.PutString(Database.Key.PLAYER_ID, node["result"]["id"].Value);
                  Database.PutString(Database.Key.SCORE, node["result"]["score"].Value);
                   Database.PutString(Database.Key.FIRST_NAME, node["result"]["name"].Value);
                   Database.PutBool(Database.Key.IS_LOGGEDIN, true);
                  // Database.PutString(Database.Key.LAST_NAME, node["result"]["last_name"].Value);
                

                  Database.PutString(Database.Key.IMAGE, node["result"]["image"].Value);


                  UIManager.instance.TransitionTo(UIPage.PageType.MAINMENU);
              }
              else if ((node["status"]).Value == ErrorCode.UNIQUIE_MOBILE)
              {
                  PopupCanvas.Instance.ShowAlertPopUp("Mobile Number already registered");
              }
              else if (node["status"].Value == ErrorCode.ERROR_LOGIN_GAME_RUNNING)
              {
                  PopupCanvas.Instance.ShowAlertPopUp(node["message"].Value);
#if _D_I
                  Debug.Log(node["message"].Value);
#endif
              }
              else
              {
                  PopupCanvas.Instance.ShowAlertPopUp(node["message"].Value);
              }

              _web.Close();

          })
          .SetOnFailureDelegate((Web _web, Response _response) =>
          {
              LoadingCanvas.Instance.HideLoadingPopUp();
              if (_response.GetText().Contains("check the connectivity"))
              {
                  PopupCanvas.Instance.ShowAlertPopUp("Please check your internet connection!");
              }
              else
              {
                  PopupCanvas.Instance.ShowAlertPopUp("Server not found!");
              }
              Debug.Log(_response.GetText());
              _web.Close();
          })
          .Connect();
    }

    public void RegisterClicked()
    {
        UIManager.instance.TransitionTo(PageType.REGISTER);
    }

    public void ForgetPasswordClicked()
    {

    }
}
