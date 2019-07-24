using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;


public class TestingJson : MonoBehaviour
{
    public string jsonString;
    public FirstPawnData firstPawnData;
    
    // Start is called before the first frame update
    void Start()
    {
        GridData from = new GridData("as", "sdsd", "1223");
        GridData to = new GridData("asda", "swsd", "4444");
        TurnData turnData = new TurnData(from, to);
        Debug.Log( JsonUtility.ToJson(turnData).ToString());
       // var
      //  newString = JSON.Parse(jsonString);
     //  firstPawnData = JsonUtility.FromJson<FirstPawnData>(newString["result"].ToString());

       
    }

    
}
