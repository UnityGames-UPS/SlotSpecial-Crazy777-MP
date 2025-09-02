using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using DG.Tweening;
using System.Linq;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Best.HTTP.Shared;

public class SocketIOManager : MonoBehaviour
{
    [SerializeField]
    private SlotBehaviour slotManager;

    [SerializeField]
    private UIManager uiManager;
    [SerializeField] private GameManager gameManager;

    internal GameData initialData = null;
    internal UiData initUIData = null;
    internal GameData resultData = null;
    internal Root resultdata = null;
    internal Player playerdata = null;
    internal Message myMessage = null;
    internal double GambleLimit = 0;
    [SerializeField]
    internal List<string> bonusdata = null;
    private SocketManager manager;

    protected string SocketURI = null;
    //protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/"; //HACK: Professional / Dev Server Address
    protected string TestSocketURI = "http://localhost:5000";

    [SerializeField]
    private string testToken;
    internal bool isResultdone = false;
    [SerializeField] internal JSFunctCalls JSManager;
    protected string gameID = "SL-CRZ";
    // protected string nameSpace="game"; //BackendChanges
    protected string nameSpace = "playground"; //BackendChanges
    private Socket gameSocket; //BackendChanges
    //protected string gameID = "";
    //protected string gameID = "";

    internal bool isLoaded = false;
    internal bool SetInit = false;

    private const int maxReconnectionAttempts = 6;
    private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);
    private bool isConnected = false; //Back2 Start
    private bool hasEverConnected = false;
    private const int MaxReconnectAttempts = 5;
    private const float ReconnectDelaySeconds = 2f;

    private float lastPongTime = 0f;
    private float pingInterval = 2f;
    private float pongTimeout = 3f;
    private bool waitingForPong = false;
    private int missedPongs = 0;
    private const int MaxMissedPongs = 5;
    private Coroutine PingRoutine; //Back2 end
    [SerializeField] private GameObject RaycastBlocker;

    private void Start()
    {
        SetInit = false;
        OpenSocket();

        //Debug.unityLogger.logEnabled = false;
    }
    void ReceiveAuthToken(string jsonData)
    {
        Debug.Log("Received data: " + jsonData);

        // Parse the JSON data
        var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
        SocketURI = data.socketURL;
        myAuth = data.cookie;
        nameSpace = data.nameSpace;
        // Proceed with connecting to the server using myAuth and socketURL
    }

    string myAuth = null;

    private void Awake()
    {
        //HTTPManager.Logger = null;
        isLoaded = false;
    }

    private void OpenSocket()
    {
        //Create and setup SocketOptions
        SocketOptions options = new SocketOptions(); //Back2 Start
        options.AutoConnect = false;
        options.Reconnection = false;
        options.Timeout = TimeSpan.FromSeconds(3); //Back2 end
        options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket; //BackendChanges

        //Application.ExternalCall("window.parent.postMessage", "authToken", "*");

#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("authToken");
        StartCoroutine(WaitForAuthToken(options));
#else
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = testToken,
                // gameId = gameID
            };
        };
        options.Auth = authFunction;
        // Proceed with connecting to the server
        SetupSocketManager(options);
