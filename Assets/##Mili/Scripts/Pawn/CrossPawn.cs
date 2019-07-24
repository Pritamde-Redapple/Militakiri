using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossPawn : Pawn {

    public override void ShowPossibleMoves()
    {
        base.ShowPossibleMoves();

        possibleMoveData = occupiedSquare.ShowDiagonalMoves(possibleMoveData);        
    }
}
