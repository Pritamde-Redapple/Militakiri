using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlusPawn : Pawn {

    public override void ShowPossibleMoves()
    {

        base.ShowPossibleMoves();

        possibleMoveData = occupiedSquare.ShowStraightForwardMoves(possibleMoveData);
    }
}
