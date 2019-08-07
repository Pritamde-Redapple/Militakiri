using UnityEngine;
using System.Collections;

public class Database {

	//public static Database Instance;
    public static string playerId;

	void Awake(){
		//Instance = this;
	}
	public enum Key{
		EMAIL,
        PLAYER_ID,
        ACCESS_TOKEN,
        GAME_ID,
        CHIP_AMOUNT,
        GAME_COIN,
        SOUND,
        ROOM_NAME,
        ROOM_ID,
        ROOM_PLAYER_ID,
        ROUND_ID,
        APPOINTMENT_NO,
        IS_LOGGEDIN,
        IS_FB_LOGGEDIN,
        FIRST_NAME,
        LAST_NAME,
        IMAGE,
        AVATAR_ID,
        AVATAR_TYPE,
        MOBILE_NUMBER,
        XP_LEVEL,
        XP_POINT,
        USER_TYPE,
        PASSWORD,
        SCORE,
        OPPONENT_NAME
	};

	public static string GetString(Key key){
     //   Debug.Log("Trying to fetch key: "+ key.ToString());
#if !(UNITY_ANDROID || UNITY_IOS)
        return PlayerPrefs.GetString(key.ToString(),"").Remove(0, playerId.Length);
#else
         return PlayerPrefs.GetString(key.ToString(),"");
#endif
    }
    public static void PutString(Key key,string value){
#if !(UNITY_ANDROID || UNITY_IOS)
        PlayerPrefs.SetString (key.ToString(),  value);
#else
        PlayerPrefs.SetString (key.ToString(), value);
#endif
    }
    public static int GetInteger(Key key)
    {
        return PlayerPrefs.GetInt(key.ToString(), 0);
    }

    public static bool GetBool(Key key)
    {
        int value =  PlayerPrefs.GetInt(key.ToString(), 0);
        if (value == 0)
            return false;
        else
            return true;
    }
    public static void PutInteger(Key key, int value)
    {
        PlayerPrefs.SetInt(key.ToString(), value);
    }

    public static void PutBool(Key key, bool value)
    {
        int theRealValue= 0;
        if (value)
            theRealValue = 1;
        else
            theRealValue = 0;
        PlayerPrefs.SetInt(key.ToString(), theRealValue);
    }
  
    public static bool HasKey(Key key){
		return PlayerPrefs.HasKey (key.ToString());
	}

    public static void DeleteEverything()
    {
        PlayerPrefs.DeleteAll();
    }
}
