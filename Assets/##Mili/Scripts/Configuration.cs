using UnityEngine;
using System.Collections;
using SimpleJSON;
public class Configuration : MonoBehaviour {

	public enum ApiKey{
		REGISTRATION,
		LOGIN,
        GUEST_LOGIN,
        FORGOT_PASSWORD,
        CHECK_OTP,
        RESET_PASSWORD,
        LOGOUT,
        GAME_LIST,
        LEADERBOARD,
        PROFILE_DETAILS,
        UPDATE_EMAIL,
        AMOUNT_LIST,
        UPDATE_PROFILE,
        PASSWORD_UPDATE,
        CURRENT_BALANCE,
        SOCIAL_LOGIN,
        SEND_GIFT,
        REDEEM,
        AVATAR_LIST,
        IMAGE_UPDATE,
        BONUS_TIME,
        BONUS_COLLECT,
        BUY_CHIP,
        CREATE_TABLE,
        JOIN_TABLE,
        JOINED_TABLE_LIST,
        DELETE_TABLE
    }

    bool isLoggedIn;

	JSONNode configuration;
	public static Configuration Instance;
	void Awake(){
		Instance = this;
		configuration = JSONNode.Parse ((Resources.Load("Configuration/Configuration")as TextAsset).text);
        //  GLog.Log("The Registration APi........."+GetApi(ApiKey.SIGN_UP));
        isLoggedIn = GetLoginStatus();
    }


	public string GetApi(ApiKey apiKey){
        return configuration["API"]["DOMAIN"].Value + configuration["API"][apiKey.ToString()].Value;
	}
    public string GetDomainUrl()
    {
        return configuration["API"]["DOMAIN"].Value;
    }

    public bool GetLoginStatus()
    {
        return isLoggedIn;
    }

    public void SetLoginStatus(bool s)
    {
        isLoggedIn = s;
    }


}
