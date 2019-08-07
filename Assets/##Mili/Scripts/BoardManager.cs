using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class BoardManager : MonoBehaviour {

    public static BoardManager instance;

    public GamePlay gamePlay;
    public bool canClick = true;
    public Square[] squarePoints;
    public Transform selectionBox;
    public GameObject possibleMoveSqaure;

    private Square selectedSquare;

    public List<int> allPawnType;

    public List<GameObject> localPawns;
    public List<GameObject> remotePawns;


    public List<PawnMovementData> pawnMovementDatas;

    [Header("UI BUTTONS")]
    public GameObject undoButton;
    public GameObject redoButton;

    private int undoCounter = -1;
    private bool undoClicked = false;

    public int UndoCounter
    {
        get
        {
            return undoCounter;
        }

        set
        {
            undoCounter = value;
            if (value <= 0) 
                undoButton.SetActive(false);
            else
               undoButton.SetActive(true);

            if(value == pawnMovementDatas.Count)
                redoButton.SetActive(false);
        }
    }

    public int CounterForPlacingPawn
    {
        get
        {
            return counterForPlacingPawn;
        }

        set
        {
            counterForPlacingPawn = value;
          
            HighlightSquareForPlacingPawn();

            if (value == 3 && OnPawnPlacementComplete != null)
            {
                ResetAllSquare();
                OnPawnPlacementComplete(); /// this action is subscribed by game manager

                if (Constants.isAI)
                    PlacePawnForAI();
                else
                    LoadingCanvas.Instance.ShowLoadingPopUp("Waiting for Opponent to place pawns");
                // PLACE REMOTE PLAYER PAWN
            }
        }
    }

    private int counterForPlacingPawn = 0;
    
    public Player[] players;
    public Player playerToPlaceSpareTower;
    public Constants.PlayerType playerWithOneTower;

    #region MoveToChoosePanelLater
    public Material local_spareTowerMaterial;
    public Material remote_spareTowerMaterial;
    public Material local_pawnMaterial;
    public Material remote_pawnMaterial; 
    #endregion

    #region ALL EVENTS
    public static UnityAction OnPawnPlacementComplete;
    public static UnityAction InitializeGameRules;
    public static UnityAction<Constants.PlayerType> OnActiveEndRule;
    public static UnityAction OnDeactiveEndRule;
    #endregion
   

    public Dictionary<string, Square> squareCollection = new Dictionary<string, Square>();
   

    private void Awake()
    {
        instance = this;
        
    }
    // Use this for initialization
    void Start ()
    {
        if(MultiplayerManager.GetPlayerTag() == Constants.PlayerTag.PLAYER_2)
        {
            transform.parent.rotation = Quaternion.Euler(0, 180, 0);
        }
        ResetAllSquare(); //Turns off highlights
        if(!Constants.isAI)
            InitializeGameRules += InitializeMultiplayerGame;
        else
            InitializeGameRules += InitializeGame;

        GamePlay.OnViewTypeChange += OnViewTypeChanged;

        //Move this to choose panel later
        local_spareTowerMaterial.color = local_pawnMaterial.color;
        remote_spareTowerMaterial.color = remote_pawnMaterial.color;
        SetupSecondaryIds();
    }   

    private void InitializeGame()
    {        
        pawnMovementDatas = new List<PawnMovementData>();
        HighlightSquareForPlacingPawn();
        GameManager.OnTurnChanged += OnTurnChanged;
    }

    private void OnViewTypeChanged(Constants.ViewType viewType)
    {
        if(viewType == Constants.ViewType.THREE_D)
        {
            //for 3d
            foreach (var thisPlayer in players)
            {
                thisPlayer.removedPawnParent3D.gameObject.SetActive(true);
                thisPlayer.removedPawnParent2D.gameObject.SetActive(false);
            }
        }
        else //for 2d
        {
            foreach (var thisPlayer in players)
            {
                thisPlayer.removedPawnParent3D.gameObject.SetActive(false);
                thisPlayer.removedPawnParent2D.gameObject.SetActive(true);
            }
        }
    }

    private void OnTurnChanged(Constants.PlayerType arg1, bool arg2)
    {
        //To place spare tower when no space was available 
        if (playerToPlaceSpareTower != null)
            playerToPlaceSpareTower.PlaceStoredSpareTower();
    }
    #region Editor Functions
    public void SetSquares()
    {
        squarePoints = GetComponentsInChildren<Square>();
        int id = 0;
        foreach (Square item in squarePoints)
        {
            item.gameObject.name = "Square_"+id;
            item.SquareId = id;
            id++;
        }
    }
    #endregion
    public void  IsSquareSelected(Square targetSquare)
    {
        if (!canClick)
            return;
        if (selectedSquare == null 
            && Constants.isAI 
            && targetSquare.occupiedPawn != null 
            && targetSquare.occupiedPawn.currentPlayerType == Constants.PlayerType.REMOTE)
            return;
        
        if (selectedSquare != null && targetSquare.isHighlighted)
        {
            // Move selected square to this square
            GameManager.instance.currentPlayerTurn = selectedSquare.occupiedPawn.currentPlayerType;
            UndoCounter = -1;
            canClick = false;
            selectedSquare.MovePawn(targetSquare);
            selectedSquare = null;
            return;
        }
        //if selected square has a pawn
        else if(targetSquare.occupiedPawn != null)
        {
            if (targetSquare.occupiedPawn.currentPlayerType != GameManager.instance.currentPlayerTurn)//if the board piece is of the opponent's
                return;
            selectedSquare = targetSquare;
            selectionBox.gameObject.SetActive(true);
            selectionBox.transform.position = targetSquare.transform.position;

            ResetAllSquare();
            targetSquare.ShowPossibleMovesOnClick();
        }
        else if (targetSquare.occupiedPawn == null && targetSquare.isHighlighted)
        {
            // PLACE SPARE TOWER IN HIGHLIGHTED PLACE
            Player player = players[(int)GameManager.instance.currentPlayerTurn];
            PlaceSparePawn(player, targetSquare);
        }
    }
    //Turns off highlights
    public void ResetAllSquare()
    {
        for (int i = 0; i < squarePoints.Length; i++)
        {
            squarePoints[i].SetHighlightStatus(false);
        }
    }
      
    public void RemovePawnFromBoard(Pawn pawn)
    {
        Transform parent3D = players[(int)pawn.currentPlayerType].removedPawnParent3D;
        Transform parent2D = players[(int)pawn.currentPlayerType].removedPawnParent2D;


        SetRemovedPawns3D(pawn.transform, parent3D);
        //for 2d
        GameObject duplicatePawn = Instantiate(pawn.gameObject, pawn.transform.position, pawn.transform.rotation);

        SetRemovedPawns2D(duplicatePawn.transform, parent2D);
       
      //  Vector3 pos = new Vector3(0, 0, 0.3f * (parent.childCount - 1));
      //  pawn.transform.localPosition = pos;

        int noOfTowerPawn = players[(int)pawn.currentPlayerType].GetNoOfTowerPawnAvailable();
      //  Debug.Log(" noOfTowerPawn " + noOfTowerPawn);
        if(noOfTowerPawn == 1 && OnActiveEndRule != null)
        {
            Debug.Log("Active End Game Rules");
            OnActiveEndRule(pawn.currentPlayerType);
            playerWithOneTower = pawn.currentPlayerType;
        }
        if(noOfTowerPawn == 0)
        {
            Debug.Log("Game should end");
            GameManager.instance.OnGameEnd(pawn.currentPlayerType);
        }
        
    }

    private void SetRemovedPawns3D(Transform targetPawn, Transform dumpPoint)
    {
        //this is for the 3d view only
        targetPawn.SetParent(dumpPoint);
        float maxTorqueRange = 7.0f;
        float minTorqueRange = 2.0f;
        float delayToDestroyRigidbody = 4.2f;
        Vector3 newRelativeTorque = new Vector3(Random.Range(minTorqueRange, maxTorqueRange), Random.Range(minTorqueRange, maxTorqueRange), Random.Range(minTorqueRange, maxTorqueRange));
        Vector3 offsetPosition = new Vector3(dumpPoint.position.x, dumpPoint.position.y, dumpPoint.position.z + Random.Range(-0.36f, 0.36f));
        targetPawn.DOMove(offsetPosition, Constants.PAWN_DUMP_SPEED).OnComplete(()=> 
        {
            MeshRenderer[] meshRenderer = targetPawn.GetComponentsInChildren<MeshRenderer>();

            foreach (var eachRenderer in meshRenderer)
            {
                MeshCollider meshCollider = eachRenderer.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                Rigidbody rigidbody = eachRenderer.gameObject.AddComponent<Rigidbody>();
                rigidbody.drag = 0.35f;
                rigidbody.angularDrag = 0.1f;
                rigidbody.useGravity = true;
                rigidbody.AddRelativeTorque(newRelativeTorque, ForceMode.Impulse);
                DOVirtual.DelayedCall(delayToDestroyRigidbody, () =>
                {
                    Destroy(rigidbody);
                });
            }




           
        });
    }

    private void SetRemovedPawns2D(Transform targetPawn, Transform dumpPoint)
    {
        targetPawn.SetParent(dumpPoint);
        Vector3 pos = new Vector3(0, 0, 0.3f * (dumpPoint.childCount - 1));
        targetPawn.localPosition = pos;
    }

    public void CheckForResetEndGameRule()
    {
        bool canReset = true;
        for (int i = 0; i < 2; i++)
        {
            int noOfTowerPawn = players[i].GetNoOfTowerPawnAvailable();
            Debug.Log(" noOfTowerPawn " + noOfTowerPawn);
            if (noOfTowerPawn < 2)
                canReset = false;
        }

        if (canReset && OnDeactiveEndRule != null)
            OnDeactiveEndRule();
       
    }
    #region UI Buttons
    public void UndoClicked()
    {
        undoClicked = true;
        UndoCounter--;
        redoButton.SetActive(true);
        selectedSquare = squarePoints[pawnMovementDatas[UndoCounter].toSquareId];
        Square targetSquare = squarePoints[pawnMovementDatas[UndoCounter].fromSquareId];
       // MovePawn(targetSquare, false);

    }
    public void RedoClicked()
    {
        Square targetSquare = squarePoints[pawnMovementDatas[UndoCounter].toSquareId];
        selectedSquare = squarePoints[pawnMovementDatas[UndoCounter].fromSquareId];
       // MovePawn(targetSquare, false);
        UndoCounter++;

    }
    #endregion

    #region Pawn Placement
    //used for highlighting rows at the start of the game
    public void HighlightSquareForPlacingPawn()
    {
        if(Constants.isAI)
        {
            #region AI
            if (CounterForPlacingPawn == 0)
            {
                GamePlay.instance.StartTurnTimer(60); //show warning here
                GamePlay.instance.ShowWarning(40);
            }

            if (CounterForPlacingPawn == 0)
            {
                ResetAllSquare();
                for (int i = 0; i < Constants.noOfSquarePerRow; i++)
                {
                    players[0].firstThreeRowSquare[i].SetHighlightStatus(true);
                }
                gamePlay.SetDesriptionForPlacingPawn("Place Plus tower in highlighed place");
            }
            else if (CounterForPlacingPawn == 1)
            {
                ResetAllSquare();
                for (int i = Constants.noOfSquarePerRow; i < 2 * Constants.noOfSquarePerRow; i++)
                {
                    players[0].firstThreeRowSquare[i].SetHighlightStatus(true);
                }
                gamePlay.SetDesriptionForPlacingPawn("Place Cross tower in highlighed place");
            }
            else if (CounterForPlacingPawn == 2)
            {
                ResetAllSquare();
                for (int i = 2 * Constants.noOfSquarePerRow; i < 3 * Constants.noOfSquarePerRow; i++)
                {
                    players[0].firstThreeRowSquare[i].SetHighlightStatus(true);
                }
                gamePlay.SetDesriptionForPlacingPawn("Place Star tower in highlighed place");
            }
            else
                gamePlay.SetDesriptionForPlacingPawn("");
            #endregion
        }
        else
        {
            #region Multiplayer
            if (CounterForPlacingPawn == 0)
            {
                GamePlay.instance.StartTurnTimer(60); //show warning here
                GamePlay.instance.ShowWarning(40);
            }

            if (CounterForPlacingPawn == 2)
            {
                ResetAllSquare();
                for (int i = 0; i < Constants.noOfSquarePerRow; i++)
                {
                    players[0].firstThreeRowSquare[i].SetHighlightStatus(true);
                }
                gamePlay.SetDesriptionForPlacingPawn("Place Plus tower in highlighed place");
            }
            else if (CounterForPlacingPawn == 1)
            {
                ResetAllSquare();
                for (int i = Constants.noOfSquarePerRow; i < 2 * Constants.noOfSquarePerRow; i++)
                {
                    players[0].firstThreeRowSquare[i].SetHighlightStatus(true);
                }
                gamePlay.SetDesriptionForPlacingPawn("Place Cross tower in highlighed place");
            }
            else if (CounterForPlacingPawn == 0)
            {
                ResetAllSquare();
                for (int i = 2 * Constants.noOfSquarePerRow; i < 3 * Constants.noOfSquarePerRow; i++)
                {
                    players[0].firstThreeRowSquare[i].SetHighlightStatus(true);
                }
                gamePlay.SetDesriptionForPlacingPawn("Place Star tower in highlighed place");
            }
            else
                gamePlay.SetDesriptionForPlacingPawn("");
            #endregion
        }

    }

    public void PlacePawnsRandomly()
    {
        for (int i = 0; i < (3 * Constants.noOfSquarePerRow); i += Constants.noOfSquarePerRow)
        {
            int rand = Random.Range(i, i+Constants.noOfSquarePerRow);
            Square randomSquareInRow = players[0].firstThreeRowSquare[rand];
            PlacePawn(randomSquareInRow);           
        }

       
    }
    PawnPlaced newPawnPlaced = new PawnPlaced();
    public void PlacePawn(Square square)
    {
        if (!square.isHighlighted)
            return;

        #region Old COde
        //if (CounterForPlacingPawn == 0) // PLACE STAR TOWER
        //{
        //    int index = square.SquareId;
        //    CreatePawn( index, players[0].allPawns, localPawns[5]);

        //    for (int i = 0; i < Constants.noOfSquarePerRow; i++)
        //    {
        //        if (players[0].firstThreeRowSquare[i].SquareId != index)
        //            CreatePawn(players[0].firstThreeRowSquare[i].SquareId, players[0].allPawns, localPawns[2]);
        //    }

        //    if (!Constants.isAI)
        //        newPawnPlaced.AddSquareID(index);
        //}
        //else  // PLACE PLUS OR CROSS TOWER
        //{
        //    int towerIndex = 0;
        //    int pawnIndex = 0;
        //    if (CounterForPlacingPawn == 2)
        //    {
        //        towerIndex = 4;
        //        pawnIndex = 0;
        //    }
        //    else
        //    {
        //        towerIndex = 3;
        //        pawnIndex = 1;
        //    }
        ////    pawnIndex = SetHightlights(square, towerIndex, pawnIndex);
        //} 
        #endregion

        int towerIndex = 0;
        int pawnIndex = 0;

        if(Constants.isAI)
        {
            #region Multiplayer
            switch (CounterForPlacingPawn)
            {

                case 0:
                    towerIndex = 4;
                    pawnIndex = 0;
                    SetHightlights(square, towerIndex, pawnIndex);
                    
                    break;
                case 1:
                    towerIndex = 3;
                    pawnIndex = 1;
                    SetHightlights(square, towerIndex, pawnIndex);
                    break;
                case 2:
                    int index = square.SquareId;
                    CreatePawn(index, players[0].allPawns, localPawns[5]);

                    for (int i = 12; i < 12+ Constants.noOfSquarePerRow; i++)
                    {
                        if (players[0].firstThreeRowSquare[i].SquareId != index)
                        {
                            CreatePawn(players[0].firstThreeRowSquare[i].SquareId, players[0].allPawns, localPawns[2]);
                        }
                    }

                    //send multiplayer data from here
                    //if (!Constants.isAI)
                    //    newPawnPlaced.AddSquareID(index);
                    break;
                default:
                    break;
            }
            #endregion
        }

        else
        {
            #region Multiplayer
            switch (CounterForPlacingPawn)
            {

                case 2:

                    int index = square.SquareId;
                    CreatePawn(index, players[0].allPawns, localPawns[5]);

                    for (int i = 0; i < Constants.noOfSquarePerRow; i++)
                    {
                        if (players[0].firstThreeRowSquare[i].SquareId != index)
                        {
                            CreatePawn(players[0].firstThreeRowSquare[i].SquareId, players[0].allPawns, localPawns[2]);
                        }
                    }
                    break;
                case 1:
                    towerIndex = 3;
                    pawnIndex = 1;
                    SetHightlights(square, towerIndex, pawnIndex);
                    break;
                case 0:
                    towerIndex = 4;
                    pawnIndex = 0;
                    SetHightlights(square, towerIndex, pawnIndex);

                    //send multiplayer data from here
                    //if (!Constants.isAI)
                    //    newPawnPlaced.AddSquareID(index);
                    break;
                default:
                    break;
            }
            #endregion
        }





        // Debug.Log("Counter for placing pawns: " + CounterForPlacingPawn);
        CounterForPlacingPawn++;
        if (CounterForPlacingPawn == 3)
            gamePlay.ShowWarning(false);

        #region Mulitplayer
        if (CounterForPlacingPawn == 3 && !Constants.isAI)
        {
            List<Square> initSquares = new List<Square>();
            initSquares = players[0].firstThreeRowSquare;
            List<GridData> newGridData = new List<GridData>();

            for (int i = 0; i < initSquares.Count; i++)
            {
                GridData gridData = new GridData(initSquares[i].squareId2, initSquares[i].occupiedPawn.currentPawnType.ToString(), initSquares[i].occupiedPawn.Rank.ToString());
                newGridData.Add(gridData);
            }
            PawnPlacements pawnPlacements = new PawnPlacements(Database.GetString(Database.Key.ROOM_NAME), Database.GetString(Database.Key.ROOM_ID), Database.GetString(Database.Key.PLAYER_ID),newGridData);
            //Send Data to server
            SocketController.instance.SendPawnPlacements(pawnPlacements);
        } 
        #endregion
    }

    private void SetHightlights(Square square, int towerIndex, int pawnIndex)
    {
        int index = square.SquareId;
        #region RecordPawnPlacements
        if (!Constants.isAI)
            newPawnPlaced.AddSquareID(index);
        #endregion
        CreatePawn(index, players[0].allPawns, localPawns[towerIndex]);

        int startIndex = (index / Constants.noOfSquarePerRow) * Constants.noOfSquarePerRow;
        int endIndex = (index / Constants.noOfSquarePerRow) * Constants.noOfSquarePerRow + Constants.noOfSquarePerRow;

        for (int i = index + 1; i < endIndex; i++)
        {
            CreatePawn(i, players[0].allPawns, localPawns[pawnIndex]);
            if (pawnIndex == 0)
                pawnIndex = 1;
            else
                pawnIndex = 0;
        }
        for (int i = startIndex; i < index; i++)
        {
            CreatePawn(i, players[0].allPawns, localPawns[pawnIndex]);
            if (pawnIndex == 0)
                pawnIndex = 1;
            else
                pawnIndex = 0;
        }       
    }

    void PlacePawnForAI()
    {
        // PLACE STAR TOWER & STAR PAWN
        int index = Random.Range(0, Constants.noOfSquarePerRow);
        for (int i = 0; i < Constants.noOfSquarePerRow; i++)
        {
            if(i == index)
               CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[5]);
            else
               CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[2]);
        }

        // PLACE PLUS AND CROSS TOWER & NORMAL PAWN
        for (int j = 1; j <= 2; j++)
        {
            int towerIndex = 0;
            int pawnIndex = 0;
            if (j == 2)
            {
                towerIndex = 4;
                pawnIndex = 0;
            }
            else
            {
                towerIndex = 3;
                pawnIndex = 1;
            }
            index = Random.Range(j * Constants.noOfSquarePerRow, (j + 1) * Constants.noOfSquarePerRow);
            CreatePawn(players[1].firstThreeRowSquare[index].SquareId, players[1].allPawns, remotePawns[towerIndex]);

            int startIndex = j* Constants.noOfSquarePerRow;
            int endIndex = (j + 1) * Constants.noOfSquarePerRow;

            for (int i = index + 1; i < endIndex; i++)
            {
                CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[pawnIndex]);
                if (pawnIndex == 0)
                    pawnIndex = 1;
                else
                    pawnIndex = 0;
            }
            for (int i = startIndex; i < index; i++)
            {
                CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[pawnIndex]);
                if (pawnIndex == 0)
                    pawnIndex = 1;
                else
                    pawnIndex = 0;
            }
        }
    }
    
    void CreatePawn(int i, Transform parent, GameObject pawnPrefabs)
    {
        Transform obj = Instantiate(pawnPrefabs).transform;
        obj.GetComponent<Pawn>().PawnId = i + 1;
        obj.name = "Pawn_" + obj.GetComponent<Pawn>().PawnId.ToString();
        obj.position = squarePoints[i].transform.position;
        squarePoints[i].occupiedPawn = obj.GetComponent<Pawn>();
        obj.GetComponent<Pawn>().occupiedSquare = squarePoints[i];
        obj.SetParent(parent);
        //squarePoints[i].SetHighlightStatus(false);
    }
    void PlaceSparePawn(int i, Transform parent, Pawn pawn)
    {
       // pawn.transform.position = squarePoints[i].transform.position;
        FlyPawnOut(pawn.transform, squarePoints[i].transform.position);
        squarePoints[i].occupiedPawn = pawn;
        pawn.occupiedSquare = squarePoints[i];
        pawn.transform.SetParent(parent);
        pawn.gameObject.SetActive(true);
        gamePlay.SetDesriptionForPlacingPawn("");
        if (GameManager.instance.isEndRuleGameActivated)
            pawn.ActiveEndGameRule(playerWithOneTower);
    }

    public void PlaceSparePawn(Player player, Square targetSquare)
    {
        Pawn pawn = player.currentSparePawn;
        player.currentSparePawn = null;
        PlaceSparePawn(targetSquare.squareId, player.allPawns, pawn);
        playerToPlaceSpareTower = null;
        ResetAllSquare();
        GameManager.instance.IncreaseTurn();
        CheckForResetEndGameRule();

    }
    //the spare tower comes out of board and lands on the target tile
    private void FlyPawnOut(Transform targetPawn, Vector3 targetPosition)
    {
        float multiplier = 1;
        if (targetPawn.position.z < 0)
            multiplier = -1;        
        else
            multiplier = 1;
        

        float zOffset = 2 * multiplier;
        float zDuration = 0.8f;
        float yOffset = 0.8f;
        float rotationDuration = 0.8f;
        float finalDuration = 0.5f;
        Vector3 rotationValue = new Vector3(-90, 0, 0);

        
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(targetPawn.DOLocalMoveZ(zOffset, zDuration)); // move out of the side of the board
        mySequence.Append(targetPawn.DOLocalRotate(rotationValue, rotationDuration));
        mySequence.Append(targetPawn.DOLocalMoveY(yOffset, zDuration)); //fly upwards
        mySequence.Append(targetPawn.DOLocalMove(targetPosition, finalDuration)).SetEase(Ease.OutCubic); //finally go to the target position
    }
    #endregion

    public void OnMovementComplete()
    {
        canClick = true;
        selectionBox.gameObject.SetActive(false);
    }

    public void SetSquarePointOfMultiplayer()
    {
        players[0].firstThreeRowSquare.Clear();
        if (MultiplayerManager.GetPlayerTag()== Constants.PlayerTag.PLAYER_1)
        {
            Debug.Log("Adding front squares");
            for (int i = 0; i < 3 * Constants.noOfSquarePerRow; i++)
            {
                players[0].firstThreeRowSquare.Add(squarePoints[squarePoints.Length - i - 1]);               
            }
        }
        else
        {

           // Debug.Log("Adding back squares");
            for (int i = 0; i < 3 * Constants.noOfSquarePerRow; i++)
            {
                players[0].firstThreeRowSquare.Add(squarePoints[i]);
            }
        }
    }

    public void SetSquarePointOfPlayer()
    {
        players[0].firstThreeRowSquare.Clear();
        players[1].firstThreeRowSquare.Clear();
        for (int i = 0; i < 3 * Constants.noOfSquarePerRow; i++)
        {
            players[0].firstThreeRowSquare.Add(squarePoints[i]);
        }
        for (int i = 0; i < 3 * Constants.noOfSquarePerRow; i++)
        {
            players[1].firstThreeRowSquare.Add(squarePoints[squarePoints.Length -i - 1]);
        }

        transform.parent.eulerAngles = new Vector3(-90f, 180, -90f);

        HighlightSquareForPlacingPawn();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SetSquarePointOfPlayer();
    }

    private void OnDestroy()
    {
        InitializeGameRules -= InitializeGame;
        GamePlay.OnViewTypeChange -= OnViewTypeChanged;
    }

    #region Multiplayer
    //squareId2 is of the A3, B5 etc format
    void SetupSecondaryIds()
    {
        char[] rows = "ABCDEF".ToCharArray();
        int[] intArray = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        int squareNumber = 0;
        for (int i = 12; i > 0; i--)
        {
            for (int j = 0; j < rows.Length; j++)
            {
                squarePoints[squareNumber].squareId2 = "" + rows[j] + i;
                squareNumber++;
            }
        }
        AddToDict();
    }
    void AddToDict()
    {
        for (int i = 0; i < squarePoints.Length; i++)
        {
            squareCollection.Add(squarePoints[i].squareId2, squarePoints[i]);
        }       
    }

    public Square GetSquare(string key)
    {
        return squareCollection[key];
    }   

    void InitializeMultiplayerGame()
    {
        pawnMovementDatas = new List<PawnMovementData>();
        SetSquarePointOfMultiplayer();
        HighlightSquareForPlacingPawn();
        GameManager.OnTurnChanged += OnTurnChanged;
    }
    //only for placing pawns and towers at the start of the game
    public void PlacePawnForOpponent(FirstPawnData pawnPlaced)
    {
        #region Old Code
        //#region Convert Square ID
        //pawnPlaced.positions[0] = 71 - pawnPlaced.positions[0];
        //pawnPlaced.positions[1] = 71 - pawnPlaced.positions[1];
        //pawnPlaced.positions[2] = 71 - pawnPlaced.positions[2];

        //#endregion
        //// PLACE STAR TOWER & STAR PAWN
        //int index = pawnPlaced.positions[0];
        //for (int i = 0; i < Constants.noOfSquarePerRow; i++)
        //{

        //    if (i == index)
        //        CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[5]);
        //    else
        //        CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[2]);
        //}

        //// PLACE PLUS AND CROSS TOWER & NORMAL PAWN
        //for (int j = 1; j <= 2; j++)
        //{
        //    int towerIndex = 0;
        //    int pawnIndex = 0;
        //    if (j == 2)
        //    {
        //        towerIndex = 4;
        //        pawnIndex = 0;
        //    }
        //    else
        //    {
        //        towerIndex = 3;
        //        pawnIndex = 1;
        //    }
        //    index = pawnPlaced.positions[j];//Random.Range(j * Constants.noOfSquarePerRow, (j + 1) * Constants.noOfSquarePerRow);
        //    CreatePawn(players[1].firstThreeRowSquare[index].SquareId, players[1].allPawns, remotePawns[towerIndex]);

        //    int startIndex = j * Constants.noOfSquarePerRow;
        //    int endIndex = (j + 1) * Constants.noOfSquarePerRow;

        //    for (int i = index + 1; i < endIndex; i++)
        //    {
        //        CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[pawnIndex]);
        //        if (pawnIndex == 0)
        //            pawnIndex = 1;
        //        else
        //            pawnIndex = 0;
        //    }
        //    for (int i = startIndex; i < index; i++)
        //    {
        //        CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[pawnIndex]);
        //        if (pawnIndex == 0)
        //            pawnIndex = 1;
        //        else
        //            pawnIndex = 0;
        //    }
        //} 
        #endregion

        //FirstpawnData contains data of this player and opponent :(
        int index = 0;
            Debug.Log("User Id :"+ pawnPlaced.pawnlist[0].user_id + "___" + Database.GetString(Database.Key.PLAYER_ID));
            string uid = Database.GetString(Database.Key.PLAYER_ID);
        if (pawnPlaced.pawnlist[0].user_id.Equals(uid))
            index = 1;
        else
            index = 0;
            Debug.Log("Got User Id :"+ index + "<---index " + pawnPlaced.pawnlist[index].user_id + "___" + uid);
        List<GridData> opponentPlacementData = pawnPlaced.pawnlist[index].GridDatas;
        CreatePawnsOpponent(opponentPlacementData);

    }

    GameObject GetGutiType(Pawn.PawnType pawnType, int rank)
    {
        switch (pawnType)
        {
            case Pawn.PawnType.CROSS:
                if (rank > 1)
                    return remotePawns[3];
                else
                    return remotePawns[0];
               
            case Pawn.PawnType.PLUS:
                if (rank > 1)
                    return remotePawns[4];
                else
                    return remotePawns[1];
               
            case Pawn.PawnType.STAR:
                if (rank > 1)
                    return remotePawns[5];
                else
                    return remotePawns[2];
               
            default:
                return remotePawns[0];
               
        }
    }

    void CreatePawnsOpponent(List<GridData> gridDatas)
    {
        //star tower = remotepawns[5];
        //star       = remotePawns[2];
        for (int i = 0; i < gridDatas.Count; i++)
        {
            Square square = GetSquare(gridDatas[i].SquareID);
            Pawn.PawnType pawnType = (Pawn.PawnType)System.Enum.Parse(typeof(Pawn.PawnType), gridDatas[i].PawnType);
            int rankNumber = System.Convert.ToInt16(gridDatas[i].PawnRank);
            GameObject guti = GetGutiType(pawnType, rankNumber);
            Transform obj = Instantiate(guti).transform;
            obj.GetComponent<Pawn>().PawnId = i + 1;
            obj.name = "Pawn_" + obj.GetComponent<Pawn>().PawnId.ToString();
            obj.position = square.transform.position;
            square.occupiedPawn = obj.GetComponent<Pawn>();
            obj.GetComponent<Pawn>().occupiedSquare = square;
            obj.SetParent(players[1].allPawns);
        }
        //// PLACE STAR TOWER & STAR PAWN
        //int index = pawnPlaced.positions[0];
        //for (int i = 0; i < Constants.noOfSquarePerRow; i++)
        //{

        //    if (i == index)
        //        CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[5]);
        //    else
        //        CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[2]);
        //}

        //// PLACE PLUS AND CROSS TOWER & NORMAL PAWN
        //for (int j = 1; j <= 2; j++)
        //{
        //    int towerIndex = 0;
        //    int pawnIndex = 0;
        //    if (j == 2)
        //    {
        //        towerIndex = 4;
        //        pawnIndex = 0;
        //    }
        //    else
        //    {
        //        towerIndex = 3;
        //        pawnIndex = 1;
        //    }
        //    index = pawnPlaced.positions[j];//Random.Range(j * Constants.noOfSquarePerRow, (j + 1) * Constants.noOfSquarePerRow);
        //    CreatePawn(players[1].firstThreeRowSquare[index].SquareId, players[1].allPawns, remotePawns[towerIndex]);

        //    int startIndex = j * Constants.noOfSquarePerRow;
        //    int endIndex = (j + 1) * Constants.noOfSquarePerRow;

        //    for (int i = index + 1; i < endIndex; i++)
        //    {
        //        CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[pawnIndex]);
        //        if (pawnIndex == 0)
        //            pawnIndex = 1;
        //        else
        //            pawnIndex = 0;
        //    }
        //    for (int i = startIndex; i < index; i++)
        //    {
        //        CreatePawn(players[1].firstThreeRowSquare[i].SquareId, players[1].allPawns, remotePawns[pawnIndex]);
        //        if (pawnIndex == 0)
        //            pawnIndex = 1;
        //        else
        //            pawnIndex = 0;
        //    }
        //}
    }
    #endregion
}
