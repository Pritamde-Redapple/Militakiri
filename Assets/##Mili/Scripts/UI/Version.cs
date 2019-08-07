using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Version : MonoBehaviour
{
    public const string Game_Version = "1.0.4";
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Text>().text = Game_Version;
    }
   
}
