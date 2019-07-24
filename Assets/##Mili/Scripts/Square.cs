using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour {

   //[HideInInspector]
    public int squareId;
    public string squareId2;
    Constants.SquareState myState;

    public int SquareId
    {
        get
        {
            return squareId;
        }

        set
        {
            squareId = value;
            FindStraightForwardSquare();
            FindDiagonalSquare();
            
        }
    }

    public List<Square> leftSquares;
    public List<Square> rightSquares;
    public List<Square> upSquares;
    public List<Square> downSquares;

    public List<Square> leftUpDiagonalSquares;
    public List<Square> rightUpDiagonalSquares;
    public List<Square> leftDownDiagonalSquares;
    public List<Square> rightDownDiagonalSquares;

    public Pawn occupiedPawn;
    public bool isHighlighted;

    private GameObject highlightedSquare;
    private Renderer highlightedSquareRenderer;

    public Material redMaterial;
    public Material yellowMaterial;

    private void Awake()
    {
        highlightedSquare = transform.GetChild(0).gameObject;
        highlightedSquareRenderer = highlightedSquare.GetComponent<Renderer>();
    }
    void FindStraightForwardSquare()
    {
        leftSquares.Clear();
        int id = SquareId;
        while ((id % Constants.noOfSquarePerRow != 0))
        {
            GameObject obj = GameObject.Find("Square_" + (id - 1));
            if (obj != null) // Left
            {
                leftSquares.Add(obj.GetComponent<Square>());
            }
            id --;
        }

        rightSquares.Clear();
        id = SquareId;
        while ((id % Constants.noOfSquarePerRow != Constants.noOfSquarePerRow - 1))
        {
            GameObject obj = GameObject.Find("Square_" + (id + 1));
            if (obj != null) // Left
            {
                rightSquares.Add(obj.GetComponent<Square>());
            }
            id++;
        }

        upSquares.Clear();
        id = SquareId;
        while (true)
        {
            id-= Constants.noOfSquarePerRow;
            GameObject obj = GameObject.Find("Square_" + (id));
            if (obj != null) // Left
            {
                upSquares.Add(obj.GetComponent<Square>());
            }
            else
                break;
           
        }

        downSquares.Clear();
        id = SquareId;
        while (true)
        {
            id += Constants.noOfSquarePerRow;
            GameObject obj = GameObject.Find("Square_" + (id));
            if (obj != null) // Left
            {
                downSquares.Add(obj.GetComponent<Square>());
            }
            else
                break;

        }

    }

    void FindDiagonalSquare()
    {
        leftUpDiagonalSquares.Clear();
        int id = SquareId;
        while ((id % Constants.noOfSquarePerRow != 0))
        {
            id = id - 1 - Constants.noOfSquarePerRow;
            GameObject obj = GameObject.Find("Square_" + (id));
            if (obj != null) // Left Up
            {
                leftUpDiagonalSquares.Add(obj.GetComponent<Square>());
            }
            else
                break;
            
        }

        rightUpDiagonalSquares.Clear();
        id = SquareId;
        while ((id % Constants.noOfSquarePerRow != Constants.noOfSquarePerRow - 1))
        {
            id = id + 1 - Constants.noOfSquarePerRow;
            GameObject obj = GameObject.Find("Square_" + (id));
            if (obj != null) // Left
            {
                rightUpDiagonalSquares.Add(obj.GetComponent<Square>());
            }
            else
                break;
            
        }

        leftDownDiagonalSquares.Clear();
        id = SquareId;
        while ((id % Constants.noOfSquarePerRow != 0))
        {
            id = id - 1 + Constants.noOfSquarePerRow;
            GameObject obj = GameObject.Find("Square_" + (id));
            if (obj != null) // Left
            {
                leftDownDiagonalSquares.Add(obj.GetComponent<Square>());
            }
            else
                break;

        }

        rightDownDiagonalSquares.Clear();
        id = SquareId;
        while ((id % Constants.noOfSquarePerRow != Constants.noOfSquarePerRow - 1))
        {
            id = id + 1 + Constants.noOfSquarePerRow;
            GameObject obj = GameObject.Find("Square_" + (id));
            if (obj != null) // Left
            {
                rightDownDiagonalSquares.Add(obj.GetComponent<Square>());
            }
            else
                break;

        }

    }


    public void ShowPossibleMoves()
    {
        occupiedPawn.ShowPossibleMoves();
    }

    public void ShowPossibleMovesOnClick()
    {
        occupiedPawn.ShowPossibleMovesOnClick();
    }

    public PossibleMoveData ShowStraightForwardMoves(PossibleMoveData possibleMoveData)
    {
        int rank = occupiedPawn.rankForMoveAndTake;

         possibleMoveData = CheckPossibleMoveForOneSide(leftSquares, rank, possibleMoveData);
         possibleMoveData = CheckPossibleMoveForOneSide(rightSquares, rank, possibleMoveData);
         possibleMoveData = CheckPossibleMoveForOneSide(upSquares, rank, possibleMoveData);
         possibleMoveData = CheckPossibleMoveForOneSide(downSquares, rank, possibleMoveData);
        

         possibleMoveData = CheckPossibleMoveForOneSideTaking(leftUpDiagonalSquares, rank, possibleMoveData, true);
         possibleMoveData = CheckPossibleMoveForOneSideTaking(rightUpDiagonalSquares, rank, possibleMoveData, true);
         possibleMoveData = CheckPossibleMoveForOneSideTaking(leftDownDiagonalSquares, rank, possibleMoveData, true);
         possibleMoveData = CheckPossibleMoveForOneSideTaking(rightDownDiagonalSquares, rank, possibleMoveData, true);

        return possibleMoveData;
    }

    public PossibleMoveData ShowDiagonalMoves(PossibleMoveData possibleMoveData)
    {
        if(occupiedPawn == null)
        {
            Debug.Log("Null found" + gameObject.name);
            return possibleMoveData;
        }
        int rank = occupiedPawn.rankForMoveAndTake;

       possibleMoveData = CheckPossibleMoveForOneSide(leftUpDiagonalSquares, rank, possibleMoveData);
       possibleMoveData = CheckPossibleMoveForOneSide(rightUpDiagonalSquares, rank, possibleMoveData);
       possibleMoveData = CheckPossibleMoveForOneSide(leftDownDiagonalSquares, rank, possibleMoveData);
       possibleMoveData = CheckPossibleMoveForOneSide(rightDownDiagonalSquares, rank, possibleMoveData);

       possibleMoveData = CheckPossibleMoveForOneSideTaking(leftSquares, rank, possibleMoveData, true);
       possibleMoveData = CheckPossibleMoveForOneSideTaking(rightSquares, rank, possibleMoveData, true);
       possibleMoveData = CheckPossibleMoveForOneSideTaking(upSquares, rank, possibleMoveData, true);
       possibleMoveData = CheckPossibleMoveForOneSideTaking(downSquares, rank, possibleMoveData, true);

       return possibleMoveData;
    }

   
    private PossibleMoveData CheckPossibleMoveForOneSide(List<Square> squareList, int rank, PossibleMoveData possibleMoveData)
    {
        for (int i = 0; i < squareList.Count; i++)
        {
            if (CheckPossibleMove(rank, i, squareList))
            {
              // squareList[i].SetHighlightStatus(true);
              int distance = (SquareId - squareList[i].SquareId) / Constants.noOfSquarePerRow;
              possibleMoveData.possibleMoves.Add(new SquareWithDistance(this,squareList[i],Mathf.Abs(distance)));
            }
            else
                break;
        }
        return possibleMoveData;
    }


    bool CheckPossibleMove(int rank, int index, List<Square> squareList)
    {
        if (rank > index)
        {
           if( squareList[index].occupiedPawn == null)
                return true;
        }
        return false;
    }


    bool canCheckForward = true;
    private PossibleMoveData CheckPossibleMoveForOneSideTaking(List<Square> squareList, int rank, PossibleMoveData possibleMoveData, bool canTake = false)
    {
        canCheckForward = true;
        for (int i = 0; i < squareList.Count; i++)
        {
            if (canCheckForward)
            {
                if (CheckPossibleMoveForTaking(rank, i, squareList, canTake))
                {
                    //squareList[i].SetHighlightStatus(true);
                    int distance = (SquareId - squareList[i].SquareId) / Constants.noOfSquarePerRow;
                    possibleMoveData.possibleTakes.Add(new SquareWithDistance(this,squareList[i], Mathf.Abs(distance)));
                   // possibleMoveData.possibleTakes.Add(squareList[i]);
                }
            }
            else
                break;
        }

        return possibleMoveData;
    }


    bool CheckPossibleMoveForTaking(int rank, int index, List<Square> squareList, bool canTake)
    {
        if (rank > index)
        {
            if (squareList[index].occupiedPawn != null)
            {
                if (occupiedPawn.currentPlayerType == squareList[index].occupiedPawn.currentPlayerType)
                {
                    canCheckForward = false;
                    return false;
                }
                else
                {
                    canCheckForward = false;
                    return true;
                }
            }
        }
        return false;
    }

    public void SetHighlightStatus(bool value)
    {
        highlightedSquare.SetActive(value);
        isHighlighted = value;

        if (!value)
            return;
        if(occupiedPawn == null)
        {
            highlightedSquareRenderer.material = redMaterial;
        }
        else
        {
            highlightedSquareRenderer.material = yellowMaterial;
        }
    }


    public void MovePawn(Square targetSquare)
    {
        if(!Constants.isAI)
        {
           // SocketController.instance.SubmitTurn(this, targetSquare);
        }
        occupiedPawn.MovePawn(targetSquare);
    }

    public bool CanTakeSquare(Square square)
    {
        if(occupiedPawn != null)
        {
            SquareWithDistance swd = occupiedPawn.possibleMoveData.possibleTakes.Find(s => (s.square == square && s.fromSquare == this));
            if (swd == null)
                return false;
            else
                return true;
        }
        return false;
    }
}