#endif
    }

    private IEnumerator WaitForAuthToken(SocketOptions options)
    {
        // Wait until myAuth is not null
        while (myAuth == null)
        {
            yield return null;
        }
        while (SocketURI == null)
        {
            Debug.Log("My Socket is null");
            yield return null;
        }

        // Once myAuth is set, configure the authFunction
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = myAuth,
                // gameId = gameID
            };
        };
        options.Auth = authFunction;

        Debug.Log("Auth function configured with token: " + myAuth);

        // Proceed with connecting to the server
        SetupSocketManager(options);
        yield return null;
    }

    private void SetupSocketManager(SocketOptions options)
    {
        // Create and setup SocketManager
#if UNITY_EDITOR
        this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif

        if (string.IsNullOrEmpty(nameSpace))
        {  //BackendChanges Start
            gameSocket = this.manager.Socket;
        }
        else
        {
            print("nameSpace: " + nameSpace);
            gameSocket = this.manager.GetSocket("/" + nameSpace);
        }
        // Set subscriptions
        gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        gameSocket.On(SocketIOEventTypes.Disconnect, OnDisconnected); //Back2 Start
        gameSocket.On<Error>(SocketIOEventTypes.Error, OnError);
        gameSocket.On<string>("game:init", OnListenEvent);
        gameSocket.On<string>("result", OnListenEvent);
        gameSocket.On<bool>("socketState", OnSocketState);
        gameSocket.On<string>("internalError", OnSocketError);
        gameSocket.On<string>("alert", OnSocketAlert);
        gameSocket.On<string>("pong", OnPongReceived);
        gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice); //BackendChanges Finish

        manager.Open();
    }

    //Connected event handler implementation
    void OnConnected(ConnectResponse resp) //Back2 Start
    {
        Debug.Log("✅ Connected to server.");

        if (hasEverConnected)
        {
            gameManager.CheckAndClosePopups();
        }

        isConnected = true;
        hasEverConnected = true;
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        SendPing();
    } //Back2 end
    private void OnDisconnected() //Back2 Start
    {
        Debug.LogWarning("⚠️ Disconnected from server.");
        isConnected = false;
        ResetPingRoutine();
    } //Back2 end

    private void OnPongReceived(string data) //Back2 Start
    {
        // Debug.Log("✅ Received pong from server.");
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        // Debug.Log($"⏱️ Updated last pong time: {lastPongTime}");
        // Debug.Log($"📦 Pong payload: {data}");
    } //Back2 end

    private void OnError(Error err)
    {
        Debug.LogError("Socket Error Message: " + err);
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("error");
#endif
    }


    private void OnListenEvent(string data)
    {
        Debug.Log("Received some_event with data: " + data);
        ParseResponse(data);
    }
    private void OnSocketState(bool state)
    {
        if (state)
        {
            Debug.Log("my state is " + state);
        }
        else
        {

        }
    }
    private void OnSocketError(string data)
    {
        Debug.Log("Received error with data: " + data);
    }
    private void OnSocketAlert(string data)
    {
        Debug.Log("Received alert with data: " + data);
    }

    private void OnSocketOtherDevice(string data)
    {
        Debug.Log("Received Device Error with data: " + data);
        //uiManager.ADfunction();
    }

    private void SendPing() //Back2 Start
    {
        ResetPingRoutine();
        PingRoutine = StartCoroutine(PingCheck());
    }

    void ResetPingRoutine()
    {
        if (PingRoutine != null)
        {
            StopCoroutine(PingRoutine);
        }
        PingRoutine = null;
    }

    private IEnumerator PingCheck()
    {
        while (true)
        {
            // Debug.Log($"🟡 PingCheck | waitingForPong: {waitingForPong}, missedPongs: {missedPongs}, timeSinceLastPong: {Time.time - lastPongTime}");

            if (missedPongs == 0)
            {
                gameManager.CheckAndClosePopups();
            }

            // If waiting for pong, and timeout passed
            if (waitingForPong)
            {
                if (missedPongs == 2)
                {
                    gameManager.ReconnectionPopup();
                }
                missedPongs++;
                Debug.LogWarning($"⚠️ Pong missed #{missedPongs}/{MaxMissedPongs}");

                if (missedPongs >= MaxMissedPongs)
                {
                    Debug.LogError("❌ Unable to connect to server — 5 consecutive pongs missed.");
                    isConnected = false;
                    gameManager.DisconnectionPopup();
                    yield break;
                }
            }

            // Send next ping
            waitingForPong = true;
            lastPongTime = Time.time;
            // Debug.Log("📤 Sending ping...");
            SendDataWithNamespace("ping");
            yield return new WaitForSeconds(pingInterval);
        }
    } //Back2 end

    private void AliveRequest()
    {
        SendDataWithNamespace("YES I AM ALIVE");
    }

    internal void ReactNativeCallOnFailedToConnect() //BackendChanges
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("onExit");
#endif
    }

    private void ParseResponse(string jsonObject)
    {
        Debug.Log(string.Concat("<color=cyan><b>From SocketManager: ", jsonObject, "</b></color>"));
        Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);
        string id = myData.id;

        switch (id)
        {
            case "initData":
                {
                    initialData = myData.gameData;
                    initUIData = myData.uiData;
                    playerdata = myData.player;
                    //bonusdata = myData.message.BonusData;
                    if (!SetInit)
                    {
                        //Debug.Log(jsonObject);
                        //List<string> InitialReels = ConvertListOfListsToStrings(initUIData.paylines.symbols);
                        List<string> InitialReels = GetReelList(initUIData.paylines.symbols);
                        InitialReels = RemoveQuotes(InitialReels);
                        PopulateSlotSocket(InitialReels);
                        SetInit = true;
                    }
                    else
                    {
                        RefreshUI();
                    }
                    break;
                }
            case "ResultData":
                {
                    //myData.message.GameData.FinalResultReel = ConvertListOfListsToStrings(myData.message.GameData.ResultReel);
                    //myData.message.GameData.FinalsymbolsToEmit = TransformAndRemoveRecurring(myData.message.GameData.symbolsToEmit);
                    //Debug.Log(myData.message.GameData.resultSymbols);
                    resultdata = myData;
                    // resultData = myData.message.GameData;
                    playerdata = myData.player;
                    isResultdone = true;
                    break;
                }
            case "ExitUser":
                {
                    if (gameSocket != null) //BackendChanges
                    {
                        Debug.Log("Dispose my Socket");
                        this.manager.Close();
                    }
                    //   Application.ExternalCall("window.parent.postMessage", "onExit", "*");
#if UNITY_WEBGL && !UNITY_EDITOR
                        JSManager.SendCustomMessage("onExit");
#endif
                    break;
                }
        }
    }

    private void RefreshUI()
    {
        //uiManager.InitialiseUIData(initUIData.AbtLogo.link, initUIData.AbtLogo.logoSprite, initUIData.ToULink, initUIData.PopLink, initUIData.paylines);
    }

    private void PopulateSlotSocket(List<string> LineIds)
    {
        slotManager.shuffleInitialMatrix();

        Debug.Log(string.Concat("<color=blue><b>", LineIds.Count, "</b></color>"));
        //for (int i = 0; i < LineIds.Count; i++)
        //{
        //    //slotManager.FetchLines(LineIds[i], i);
        //    Debug.Log(string.Concat("<color=green><b>", i, "</b></color>"));
        //}

        slotManager.SetInitialUI();

        isLoaded = true;
        // Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnEnter");
#endif
        RaycastBlocker.SetActive(false);
    }

    internal IEnumerator CloseSocket() //Back2 Start
    {
        RaycastBlocker.SetActive(true);
        ResetPingRoutine();

        Debug.Log("Closing Socket");

        manager?.Close();
        manager = null;

        Debug.Log("Waiting for socket to close");

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Socket Closed");

#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit"); //Telling the react platform user wants to quit and go back to homepage
#endif
    } //Back2 end


    internal void AccumulateResult(int currBet)
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.type = "SPIN";
        message.payload = new Data();
        message.payload.betIndex = currBet;
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("request", json);
    }

    private void SendDataWithNamespace(string eventName, string json = null)
    {
        // Send the message
        if (gameSocket != null && gameSocket.IsOpen) //BackendChanges
        {
            if (json != null)
            {
                gameSocket.Emit(eventName, json);
                Debug.Log("JSON data sent: " + json);
            }
            else
            {
                gameSocket.Emit(eventName);
            }
        }
        else
        {
            Debug.LogWarning("Socket is not connected.");
        }
    }
    void CloseGame()
    {
        Debug.Log("Unity: Closing Game");
        StartCoroutine(CloseSocket());
    }

    private List<string> RemoveQuotes(List<string> stringList)
    {
        for (int i = 0; i < stringList.Count; i++)
        {
            stringList[i] = stringList[i].Replace("\"", ""); // Remove inverted commas
        }
        return stringList;
    }

    //private List<string> ConvertListListIntToListString(List<List<int>> listOfLists)
    //{
    //    List<string> resultList = new List<string>();

    //    foreach (List<int> innerList in listOfLists)
    //    {
    //        // Convert each integer in the inner list to string
    //        List<string> stringList = new List<string>();
    //        foreach (int number in innerList)
    //        {
    //            stringList.Add(number.ToString());
    //        }

    //        // Join the string representation of integers with ","
    //        string joinedString = string.Join(",", stringList.ToArray()).Trim();
    //        resultList.Add(joinedString);
    //    }

    //    return resultList;
    //}

    //private List<string> ConvertListOfListsToStrings(List<List<string>> inputList)
    //{
    //    List<string> outputList = new List<string>();

    //    foreach (List<string> row in inputList)
    //    {
    //        string concatenatedString = string.Join(",", row);
    //        outputList.Add(concatenatedString);
    //    }

    //    return outputList;
    //}
    internal List<List<int>> ConvertMatrixToInt(List<List<string>> stringMatrix)
    {
        var intMatrix = new List<List<int>>();

        if (stringMatrix == null) return intMatrix; // return empty if null

        foreach (var row in stringMatrix)
        {
            var intRow = new List<int>();

            if (row != null)
            {
                foreach (var value in row)
                {
                    if (int.TryParse(value, out int result))
                    {
                        intRow.Add(result);
                    }
                    else
                    {
                        // if conversion fails, you can decide what to do
                        // here I add 0, but you could also throw an exception
                        intRow.Add(0);
                    }
                }
            }

            intMatrix.Add(intRow);
        }

        return intMatrix;
    }

    internal static List<int> ToIntList(List<string> stringList)
    {
        List<int> intList = new List<int>();
        foreach (string str in stringList)
        {
            if (int.TryParse(str, out int result))
                intList.Add(result);
            else
                intList.Add(0); // fallback if parse fails
        }
        return intList;
    }

    private List<string> GetReelList(List<Symbol> m_List)
    {
        List<string> m_ResultList = new List<string>();

        foreach (var m in m_List)
        {
            m_ResultList.Add(m.id.ToString());
        }

        return m_ResultList;
    }

    private List<string> TransformAndRemoveRecurring(List<List<string>> originalList)
    {
        // Flattened list
        List<string> flattenedList = new List<string>();
        foreach (List<string> sublist in originalList)
        {
            flattenedList.AddRange(sublist);
        }

        // Remove recurring elements
        HashSet<string> uniqueElements = new HashSet<string>(flattenedList);

        // Transformed list
        List<string> transformedList = new List<string>();
        foreach (string element in uniqueElements)
        {
            transformedList.Add(element.Replace(",", ""));
        }

        return transformedList;
    }
}

