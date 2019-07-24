using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarPawn : Pawn {

    public override void ShowPossibleMoves()
    {
        base.ShowPossibleMoves();

        possibleMoveData = occupiedSquare.ShowDiagonalMoves(possibleMoveData);
        possibleMoveData = occupiedSquare.ShowStraightForwardMoves(possibleMoveData);
    }
}
