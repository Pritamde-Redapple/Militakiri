using socket.io;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.SceneManagement;
using MiniJSON;
using DG.Tweening;
using System;

public class SocketController : MonoBehaviour {
    public static SocketController instance;

    private Socket socket;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        socket = Socket.Connect(Constants.SocketURL);
        socket.On(SystemEvents.connect, () =>
        {
            Debug.Log("Socket connected successfully");
        });

        socket.On(SystemEvents.disconnect, () =>
        {
            Debug.Log("Socket disconnected successfully");
        });

        socket.On(SystemEvents.reconnect, (int reconnectionAttempt) =>
        {
            Debug.Log("Socket recoonected after " + reconnectionAttempt + " attempt");
        });

        socket.On(GameListen.user_connected.ToString(), UserConnectedCallBack);
        socket.On(GameListen.connected_room.ToString(), GameRequestCallback);  //when room created and waiting for another player
        socket.On(GameListen.enter_user.ToString(), EnterUserCallback); //fired twice
        socket.On(GameListen.gameinit.ToString(), GameInitCallback); //when two player joins and game starts. Change scene here.
        socket.On(GameListen.gamestart.ToString(), GameStart); 
        // socket.On("game_winner", GameWinnerCallback);
        socket.On(GameListen.initialpawnplacements.ToString(), StartingPawnsPlaced);
        socket.On(GameListen.turndata.ToString(), OpponentsTurnCallback);
        socket.On(GameListen.playerturn.ToString(), WhoWillPlay);
        socket.On(GameListen.turn_start.ToString(), WhoWillPlay);
        socket.On(GameListen.leave_room.ToString(), SomeoneLeftRoom);
        socket.On(GameListen.game_winner.ToString(), GameDone);
       
        

    }

    private void GameDone(string obj)
    {
        Debug.Log("Game Over " + obj);
    }

    private void SomeoneLeftRoom(string obj)
    {
        Debug.Log("SomeoneLeftRoom " + obj);
    }

    /*
* {
"status":"1",
"message":"player turn",
"result":{
"user_id":"5d3ec68bf17488071c0fa214",
"name":"Ganesh",
"player_id":"MILT-5d3ec68bf17488071c0fa214"
}
} 
* 
*/
    private void WhoWillPlay(string obj)
    {
        Debug.Log("WhoWillPlay" + obj);
        JSONNode data = JSONNode.Parse(obj);
        string currentPlayerUserId = data["result"]["user_id"].Value;
        Debug.Log(currentPlayerUserId +"____"+ Database.GetString(Database.Key.PLAYER_ID));
        if(Database.GetString(Database.Key.PLAYER_ID).CompareTo(currentPlayerUserId) == 0)
        {
            Debug.Log("My Turn");
            BoardManager.instance.canClick = true;
            GameManager.instance.currentGameState = GameManager.GAMESTATE.PLAY;
            GameManager.instance.IncreaseTurn(Constants.PlayerType.LOCAL);
        }
        else
        {
            Debug.Log("Other Players Turn");
            Debug.Log(GameManager.instance.currentGameState.ToString());
            GameManager.instance.currentGameState = GameManager.GAMESTATE.NONE;
            GameManager.instance.IncreaseTurn(Constants.PlayerType.REMOTE);
        }
    }


    #region Emit Functions
    public void AddUser()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["access_token"] = Database.GetString(Database.Key.ACCESS_TOKEN);
        socket.EmitJson(GameEmits.adduser.ToString(), new JSONObject(data).ToString());

#if SOCKET_EMIT_LOG
        Debug.Log("Add User: " + new JSONObject(data).ToString());
#endif
    }

    public void GameRequest()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["access_token"] = Database.GetString(Database.Key.ACCESS_TOKEN);
        data["board_type"] = Constants.currentBoardType.ToString().ToLower();
        data["score"] = "35";//Database.GetString(Database.Key.SCORE); 
        Debug.Log(" Database.GetString(Database.Key.SCORE)  " + data["score"]);
        socket.EmitJson(GameEmits.gamerequest.ToString(), new JSONObject(data).ToString());

#if SOCKET_EMIT_LOG
        Debug.Log("gamerequest: " + new JSONObject(data).ToString());