[Serializable]
public class BetData
{
    public double currentBet;
    public double currentLines;
    public double spins;
}

[Serializable]
public class AuthData
{
    public string GameID;
}

[Serializable]
public class MessageData
{
    // public BetData data;
    // public string id;

    public string type;
    public Data payload;
}
[Serializable]
public class Data
{
    public int betIndex;
    public string Event;
    public double lastWinning;
    public int index;

}
[Serializable]
public class ExitData
{
    public string id;
}

[Serializable]
public class InitData
{
    public AuthData Data;
    public string id;
}

[Serializable]
public class AbtLogo
{
    public string logoSprite { get; set; }
    public string link { get; set; }
}

public class GameData
{
    //  public List<double> bets { get; set; }
    public List<int> autoSpin { get; set; }
    public List<List<int>> resultSymbols { get; set; }
    public bool isFreeSpin { get; set; }
    public int freeSpinCount { get; set; }

    public List<List<int>> lines { get; set; }
    public List<double> bets { get; set; }


}
[Serializable]
public class Features
{
    public int defaultPayout { get; set; }
    public Respin respin { get; set; }
}

[Serializable]
public class Respin
{
    public int min { get; set; }
    public int max { get; set; }
}

// [Serializable]
// public class UiData
// {
//     public Paylines paylines { get; set; }
// }

