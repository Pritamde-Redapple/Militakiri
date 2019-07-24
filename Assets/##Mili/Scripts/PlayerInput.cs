﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PlayerInput : MonoBehaviour {

    public float rayDistance;
    public LayerMask interactionLayer;
    BoardManager boardManager;

    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        GameManager.instance.currentGameState = GameManager.GAMESTATE.PLACE_PAWN;
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (!boardManager.canClick)
                return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit, rayDistance, interactionLayer))
            {
                if (GameManager.instance.currentGameState == GameManager.GAMESTATE.PLACE_PAWN)
                {
                    boardManager.PlacePawn(hit.collider.GetComponent<Square>());
                    
                }
                else if (GameManager.instance.currentGameState == GameManager.GAMESTATE.PLAY)
                {
                    boardManager.IsSquareSelected(hit.collider.GetComponent<Square>());
                    //if valid click return a callback
                }
            }
        }
    }
}