#endif
    }

    public void Leave()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["access_token"] = Database.GetString(Database.Key.ACCESS_TOKEN);
        data["room_id"] = Database.GetString(Database.Key.ROOM_ID);
        data["room_name"] = Database.GetString(Database.Key.ROOM_NAME);

        socket.EmitJson(GameEmits.leave_room.ToString(), new JSONObject(data).ToString());
    }

    public void Chat()
    {

    }
    #endregion

    public void FinishedTurn()
    {
        //{ "room_id":"5d3adfceb7345c267b360b67","room_name":"ROOM1564139470776",
        //    "user_id":"5cf64d845732bb2515e975d0","turn_start":"1"}

        Dictionary<string, string> data = new Dictionary<string, string>();
        data["room_id"] = Database.GetString(Database.Key.ROOM_ID);
        data["room_name"] = Database.GetString(Database.Key.ROOM_NAME);
        data["user_id"] = Database.GetString(Database.Key.PLAYER_ID);
        data["turn_start"] = "1";
        Debug.Log("Turn Start Fired "+ data.ToString());
        socket.EmitJson(GameEmits.turnstart.ToString(), new JSONObject(data).ToString());
    }

    #region On Functions
    public void UserConnectedCallBack(string jsonData)
    {
#if SOCKET_ON_LOG
        Debug.Log("User Connected :" + jsonData);
        //  LoadingCanvas.Instance.ShowLoadingPopUp("Searching Player! Please wait");
#endif
        //  Invoke("UserEntered", 3);
        //  Invoke("StartGame", 5);
        GameRequest();
    }

    public void GameRequestCallback(string jsonData)
    {
#if SOCKET_ON_LOG
        Debug.Log("Game Requested :" + jsonData);
#endif
        JSONNode data = JSONNode.Parse(jsonData);
        if (data["status"].Value == ErrorCode.SUCCESS_CODE)
        {
         //   Debug.Log("Show Searching player");
            LoadingCanvas.Instance.ShowLoadingPopUp("Searching Player! Please wait");
        }
    }
    #region EnterUser
    /*
 * 
 * {
"status":"1",
"message":"New user enter to room",
"result":{
"room_id":"5c18ff6fb1caa92f5af24dd3",
"room_name":"ROOM1545142127091",
"roomplayer_id":"5c18ff6fb1caa92f5af24dd4",
"pending_time":10,
"rank":"Starter"
}
}
 * 
 * 
 * */ 
    #endregion
    public void EnterUserCallback(string jsonData)
    {
#if SOCKET_ON_LOG
        Debug.Log("Enter User :" + jsonData);
        // get opponent information here and show user info on screen like name, picture etc
        JSONNode data = JSONNode.Parse(jsonData);
        //now change scene saving the players information
        Database.PutString(Database.Key.ROOM_NAME, data["result"]["room_name"]);
        Database.PutString(Database.Key.ROOM_ID, data["result"]["room_id"]);



        //parse to "PawnPlaced"
        //Call BoardManager to set the pawns
#endif
    }
    #region CallInitJson
    /*
* 
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
*/ 
    #endregion
    public void GameInitCallback(string jsonData)
    {
#if SOCKET_ON_LOG
        Debug.Log("Game init :" + jsonData);
#endif
        Debug.Log("Stop Searching player");
        LoadingCanvas.Instance.HideLoadingPopUp();
        JSONNode data = JSONNode.Parse(jsonData);
        if (data["status"].Value == ErrorCode.SUCCESS_CODE)
        {
            Debug.Log("Take to the Game page");
            //get opponent players name from roomplayers data["result"]["roomplayers"]
            for (int i = 0; i < 2; i++)
            {
                string thisUserId = data["result"]["roomplayers"][i]["user_id"].Value.ToString();
              //  Debug.Log("this user id: "+ Database.GetString(Database.Key.PLAYER_ID) + "___other: "+ data["result"]["roomplayers"][i]["user_id"].Value.ToString());
                if(!thisUserId.Equals(Database.GetString(Database.Key.PLAYER_ID)))
                {
                    string opponentsName = data["result"]["roomplayers"][i]["room_player_name"].Value.ToString();
                   // Debug.Log("Opponents Name: "+ opponentsName);
                    LoadingCanvas.Instance.ShowOnlyInfo("Starting match with player " + opponentsName);
                    Database.PutString(Database.Key.OPPONENT_NAME, opponentsName);
                }
            } //opponent player name           

            string playerOneUserID = data["result"]["roomplayers"][0]["user_id"].Value.ToString();
            if(Database.GetString(Database.Key.PLAYER_ID).Equals(playerOneUserID))            
                MultiplayerManager.SetPlayerTag?.Invoke(Constants.PlayerTag.PLAYER_1);            
            else
                MultiplayerManager.SetPlayerTag?.Invoke(Constants.PlayerTag.PLAYER_2);
        }
        else if (data["status"].Value == ErrorCode.ERROR_STATUS)
        {
            PopupCanvas.Instance.ShowAlertPopUp(data["message"].Value);
            return;
        }

        MultiplayerManager.Instance.SwitchToGameScene();
    }

    //Temporary
   public void LoadedBoardScene()
    {
    //    Debug.Log("Loaded board Scene");
        Dictionary<string, string> data = new Dictionary<string, string>();
       
        data["room_id"] = Database.GetString(Database.Key.ROOM_ID);
        data["room_name"] = Database.GetString(Database.Key.ROOM_NAME);
        data["user_id"] = Database.GetString(Database.Key.PLAYER_ID);
        data["status"] = "ready"; //!!
        socket.EmitJson(GameEmits.playerReady.ToString(), new JSONObject(data).ToString());
    }

    void GameStart(string jsonData)
    {
        Debug.Log("Game Start :"+ jsonData);
        LoadingCanvas.Instance.HideLoadingPopUp();
        BoardManager.InitializeGameRules?.Invoke();
    }
    public void GameWinnerCallback(string jsonData)
    {
#if SOCKET_ON_LOG
        Debug.Log("Game Winner :" + jsonData);
#endif
        JSONNode data = JSONNode.Parse(jsonData);
        if (data["status"].Value == ErrorCode.SUCCESS_CODE)
        {
            if (data["result"]["player_id"].Value == Database.GetString(Database.Key.PLAYER_ID))
            {
                Debug.Log("You win");
            }
            else
            {
                Debug.Log("You lose");
            }
        }
    }

    //dummy callback
    public void StartGame()
    {
        LoadingCanvas.Instance.HideLoadingPopUp();
        SceneManager.LoadScene(1);
    }

    public void UserEntered()
    {
        LoadingCanvas.Instance.ShowLoadingPopUp("Ganesh Joined! Starting Game...");
    }

    public void SendPawnPlacements(PawnPlacements pawnPlacements)
    {
        //Debug.Log("First Pawn" + pawnPlaced.positions[0] + " Seconds Pawn" + pawnPlaced.positions[1] + " Third Pawn" + pawnPlaced.positions[2]);
        string newPawnPlacedData = JsonUtility.ToJson(pawnPlacements);
        Debug.Log("Sending pawn placed data: " + newPawnPlacedData);
        Debug.Log(GameEmits.pawnplacements.ToString());
        socket.EmitJson(GameEmits.pawnplacements.ToString(), newPawnPlacedData);
        GamePlay.instance.StopTurnTimer();       
        GameManager.instance.currentGameState = GameManager.GAMESTATE.NONE;
        //Emit("pawnplacement",newpawnPlacedData);
        //PawnPlaced(newPawnPlacedData); //dont  call this from here        
    }   

    //Call this with the opponent data.....sets pawns for opponent
    public void StartingPawnsPlaced(string data)
    {
        Debug.Log("Got Pawn Data: " + data);
        LoadingCanvas.Instance.HideLoadingPopUp();

        FirstPawnData opponentPawnPlacement = JsonUtility.FromJson<FirstPawnData>(JSON.Parse(data)["result"].ToString());
        BoardManager.instance.PlacePawnForOpponent(opponentPawnPlacement);
    }

    public void OnTurnStart(string data)
    {
        // get the player id of the player will play first
        //Constants.PlayerType
        GameManager.instance.currentGameState = GameManager.GAMESTATE.PLAY;
        //find out if its the local or remote player. 


        //Apply last remote player's turn if any here
        //if not(this. player)
        /*player input to none. 
         * 
         * notification about players turn, timer start
         * 
         * 
         * 
         * */



        if (true) //local player
        {
            //Call OnTurnChanged if 0 for local player
            GameManager.instance.IncreaseTurn(Constants.PlayerType.LOCAL);
        }
        else //if remote player's turn
        {
            //call OnTurnChanged with 1 for remote player
            GameManager.instance.IncreaseTurn(Constants.PlayerType.REMOTE);
        }
    }

   

    public void SubmitTurn(Square from, Square to)
    {
        //TurnData turnData = new TurnData(from.squareId, to.squareId);
        //string turnDataString = JsonUtility.ToJson(turnData);
        //Debug.Log("Player turn data: " + turnDataString);
        //emit("submitTurnData", turnDataString);
    }

    public void PrepareTurnData(GridData f, GridData t)
    {
        TurnData turnData = new TurnData(f, t);
        TurnDataWrapper turnDataWrapper = new TurnDataWrapper( Database.GetString(Database.Key.ROOM_ID), Database.GetString(Database.Key.PLAYER_ID), turnData, Database.GetString(Database.Key.ROOM_NAME));
        string data = JsonUtility.ToJson(turnDataWrapper);
        Debug.Log("Turn Submitted: "+ data);
        socket.EmitJson(GameEmits.turnSubmited.ToString(), data);
        GameManager.instance.currentGameState = GameManager.GAMESTATE.NONE;
    }


    private void OpponentsTurnCallback(string data)
    {
        Debug.Log("Got Opponents Data : " + data);
        var n = JSON.Parse(data);
        string fromSquareID = n["result"]["turnData"]["from"]["SquareID"].Value;
        string toSquareID = n["result"]["turnData"]["to"]["SquareID"].Value;
        Debug.Log("From Square ID " + fromSquareID);
        //foreach (KeyValuePair<string, Square> item in BoardManager.instance.squareCollection)
        //{
        //    Debug.Log("My Key" + item.Key);
        //}
        Square newSquare = BoardManager.instance.GetSquare(fromSquareID);
        newSquare.MovePawn(BoardManager.instance.GetSquare(toSquareID));
    }

    #endregion
}
