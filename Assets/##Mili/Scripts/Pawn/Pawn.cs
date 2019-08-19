using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Pawn : MonoBehaviour
{

    public int pawnId;

    public int PawnId
    {
        get
        {
            return pawnId;
        }

        set
        {
            pawnId = value;
        }
    }



    public enum PawnType
    {
        CROSS,
        PLUS,
        STAR
    }



    public Constants.PlayerType currentPlayerType;
    public PawnType currentPawnType;

    [SerializeField]
    private int rank = 1;
    public int Rank
    {
        get
        {
            return rank;
        }

        set
        {
            rank = value;
            if (!isEndRuleActivated)
                rankForMoveAndTake = value;
        }
    }
    public int rankForMoveAndTake;

    public Square occupiedSquare;

    public PossibleMoveData possibleMoveData;

    private Transform meshComponent;
    private Image rankImage;

    private bool isEndRuleActivated = false;

    public static event Action<Constants.PlayerType, Pawn> OnTakeComplete;
    private void Awake()
    {
        rankForMoveAndTake = rank;
        meshComponent = transform.GetChild(0);
        rankImage = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        GamePlay.OnViewTypeChange += OnViewTypeChange;
        BoardManager.OnActiveEndRule += ActiveEndGameRule;
        BoardManager.OnDeactiveEndRule += DeactiveEndGameRule;

    }

    private void Start()
    {
        //Rank = rank;
        OnViewTypeChange(Constants.currentViewType);
    }

    private void DeactiveEndGameRule()
    {
        rankForMoveAndTake = rank;
        isEndRuleActivated = false;
    }

    public void ActiveEndGameRule(Constants.PlayerType type)
    {
        if (rank > 5 && currentPlayerType == type)
        {
            rankForMoveAndTake = 1;
            isEndRuleActivated = true;
        }
    }

    private void OnDestroy()
    {
        GamePlay.OnViewTypeChange -= OnViewTypeChange;
        BoardManager.OnActiveEndRule -= ActiveEndGameRule;
        BoardManager.OnDeactiveEndRule -= DeactiveEndGameRule;
    }

    private void OnViewTypeChange(Constants.ViewType obj)
    {
        rankImage.transform.parent.gameObject.SetActive(!(obj == Constants.ViewType.THREE_D));
    }

    public virtual void ShowPossibleMoves()
    {
        possibleMoveData = new PossibleMoveData();
        if (Rank > 1 && Rank < 5)
        {
            for (int i = 1; i < transform.childCount - 1; i++)
            {
                if (transform.GetChild(i).GetComponent<Pawn>().currentPawnType == PawnType.PLUS)
                {
                    possibleMoveData = occupiedSquare.ShowStraightForwardMoves(possibleMoveData);
                }
                else if (transform.GetChild(i).GetComponent<Pawn>().currentPawnType == PawnType.CROSS)
                {
                    possibleMoveData = occupiedSquare.ShowDiagonalMoves(possibleMoveData);
                }
                else if (transform.GetChild(i).GetComponent<Pawn>().currentPawnType == PawnType.STAR)
                {
                    possibleMoveData = occupiedSquare.ShowStraightForwardMoves(possibleMoveData);
                    possibleMoveData = occupiedSquare.ShowDiagonalMoves(possibleMoveData);
                }
            }
        }
    }

    public void ShowPossibleMovesOnClick()
    {

        ShowPossibleMoves();
        ShowHighlightedSquare();
    }

    public void ShowHighlightedSquare()
    {
        if (possibleMoveData != null)
        {
            for (int i = 0; i < possibleMoveData.possibleMoves.Count; i++)
            {
                possibleMoveData.possibleMoves[i].square.SetHighlightStatus(true);
            }
            for (int i = 0; i < possibleMoveData.possibleTakes.Count; i++)
            {
                possibleMoveData.possibleTakes[i].square.SetHighlightStatus(true);
            }
        }
    }

    public virtual void MovePawn(Square targetSquare)
    {    // SIMLE MOVE
        if (targetSquare.occupiedPawn == null)
        {


            // SIMLE MOVE
            transform.DOMove(targetSquare.transform.position, 1f)
            .OnComplete(() =>
            {
                //Set Data for Multiplayer

                GridData from = new GridData(occupiedSquare.squareId2, "", "");

                GridData to = new GridData(targetSquare.squareId2, currentPawnType.ToString(), rank.ToString());

                if (GameManager.instance.currentPlayerTurn == Constants.PlayerType.LOCAL)
                    SocketController.instance.PrepareTurnData(from, to);
                else
                    SocketController.instance.FinishedTurn();

                occupiedSquare.occupiedPawn = null;
                targetSquare.occupiedPawn = this;
                occupiedSquare = targetSquare;
                BoardManager.instance.ResetAllSquare();
                BoardManager.instance.OnMovementComplete();
                GameManager.instance.IncreaseTurn();
            });



        }
        //SIMPLE MOVE AND REMOVE TARGET PAWN
        else
        {


            if ((Rank < 5 && targetSquare.occupiedPawn.Rank > 5) || (Rank > 5) ||
                (Rank == Constants.GetMaximumRank(currentPawnType)))
            {





                //SIMPLE MOVE AND REMOVE TARGET PAWN

                transform.DOMove(targetSquare.transform.position, 1f)
                .OnComplete(() =>
                {
                    //Set Data for Multiplayer
                    GridData from = new GridData(occupiedSquare.squareId2, "", "");

                    GridData to = new GridData(targetSquare.squareId2, currentPawnType.ToString(), rank.ToString());


                    if (GameManager.instance.currentPlayerTurn == Constants.PlayerType.LOCAL)
                        SocketController.instance.PrepareTurnData(from, to);
                    else
                        SocketController.instance.FinishedTurn();


                    BoardManager.instance.RemovePawnFromBoard(targetSquare.occupiedPawn);

                    occupiedSquare.occupiedPawn = null;
                    targetSquare.occupiedPawn = this;
                    occupiedSquare = targetSquare;
                    BoardManager.instance.ResetAllSquare();
                    BoardManager.instance.OnMovementComplete();
                    GameManager.instance.IncreaseTurn();
                });
            }
            else if (Rank == targetSquare.occupiedPawn.Rank && Rank != 1)
            {
                // IF Rank 2 pawn of upper shape star take another pawn of rank 2, then rank will be 3 and lower pawn of taken pawn will be removed

                //Set Data for Multiplayer
                GridData from = new GridData(occupiedSquare.squareId2, "", "");

                string pawnTypes = "" + currentPawnType.ToString() + "," + targetSquare.occupiedPawn.currentPawnType.ToString();

                //GridData to = new GridData(targetSquare.squareId2, currentPawnType.ToString(), rank.ToString());

                //SocketController.instance.PrepareTurnData(from, to);


                transform.DOMove(targetSquare.transform.position, 1f)
                     .OnComplete(() =>
                     {
                         occupiedSquare.occupiedPawn = null;

                         targetSquare.occupiedPawn.transform.SetParent(transform);
                         for (int i = targetSquare.occupiedPawn.transform.childCount - 2; i > 0; i--)
                         {
                             targetSquare.occupiedPawn.transform.GetChild(i).SetParent(transform);
                         }
                         Rank += targetSquare.occupiedPawn.Rank;
                         Rank = Mathf.Clamp(Rank, 0, Constants.GetMaximumRank(currentPawnType));

                         UpdateRankImage(Rank, false);

                         for (int i = transform.childCount - 2; i > 0; i--)
                         {
                             if (i >= Rank)
                             {
                                 Pawn pawn = transform.GetChild(i).GetComponent<Pawn>();
                                 pawn.SetMeshPosition(1);
                                 BoardManager.instance.RemovePawnFromBoard(pawn);
                             }
                             else
                                 break;
                         }

                         SetMeshPosition(Rank);

                         for (int i = 1; i < transform.childCount - 1; i++)
                         {
                             transform.GetChild(i).GetComponent<Pawn>().SetMeshPosition(Rank - i);
                         }
                         meshComponent.localPosition = new Vector3(0, 0, (Rank - 1) * 0.0013f);
                         occupiedSquare = targetSquare;
                         targetSquare.occupiedPawn = this;
                         BoardManager.instance.ResetAllSquare();
                         BoardManager.instance.OnMovementComplete();

                         if (OnTakeComplete != null)
                         {
                             OnTakeComplete(currentPlayerType, this);
                             string newPawnID2 = currentPawnType.ToString();
                             GridData to2 = new GridData(occupiedSquare.squareId2, newPawnID2, Rank.ToString());
                             if (GameManager.instance.currentPlayerTurn == Constants.PlayerType.LOCAL)
                             {
                                 SocketController.instance.PrepareTurnData(from, to2, TurnType.sparePawn.ToString());
                                 return;
                             }
                         }
                         else
                             GameManager.instance.IncreaseTurn();

                         string newPawnID = currentPawnType.ToString();
                         foreach (Transform item in transform)
                         {
                            // newPawnID = "," + item.GetComponent<Pawn>().currentPawnType.ToString();
                         }

                         GridData to = new GridData(occupiedSquare.squareId2, newPawnID, Rank.ToString());
                         if (GameManager.instance.currentPlayerTurn == Constants.PlayerType.LOCAL)
                             SocketController.instance.PrepareTurnData(from, to, TurnType.sparePawn.ToString());
                         else
                             SocketController.instance.FinishedTurn();
                     });
            }
            else if (Rank >= targetSquare.occupiedPawn.Rank && Rank < 5)
            {
                // TAKE
                Take(targetSquare);
            }
            else if (Rank <= targetSquare.occupiedPawn.Rank && Rank < 5)
            {
                // DEGRADE Rank
                Degrade(targetSquare);
            }
        }
    }

    private void Degrade(Square targetSquare)
    {
        GridData from = new GridData(occupiedSquare.squareId2, "", "");
        for (int i = 0; i < Rank; i++)
        {
            BoardManager.instance.RemovePawnFromBoard(targetSquare.occupiedPawn.transform.GetChild(targetSquare.occupiedPawn.transform.childCount - 2).GetComponent<Pawn>());
        }
        occupiedSquare.occupiedPawn = null;

        targetSquare.occupiedPawn.Rank -= Rank;

        targetSquare.occupiedPawn.SetMeshPosition(targetSquare.occupiedPawn.Rank, false);
        for (int i = 1; i < targetSquare.occupiedPawn.transform.childCount - 1; i++)
        {
            targetSquare.occupiedPawn.transform.GetChild(i).GetComponent<Pawn>().SetMeshPosition(targetSquare.occupiedPawn.Rank - i);
        }

        #region MultiplayerData
        string newPawnID = currentPawnType.ToString();
        foreach (Transform item in transform)
        {
           // newPawnID = "," + item.GetComponent<Pawn>().currentPawnType.ToString();
        }
        GridData to = new GridData(targetSquare.squareId2, newPawnID, Rank.ToString());
        if (GameManager.instance.currentPlayerTurn == Constants.PlayerType.LOCAL)
            SocketController.instance.PrepareTurnData(from, to);
        else
            SocketController.instance.FinishedTurn();
        #endregion

        occupiedSquare = null;
        BoardManager.instance.RemovePawnFromBoard(this);
        BoardManager.instance.ResetAllSquare();
        BoardManager.instance.OnMovementComplete();
        GameManager.instance.IncreaseTurn();
    }

    private void Take(Square targetSquare)
    {

        GridData from = new GridData(occupiedSquare.squareId2, "", "");
        // Debug.Log("Now Take");
        transform.DOMove(targetSquare.transform.position, 1f)
                 .OnComplete(() =>
                 {
                     occupiedSquare.occupiedPawn = null;

                     targetSquare.occupiedPawn.transform.SetParent(transform);
                     Rank++;


                     SetMeshPosition(Rank, false);
                     for (int i = 1; i < transform.childCount - 1; i++)
                     {
                         transform.GetChild(i).GetComponent<Pawn>().SetMeshPosition(Rank - i);
                     }
                     meshComponent.localPosition = new Vector3(0, 0, (Rank - 1) * 0.0013f);
                     occupiedSquare = targetSquare;
                     targetSquare.occupiedPawn = this;
                     BoardManager.instance.ResetAllSquare();
                     BoardManager.instance.OnMovementComplete();

                     if (Constants.GetMaximumRank(currentPawnType) <= Rank && OnTakeComplete != null)
                     {
                         Debug.Log("Call once ");
                         OnTakeComplete(currentPlayerType, this);
                         string newPawnID2 = currentPawnType.ToString();

                         GridData to2 = new GridData(targetSquare.squareId2, newPawnID2, Rank.ToString());
                         if (GameManager.instance.currentPlayerTurn == Constants.PlayerType.LOCAL)
                         {
                             SocketController.instance.PrepareTurnData(from, to2, TurnType.sparePawn.ToString());
                             return;
                         }


                     }
                     else
                         GameManager.instance.IncreaseTurn();

                     string newPawnID = currentPawnType.ToString();
                     //foreach (Transform item in transform)
                     //{
                     //    if (item.GetComponent<Pawn>())
                     //        newPawnID = "," + item.GetComponent<Pawn>().currentPawnType.ToString();
                     //}
                     GridData to = new GridData(targetSquare.squareId2, newPawnID, Rank.ToString());
                     if (GameManager.instance.currentPlayerTurn == Constants.PlayerType.LOCAL)
                         SocketController.instance.PrepareTurnData(from, to, TurnType.normalPawn.ToString());
                     else
                         SocketController.instance.FinishedTurn();
                 });
    }

    public Vector3 PreCheckForTaking(Square targetSquare)
    {
        Vector3 targetPos = targetSquare.transform.position;

        Pawn targetPawn = targetSquare.occupiedPawn;
        if (targetPawn != null)
        {
            targetPos = targetPawn.transform.position;
            if (Rank > 5) // ALL TOWER PAWNS
            {
                // TARGET PAWN SHOULD REMOVE FROM BOARD
            }
            else if (Rank == 1 && targetPawn.Rank == 1)
            {
                // SELECTED PAWN WILL SIT OVER TARGET PAWN AND Rank WILL BE 2

                targetPos.y = 0.08f;
            }
        }
        return targetPos;

    }

    public void CheckTaking(Pawn targetPawn)
    {
        if (targetPawn != null)
        {
            if (Rank > 5) // ALL TOWER PAWNS
            {
                // TARGET PAWN SHOULD REMOVE FROM BOARD

                Vector3 pos = targetPawn.transform.position;
                pos.x = 2;
                pos.z = 0;

                targetPawn.transform.position = pos;
            }
            else if (Rank == 1 && targetPawn.Rank == 1)
            {
                Rank = 2;
                targetPawn.transform.SetParent(transform);
            }

        }
    }

    public void SetMeshPosition(int rank, bool shouldHide = true)
    {
        meshComponent.localPosition = new Vector3(0, 0, (rank - 1) * 0.0013f);
        UpdateRankImage(rank, shouldHide);
    }

    void UpdateRankImage(int rank, bool shouldHide)
    {
        rankImage.transform.parent.SetAsLastSibling();
        rankImage.gameObject.SetActive(!shouldHide);
        if (!shouldHide)
            rankImage.sprite = GameManager.instance.rankSprites[rank - 1];
    }

    public int GetDistance(Square square)
    {
        return (occupiedSquare.SquareId - square.SquareId) / Constants.noOfSquarePerRow;
    }
}
