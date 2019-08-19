using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    public Constants.PlayerType currentPlayerType;

    public List<Square> firstThreeRowSquare;

    public Material pawnMaterial;

    public List<Pawn> availableSpareTower;

    public Transform allPawns;
    public Transform removedPawnParent3D;
    public Transform removedPawnParent2D;

    public Pawn currentSparePawn;

    private string playerName;
    private string profilePicURL;

    private int totalTurnLeft = -1;
    public Text totalTurnLeftText;

    public Player opponentPlayer;

    private int percentageForAICalculation;
    public int noOfTowerPawn = 3;

    private void Start()
    {
        //noOfTowerPawn = 3 * Constants.
        Pawn.OnTakeComplete += CheckAvailabilityOfSpareTower;
        GameManager.OnTurnChanged += OnTurnChanged;
        BoardManager.OnActiveEndRule += ActiveEndGameRule;
        BoardManager.OnDeactiveEndRule += DeactiveEndGameRule;
        percentageForAICalculation = Constants.GetPercentageForAILevel();
    }

    private void DeactiveEndGameRule()
    {
        if (totalTurnLeftText != null)
            totalTurnLeftText.transform.parent.gameObject.SetActive(false);
        totalTurnLeft = -1;
    }

    private void ActiveEndGameRule(Constants.PlayerType type)
    {
        if (totalTurnLeft != -1)
            return;
        totalTurnLeft = Constants.TotalTurnPerBoard;
        if (totalTurnLeftText != null)
        {
            totalTurnLeftText.transform.parent.gameObject.SetActive(true);
            totalTurnLeftText.text = totalTurnLeft.ToString() + " MOVES LEFT";
        }
    }

    private void OnTurnChanged(Constants.PlayerType pType, bool isEndRuleGameActivated)
    {
        if (pType == currentPlayerType && currentPlayerType == Constants.PlayerType.REMOTE && Constants.isAI)
        {
            Invoke("AICalculation", Random.Range(3, 5f));
        }
        if (isEndRuleGameActivated && pType == currentPlayerType)
        {
            totalTurnLeft--;
            if (totalTurnLeftText != null)
                totalTurnLeftText.text = totalTurnLeft.ToString() + " MOVES LEFT";
            if (totalTurnLeft == 0)
            {
                Debug.Log("Game should end");
                GameManager.instance.OnGameEnd(Constants.PlayerType.NONE);
            }
        }
    }

    private void OnDisable()
    {
        Pawn.OnTakeComplete -= CheckAvailabilityOfSpareTower;
        BoardManager.OnActiveEndRule -= ActiveEndGameRule;
        GameManager.OnTurnChanged -= OnTurnChanged;
        BoardManager.OnDeactiveEndRule -= DeactiveEndGameRule;
    }

    private void CheckAvailabilityOfSpareTower(Constants.PlayerType obj, Pawn pawn)
    {
        Debug.Log(obj.ToString() + " checking for spare tower");
        if (obj != currentPlayerType)
        {
            BoardManager.instance.gamePlay.SetDesriptionForPlacingPawn("Waiting for "+ Database.GetString(Database.Key.OPPONENT_NAME) + "to place spare tower");
            //BoardManager.instance.RemovePawnFromBoard(pawn);
            return;
        }
        for (int i = 0; i < availableSpareTower.Count; i++)
        {
            if (availableSpareTower[i].currentPawnType == pawn.currentPawnType)
            {
                Debug.Log("Can replace with spare tower");
                currentSparePawn = availableSpareTower[i];
                BoardManager.instance.RemovePawnFromBoard(pawn);
                pawn.occupiedSquare.occupiedPawn = null;
                pawn.occupiedSquare = null;
                ReplacePawnWithSpareTower(pawn, availableSpareTower[i]);
                availableSpareTower.RemoveAt(i);
                return;
            }
        }
        //HERE CHANGE PLAYER TURN
        GameManager.instance.IncreaseTurn();
    }

    void ReplacePawnWithSpareTower(Pawn pawn, Pawn spareTower)
    {
        // First Check Available Space in first 3 row
        List<Square> availableSquare = CheckAvailableSpaceInMyPlace();
        if (availableSquare.Count > 0)
        {
            //PLACE SPARE TOWER IN HIGHLIGHTED PLACE
            BoardManager.instance.gamePlay.SetDesriptionForPlacingPawn("Select highlighted square to place SPARE tower");

            if (Constants.isAI && currentPlayerType == Constants.PlayerType.REMOTE)
            {
                Debug.Log("AI places spare tower automatically ");
                int squareIndex = Random.Range(0, availableSquare.Count);
                BoardManager.instance.PlaceSparePawn(this, availableSquare[squareIndex]);
            }
        }
        else
        {
            //CHANGE PLAYER TURN AND WAIT FOR AVAILABLE SPACE
            BoardManager.instance.playerToPlaceSpareTower = this;
            GameManager.instance.IncreaseTurn();
        }
    }
    // RETURN AN EMPTY SQUARE AND HIGHLIGHT IT
    List<Square> CheckAvailableSpaceInMyPlace()
    {
        List<Square> availableSquare = new List<Square>();
        for (int i = 0; i < firstThreeRowSquare.Count; i++)
        {
            if (firstThreeRowSquare[i].occupiedPawn == null)
            {
                availableSquare.Add(firstThreeRowSquare[i]);
                firstThreeRowSquare[i].SetHighlightStatus(true);
            }
        }
        return availableSquare;
    }
    //PLACES A SPARE TOWER ON A EMPTY SQUARE OF THE PLAYER
    public void PlaceStoredSpareTower()
    {
        List<Square> availableSquare = CheckAvailableSpaceInMyPlace();
        if (availableSquare.Count > 0)
        {
            BoardManager.instance.PlaceSparePawn(this, availableSquare[0]);
        }
    }

    public void SetParameter(string playerName, string profilePicURL)
    {

    }
    //GET AVAILABLE TOWER PAWN
    public int GetNoOfTowerPawnAvailable()
    {
        noOfTowerPawn = 0;
        for (int i = 0; i < allPawns.childCount; i++)
        {
            if (allPawns.GetChild(i).GetComponent<Pawn>().Rank > 5)
                noOfTowerPawn++;
        }
        return noOfTowerPawn;
    }

    public List<SquareWithDistance> allPossobleMoves;
    public List<SquareWithDistance> allpossibleTakes;

    public void CalculateAllPossibility()
    {
        for (int i = 0; i < allPawns.childCount; i++)
        {
            allPawns.GetChild(i).GetComponent<Pawn>().ShowPossibleMoves();
        }

        allPossobleMoves = new List<SquareWithDistance>();
        allpossibleTakes = new List<SquareWithDistance>();

        for (int i = 0; i < allPawns.childCount; i++)
        {
            if (allPawns.GetChild(i).GetComponent<Pawn>().possibleMoveData != null)
            {
                if (allPawns.GetChild(i).GetComponent<Pawn>().possibleMoveData.possibleMoves.Count > 0)
                {
                    allPossobleMoves = allPossobleMoves.Concat(allPawns.GetChild(i).GetComponent<Pawn>().possibleMoveData.possibleMoves).ToList();
                }
                if (allPawns.GetChild(i).GetComponent<Pawn>().possibleMoveData.possibleTakes.Count > 0)
                {
                    allpossibleTakes = allpossibleTakes.Concat(allPawns.GetChild(i).GetComponent<Pawn>().possibleMoveData.possibleTakes).ToList();
                }
            }
        }
    }


    public List<SquareWithDistance> PossibleTakingTowerPawn()
    {
        List<SquareWithDistance> possibleTakingTowerPawn = new List<SquareWithDistance>();
        for (int i = 0; i < allpossibleTakes.Count; i++)
        {
            if (allpossibleTakes[i].square.occupiedPawn.Rank > 5)
            {
                possibleTakingTowerPawn.Add(allpossibleTakes[i]);
            }
        }
        return possibleTakingTowerPawn;
    }

    void AICalculation()
    {
        if (GameManager.instance.currentGameState == GameManager.GAMESTATE.END)
            return;
        // Calculate all possibility of player it self
        CalculateAllPossibility();

        // Calculate all possibility of opponent player
        opponentPlayer.CalculateAllPossibility();
        //Check whether opponent player will able to take my tower pawn
        List<SquareWithDistance> possibleTakingTowerPawnByOpponent = opponentPlayer.PossibleTakingTowerPawn();

        // Debug.Log(" possibleTakingTowerPawnByOpponent.Count : " + possibleTakingTowerPawnByOpponent.Count);
        if (possibleTakingTowerPawnByOpponent.Count > 0)
        {
            int percentage = Random.Range(0, 100);
            if (percentage <= Constants.GetPercentageForDetectingOpponent())
            {
                int index = Random.Range(0, possibleTakingTowerPawnByOpponent.Count);
                Square squareToMove = possibleTakingTowerPawnByOpponent[index].square;
                // First check whether this pawn can take opponent Tower pawn
                if (squareToMove.CanTakeSquare(possibleTakingTowerPawnByOpponent[index].fromSquare))
                {
                    squareToMove.MovePawn(possibleTakingTowerPawnByOpponent[index].fromSquare);
                    Debug.Log("Take opponent one");
                    return;
                }
                else
                {
                    // Check whether any other pawn of mine can take the pawn who can take my tower pawn (squareToMove)
                    Square squareToTake = possibleTakingTowerPawnByOpponent[index].fromSquare;
                    List<SquareWithDistance> allMoves = allpossibleTakes.FindAll(s => s.square == squareToTake);

                    if (allMoves.Count > 0)
                    {
                        int i = Random.Range(0, allMoves.Count);
                        allMoves[i].fromSquare.MovePawn(allMoves[i].square);
                        Debug.Log("Other pawn take that pawn................");
                        return;
                    }
                    else
                    {
                        List<SquareWithDistance> moves = allPossobleMoves.FindAll(s => s.fromSquare == squareToMove);
                        Debug.Log("Moves count : " + moves.Count + " Id : " + squareToMove);
                        if (moves.Count > 0)
                        {
                            index = Random.Range(0, moves.Count);
                            moves[index].fromSquare.MovePawn(moves[index].square);
                            return;
                        }
                    }
                }
            }

        }

        int maxDistance = Constants.GetMaximumDistanceCanTowerPawnMove();
        if ((opponentPlayer.noOfTowerPawn == 1 || noOfTowerPawn == 1) && Constants.currentLevelType == Constants.LevelType.HARD)
            maxDistance = 15;
        allPossobleMoves.RemoveAll(pawn => pawn.distance > maxDistance);
        allpossibleTakes.RemoveAll(pawn => pawn.distance > maxDistance);

        // Debug.Log("noOfTowerPawn : " + noOfTowerPawn + " maxDistance :" + maxDistance + " LevelType : " + Constants.currentLevelType);

        // Possiblity to take opponent player tower pawn
        List<SquareWithDistance> allTakeableTowerPawn = new List<SquareWithDistance>();
        for (int i = 0; i < allpossibleTakes.Count; i++)
        {
            if (allpossibleTakes[i].square.occupiedPawn.Rank > 5)
            {
                allTakeableTowerPawn.Add(allpossibleTakes[i]);
            }
        }

        if (allTakeableTowerPawn.Count > 0)
        {
            int percentage = Random.Range(0, 100);
            // Debug.Log(percentage);
            if (percentage < (percentageForAICalculation))
            {
                int index = Random.Range(0, allTakeableTowerPawn.Count);
                allTakeableTowerPawn[index].fromSquare.MovePawn(allTakeableTowerPawn[index].square);

                Debug.Log("allTakeableTowerPawn.Count : " + allTakeableTowerPawn.Count + " index :  " + index);

            }
            else
            {

                PossibilityToMakeTowerPawn();
            }
        }
        else
        {
            PossibilityToMakeTowerPawn();
        }

    }

    void PossibilityToMakeTowerPawn()
    {
        // Possiblity to make tower pawn
        List<SquareWithDistance> pawnToMakeTower = new List<SquareWithDistance>();
        for (int i = 0; i < allpossibleTakes.Count; i++)
        {
            if (allpossibleTakes[i].square.occupiedPawn.Rank < 5 &&
                allpossibleTakes[i].fromSquare.occupiedPawn.Rank < 5)
            {
                pawnToMakeTower.Add(allpossibleTakes[i]);
            }
        }
        if (pawnToMakeTower.Count > 0)
        {
            int i = Random.Range(0, pawnToMakeTower.Count);
            pawnToMakeTower[i].fromSquare.MovePawn(pawnToMakeTower[i].square);
            Debug.Log("To make tower pawn");
        }
        else
        {
            AI(40);
        }
    }
    public void AI(int difference = 0)
    {
        int percentage = Random.Range(0, 100);
        //  Debug.Log(percentage);
        if (percentage < (percentageForAICalculation + difference))
        {
            // SIMPLE MOVE
            AISimpleMove();
        }
        else
        {
            //Take
            if (allpossibleTakes.Count > 0)
                AITake();
            else
                AISimpleMove();
        }
    }

    void AISimpleMove()
    {
        int pawnIndex = Random.Range(0, allPossobleMoves.Count);
        allPossobleMoves[pawnIndex].fromSquare.MovePawn(allPossobleMoves[pawnIndex].square);

        //  Debug.Log("allPossobleMoves.Count : " + allPossobleMoves.Count + " pawnIndex :  " + pawnIndex);
    }

    void AITake()
    {
        int pawnIndex = Random.Range(0, allpossibleTakes.Count);
        allpossibleTakes[pawnIndex].fromSquare.MovePawn(allpossibleTakes[pawnIndex].square);
        Debug.Log("allpossibleTakes.Count : " + allpossibleTakes.Count + " pawnIndex :  " + pawnIndex);
    }
}
