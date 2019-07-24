using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditModeFunctions : MonoBehaviour {

    [MenuItem("Window/Edit Mode Functions")]
    public static void ShowWindow()
    {
        BoardManager boardManager = FindObjectOfType<BoardManager>();
        boardManager.SetSquares();
    }

    [MenuItem("Window/Create Highlight Square")]
    public static void CreateHighlightSquare()
    {
        BoardManager boardManager = FindObjectOfType<BoardManager>();
        for (int i = 0; i < boardManager.squarePoints.Length; i++)
        {
            GameObject obj = Instantiate(boardManager.possibleMoveSqaure, boardManager.squarePoints[i].transform);
            obj.transform.SetAsFirstSibling();
            obj.transform.localPosition = new Vector3(0.002f, 0.026f, 0f);
            obj.transform.localScale = new Vector3(20.07f, 20.09f, 20.07f);
        }
    }
    [MenuItem("Window/Delete Highlight Square")]
    public static void DeleteAllSelectedSquares()
    {
        BoardManager boardManager = FindObjectOfType<BoardManager>();
        for (int i = 0; i < boardManager.squarePoints.Length; i++)
        {
            foreach (Transform t in boardManager.squarePoints[i].transform)
            {
                DestroyImmediate(t.gameObject);
            }
        }
    }

    [MenuItem("Window/Reset Highlight Square")]
    public static void ResetHighlightSquare()
    {
        BoardManager boardManager = FindObjectOfType<BoardManager>();
        for (int i = 0; i < boardManager.squarePoints.Length; i++)
        {
            boardManager.squarePoints[i].transform.GetChild(0).transform.localPosition = Vector3.zero;
        }
    }

    [MenuItem("Window/Create Local & Remote Pawn")]
    public static void CreateLocalAndRemotePawn()
    {
        BoardManager boardManager = FindObjectOfType<BoardManager>();
       // boardManager.CreateLocalAndRemotePawn();
    }


}
