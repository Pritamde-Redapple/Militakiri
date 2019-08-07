using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;


public class TestingJson : MonoBehaviour
{
    public string jsonString;
    public FirstPawnData firstPawnData;
    // public TurnDataWrapper dataWrapper;
    // Start is called before the first frame update  
   
    void Start()
    {       
        var N = JSON.Parse(jsonString);
        Debug.Log(N["result"]["turnData"]);     
       


    }

    
}