public class Message
{
    public GameData GameData { get; set; }
    public UiData UIData { get; set; }
    public Player PlayerData { get; set; }
}

public class Paylines
{
    public List<Symbol> symbols { get; set; }
}

public class Player
{
    public double balance { get; set; }
}

public class Root
{
    //public string id { get; set; }
    public Message message { get; set; }
    // public string username { get; set; }

    public string id { get; set; }
    public GameData gameData { get; set; }
    public Features features { get; set; }
    public UiData uiData { get; set; }
    public Player player { get; set; }

    public bool success { get; set; }
    public List<List<string>> matrix { get; set; }
    public Payload payload { get; set; }

}
[Serializable]
public class Payload
{
    public double currentWinning { get; set; }
    public bool isRespin { get; set; }
    public int respinCount { get; set; }
    public Win win { get; set; }
}

[Serializable]
public class Win
{
    public string type { get; set; }
    public int symbolId { get; set; }
    public string specialSymbolId { get; set; }
    public string special { get; set; }
}


// [Serializable]
// public class Player
// {
//     public double balance { get; set; }
// }


// public class Symbol
// {
//     public int ID { get; set; }
//     public string Name { get; set; }
//     public object multiplier { get; set; }
//     public object defaultAmount { get; set; }
//     public object symbolsCount { get; set; }
//     public object increaseValue { get; set; }
//     public object description { get; set; }
//     public object payout { get; set; }
//     public object mixedPayout { get; set; }
//     public object defaultPayout { get; set; }
// }

[Serializable]
public class Symbol
{
    public int id { get; set; }
    public string name { get; set; }
    public List<object> multiplier { get; set; }
    public int payout { get; set; }
    public int mixedPayout { get; set; }
    public object isSpecialCrz { get; set; }
    public string specialType { get; set; }
    public string description { get; set; }
}

public class Multiplier
{
}

public class IncreaseValue
{
}

public class DefaultAmount
{
}

public class DefaultPayout
{
}

public class Description
{
}

public class SymbolsCount
{
}

public class UiData
{
    public Paylines paylines { get; set; }
    public List<object> spclSymbolTxt { get; set; }
    public AbtLogo AbtLogo { get; set; }
    public string ToULink { get; set; }
    public string PopLink { get; set; }
}

[Serializable]
public class AuthTokenData
{
    public string cookie;
    public string socketURL;
    public string nameSpace;
}