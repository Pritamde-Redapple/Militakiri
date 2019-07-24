using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {

    #region Server Variables
    public static string SocketURL = "http://18.219.52.107:3009/";
    public static string PASSWORD = "password";
    public static string NAME = "name";
    public static string USER_NAME = "username";

    public static string TRUE = "true";
    public static string FALSE = "false";

    public static string defaultImagePath = "http://52.66.9.228:3000/images/avatar/defaultimage.png";

    #endregion

   

    public static bool isAI = true;
    public static int noOfSquarePerRow = 6;
    public static int threeRowsSetupTime = 30;
    public static int threeRowsSetupWarning = 15;
    public enum SquareState { MOVABLE, CLICKABLE, EMPTY, OCCUPIED, SELECTED, DESELECTED};
    public enum PlayerType
    {
        LOCAL,
        REMOTE,
        NONE
    };
    public enum BoardType
    {
        SINGLE,
        DOUBLE
    }
    public enum ViewType
    {
        TWO_D,
        THREE_D
    };
    public enum LevelType
    {
        EASY,
        MEDIUM,
        HARD
    }

    public enum PlayerTag
    {
        PLAYER_1,
        PLAYER_2
    }

    public static ViewType currentViewType;
    public static LevelType currentLevelType;
    public static BoardType currentBoardType;
    public static int TotalTurnPerBoard = 50;
    public static float PAWN_DUMP_SPEED = 0.8f;

    public static int GetMaximumRank(Pawn.PawnType type)
    {
        switch(type)
        {
            case Pawn.PawnType.STAR:
                return 4;
            default:
                return 3;
        }
    }

    public static int GetPercentageForAILevel()
    {
        switch (currentLevelType)
        {
            case LevelType.EASY:
                return 40;
            case LevelType.MEDIUM:
                return 15;
            case LevelType.HARD:
                return 0;
        }
        return 100;
    }
    public static int GetPercentageForDetectingOpponent()
    {
        switch (currentLevelType)
        {
            case LevelType.EASY:
                return 50;
            case LevelType.MEDIUM:
                return 75;
            case LevelType.HARD:
                return 100;
        }
        return 100;
    }

    public static int GetMaximumDistanceCanTowerPawnMove()
    {
        switch (currentLevelType)
        {
            case LevelType.EASY:
                return 15;
            case LevelType.MEDIUM:
                return 7;
            case LevelType.HARD:
                return 4;
        }
        return 4;
    }

    #region Multiplayer
    public enum GameEmits { adduser, gamerequest, playerReady, pawnplacement, leave_room, message_received, turnSubmited};

    #endregion

}
[Serializable]
public class PawnAndSquareMaterialInfo
{
    public List<int> indexforPawnAndSquare;

    public PawnAndSquareMaterialInfo()
    {
        indexforPawnAndSquare = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            indexforPawnAndSquare.Add(0);
        }
    }
}
[Serializable]
public class PawnMovementData
{
    public int fromSquareId;
    public List<int> fromSquarePawnId;
    public int toSquareId;
    public List<int> toSquarePawnId;
    public int removedPawnId;

    public PawnMovementData(int fromSquareId, List<int> fromSquarePawnId, int toSquareId, List<int> toSquarePawnId, int removedPawnId)
    {
        this.fromSquareId = fromSquareId;
        this.fromSquarePawnId = fromSquarePawnId;
        this.toSquareId = toSquareId;
        this.toSquarePawnId = toSquarePawnId;
        this.removedPawnId = removedPawnId;
    }
}
[Serializable]
public class SquareWithDistance
{
    public Square fromSquare;
    public Square square;
    public int distance;

    public SquareWithDistance(Square fromSquare,Square square, int distance)
    {
        this.fromSquare = fromSquare;
        this.square = square;
        this.distance = distance;
    }
}
[Serializable]
public class PossibleMoveData
{
    public List<SquareWithDistance> possibleMoves;
    public List<SquareWithDistance> possibleTakes;

    public PossibleMoveData()
    {
        possibleMoves = new List<SquareWithDistance>();
        possibleTakes = new List<SquareWithDistance>();
    }
}
[Serializable]
public class SquareWithTowerPawn
{
    public Pawn pawn;
    public Square square;
    public int distance;

    public SquareWithTowerPawn(Pawn pawn, Square square, int distance)
    {
        this.pawn = pawn;
        this.square = square;
        this.distance = distance;
    }
}

[Serializable]
public class PawnPlaced
{
    public List<int> positions = new List<int>();

    public void AddSquareID(int id)
    {
        positions.Add(id); ;
    }
}

//[Serializable]
//public class TurnData
//{
//    public int from;
//    public int to;

//    public TurnData(int from, int to)
//    {
//        this.from = from;
//        this.to = to;
//    }
//}

#region GameInitCallBack
/*
* {
"status":"1",
"message":"game inits",
"result":{
"turncount":50,
"room_id":"5cf65591328267444552b5bd",
"roomplayers":[
{
"user_id":"5cb42218a0f19e2d5a64af93",
"room_player_id":"MILT-5cb42218a0f19e2d5a64af93",
"room_player_name":"pritam"
},
{
"user_id":"5cf5228dbc3b52166dc6d1e7",
"room_player_id":"MILT-5cf5228dbc3b52166dc6d1e7",
"room_player_name":"a"
}
]
}
}
* 
* 
* */ 
#endregion
[Serializable]
public class GameInitInfo
{

}

[Serializable]
public struct PawnPlacements
{
    public string room_name;
    public string room_id;
    public string user_id;

    public List<GridData> GridDatas;  

    public PawnPlacements(string room_name, string room_id, string user_id, List<GridData> gridDatas)
    {
        this.room_name = room_name;
        this.room_id = room_id;
        this.user_id = user_id;
        GridDatas = gridDatas;
    }
}
[Serializable]
public struct GridData
{
    public string SquareID;
    public string PawnType;
    public string PawnRank;

    public GridData(string squareType, string pawnID, string pawnRank)
    {
        SquareID = squareType;
        PawnType = pawnID;
        PawnRank = pawnRank;
    }
}

[Serializable]
public struct FirstPawnData
{
   public PawnPlacements[] pawnlist;

    public FirstPawnData(PawnPlacements[] pawnlist)
    {
        this.pawnlist = pawnlist;
    }
}


[Serializable]
public struct TurnData
{
    public GridData from;
    public GridData to;

    public TurnData(GridData from, GridData to)
    {
        this.from = from;
        this.to = to;
    }
}








