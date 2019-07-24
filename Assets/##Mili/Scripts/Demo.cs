using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour {

    public bool isAI = true;
    public Constants.LevelType difficultyLevel;
    private void Awake()
    {
        Constants.isAI = isAI;
        Constants.currentLevelType = difficultyLevel;
    }
}
