using TMPro;
using System;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using GamerWolf.Utils;
using System.Collections;
using Baracuda.Monitoring;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
[RequireComponent(typeof(PhotonView))]
public class MatchHandler : MonoBehaviourPunCallbacks, IOnEventCallback {

    [Serializable]
    public class TrackManagers {
        public TrackTypes tracks;
        public GameObject trackHolder;
        public TrackCheckPoint trackCheckPoint;
        public CinemachineCameraMovementAnimations cinemachineCameraMovementAnimations;
        public Transform[] spawnPointsForTrack;
    }
    public static MatchHandler Current{get;private set;}
    public static event EventHandler OnRaceStart;


#region MONITORING.........
    [Monitor,MPosition(UIPosition.UpperRight)] private string connectionstatus;
    [SerializeField] private List<GamePlayerInfo> playerInfoList;
    [SerializeField,Monitor] private List<GamePlayerInfo> finishedCarList;
    [SerializeField] private List<int> currentActorNumbers = new List<int> {1,2,3,4,5,6};
    [SerializeField] private List<CarDriverAI> parkedBotDriversCar;
    private int totalLaps = 1,totlaCheckPointPerLaps;
    private GameState gameState;
    private float waitTimeCurrent;
    private List<Transform> driversList;

#endregion

    [SerializeField] private List<TrackManagers> trackManagersAreasList;
    [SerializeField] private Material trackMinimapMaterial;
    [SerializeField] private GameSettingsSO gameSettings;
    [SerializeField] private TextMeshProUGUI[] lapsCountTmProList,TotalPlayerCountTmproList;
    [SerializeField] private List<LeaderboardPlayerCard> leaderboardPlayerCardFinalList;
    [SerializeField] private List<PlayerJoinedCard> joinedPlayerCardList;
    [SerializeField] private GameObject humanPlayerCar,aiBotCar;
    [SerializeField] private GameObject endWindow;
    [SerializeField] private bool perpetual = true;
    [SerializeField] private bool playerAdded;
    [SerializeField] private int spawnIndex;
    [SerializeField] private StartingLight startingLight;
    [SerializeField] private UIControllers uIControllers;
    [SerializeField] private GamePlayTimeManager gamePlayTimeManager;
    [SerializeField] private Transform cameraTracker;


//! Private Variables............
    private CinemachineCameraMovementAnimations cinemachineCameraMovementAnimations;
    private TrackCheckPoint currentTrackCheckPoint;
    private bool isLeaderBoardOpen = false,isTimeUp = false;
    private float startTime;
    [Monitor] private int myind = -1;
    private bool botIsAlreadySpawned;
    private float finalTimeToFinishRace;
    private void Awake(){
        Monitor.StartMonitoring(this);
        // Or use this extension method:
        this.StartMonitoring();
        if(Current == null){
            Current = this;
        } else{
            Destroy(Current.gameObject);
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        botIsAlreadySpawned = false;
    }
    private void OnDestroy() {
        Monitor.StopMonitoring(this);
        // Or use this extension method:
        this.StopMonitoring();
    }
    private void Start(){
        for (int i = 0; i < leaderboardPlayerCardFinalList.Count; i++) {
            leaderboardPlayerCardFinalList[i].HideCard();
        }
        for (int i = 0; i < joinedPlayerCardList.Count; i++) {
            joinedPlayerCardList[i].HideCard();
        }
        AudioManager.Current.StopAudio(Sounds.SoundType.TrackDay_Loop_Standerd_1);
        AudioManager.Current.PlayMusic(Sounds.SoundType.TrackDay_Loop_2);
        startingLight.SetLightColor(Color.red);
        trackMinimapMaterial.mainTexture = gameSettings.currentTrack.trackTrackTexture;
        driversList = new List<Transform>();
        playerInfoList = new List<GamePlayerInfo>();
        currentActorNumbers = new List<int> {1,2,3,4,5,6};
        finishedCarList = new List<GamePlayerInfo>();
        for (int i = 0; i < trackManagersAreasList.Count; i++) {
            if(trackManagersAreasList[i] != GetCurrentTrackManagers()){
                trackManagersAreasList[i].trackHolder.SetActive(false);
            }else{
                trackManagersAreasList[i].trackHolder.SetActive(true);
            }
        }
        currentTrackCheckPoint = GetCurrentTrackManagers().trackCheckPoint;
        cinemachineCameraMovementAnimations = GetCurrentTrackManagers().cinemachineCameraMovementAnimations;
        cinemachineCameraMovementAnimations.StartCameraMovement();
        startTime = KeysHolder.MAX_STARTING_TIME;
        totalLaps = GetTotalLapCount();
        ValidateConnection();
        totlaCheckPointPerLaps = currentTrackCheckPoint.GetTotalCheckPointCount();
        NewPlayer_S(gameSettings.gameProfileData,false);
        uIControllers.ShowHidePlayHud(false);
        uIControllers.ShowHideEndGameWindow(false);
        uIControllers.ShowHideRankWindow(false);
        uIControllers.ShowHideStartingTimeWindow(false);
        uIControllers.ShowHideWaitForOthersToFinishWindow(false);
        finalTimeToFinishRace = KeysHolder.FINALFINISHINGTIME;
        gameState = GameState.WaitingForPlayer;
        uIControllers.ShowHideFinalRaceEndTimeOtherPlayerHud(false);
        uIControllers.ShowHideFinalRaceEndTimeWinnerPlayerHud(false);
        gameSettings.gameData.IncreaseTotalRacesJoinedCount(1);
        cameraTracker.position = GetCurrentTrackManagers().spawnPointsForTrack[0].position;
        StartCoroutine(PingServer());
        if (PhotonNetwork.IsMasterClient){
            RefreshTimer_S();
            playerAdded = true;
            // Spawning a New Player.. in the Local Machine.......
            SpawnPlayer();
        }
    }
    public override void OnMasterClientSwitched(Player newMasterClient) {
        Debug.Log("New Master Client " + newMasterClient);
        base.OnMasterClientSwitched(newMasterClient);
    }
    private void OnDrawGizmos(){
        Gizmos.color = Color.cyan;
        foreach (TrackManagers track in trackManagersAreasList){
            foreach(Transform spawnPoint in track.spawnPointsForTrack){
                Gizmos.DrawSphere(spawnPoint.position,.1f);
            }
        }
    }
    private void ValidateConnection () {
        if (PhotonNetwork.IsConnected) return;
        SceneManager.LoadSceneAsync(0);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer) {
        Debug.LogError(otherPlayer.NickName + " Left the Room");
        if(PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers){
            if(gameState == GameState.WaitingForPlayer){
                foreach(GamePlayerInfo playerInfo in playerInfoList){
                    if(playerInfo.actorId == otherPlayer.ActorNumber){
                        playerInfoList.Remove(playerInfo);
                    }
                }
            }
        }
        RefershJoiningWindow();
    }
    public override void OnLeftRoom() {
        if(PhotonNetwork.IsMasterClient){
            Debug.LogError("Master Client Left the Room");
            int randomPlayerNumber = Random.Range(0,PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.CurrentRoom.SetMasterClient(PhotonNetwork.CurrentRoom.Players[randomPlayerNumber]);
        }
        base.OnLeftRoom();
    }
    private IEnumerator PingServer() {
        while (true) {
            float ping = PhotonNetwork.GetPing();
            uIControllers.SetPing(ping);
            yield return new WaitForSeconds(1f);
        }
    }

    private void Update(){
        connectionstatus = PhotonNetwork.NetworkClientState.ToString();
        switch(gameState){
            case GameState.WaitingForPlayer:
                if(!IsRoomFull()){
                    if(PhotonNetwork.IsMasterClient){
                        if(gameSettings.hasBots){
                            if(!isTimeUp){
                                CheckBotCarSpawning();
                            }
                        }
                    }
                    return;
                }
                uIControllers.WaitforPlayerToJoinWindow(0,false,PhotonNetwork.IsMasterClient);
                uIControllers.ShowHideEndGameWindow(false);
                uIControllers.ShowHideStartingTimeWindow(false);
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                CancelInvoke(nameof(PlaySoundAfterDelay));
                Invoke(nameof(PlaySoundAfterDelay),KeysHolder.START_SOUND_DELAY);
                gameState = GameState.Starting;
                break;
            case GameState.Starting:
                cinemachineCameraMovementAnimations.StopCameraMovement();
                uIControllers.ShowHideEndGameWindow(false);
                uIControllers.WaitforPlayerToJoinWindow(0,false,PhotonNetwork.IsMasterClient);
                if(GetTotalActivePlayer() != PhotonNetwork.CurrentRoom.MaxPlayers){
                    return;
                }
                foreach (GameObject playerObject in GameObject.FindGameObjectsWithTag("Player")) {
                    if(playerObject.TryGetComponent(out CarDriverAI carDriverAI)){
                        if(carDriverAI.IsReady()){
                            if(!driversList.Contains(carDriverAI.transform)){
                                driversList.Add(carDriverAI.transform);
                            }
                        }
                    }
                    if(playerObject.TryGetComponent(out CarDriver carDriver)){
                        if(!driversList.Contains(playerObject.transform)){
                            driversList.Add(playerObject.transform);
                        }
                        if(!carDriver.IsReady()){
                            return;
                        }
                    }
                }
                uIControllers.ShowHideStartingTimeWindow(true);
                startTime -= Time.deltaTime;
                uIControllers.ShowStartTime(Mathf.CeilToInt(startTime));
                float nomralizedStartTime = startTime / KeysHolder.MAX_STARTING_TIME;
                if(nomralizedStartTime < .7f && nomralizedStartTime > .4f){
                    startingLight.SetLightColor(Color.red);
                }else if(nomralizedStartTime <= .06f){
                    startingLight.SetLightColor(Color.green);
                }
                if(startTime <= 0f){
                    cinemachineCameraMovementAnimations.StopCameraMovement();
                    gamePlayTimeManager.StartTimer();
                    uIControllers.WaitforPlayerToJoinWindow(0,false);
                    uIControllers.ShowHideWaitForOthersToFinishWindow(false);
                    currentTrackCheckPoint.SetUp(driversList);
                    uIControllers.ShowHideStartingTimeWindow(false);
                    InitializeUI();
                    OnRaceStart?.Invoke(this,EventArgs.Empty);
                    RefreshTimer_S();
                    gameState = GameState.Playing;
                }
                break;
            case GameState.Playing:
                if(finishedCarList.Count > 0){
                    finalTimeToFinishRace -= Time.deltaTime;
                    RefreshTimer_S();
                    if(finalTimeToFinishRace<= 0f){
                        finalTimeToFinishRace = 0f;
                        RefreshTimer_S();
                    }
                }
                break;
            case GameState.Ending:
                uIControllers.WaitforPlayerToJoinWindow(0,false);
                uIControllers.ShowHideWaitForOthersToFinishWindow(false);
                uIControllers.ShowHidePlayHud(false);
                uIControllers.ShowHideEndGameWindow(true);
                break;
        }
    }
    

    private float GetCurrentMaxWaitTime() {
        return (float)PhotonNetwork.CurrentRoom.CustomProperties[KeysHolder.WAITING_FOR_PLAYER_TIME_KEY];
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
        Debug.Log("Changed Properties "+ propertiesThatChanged);
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
    }
    private void CheckBotCarSpawning(){
        if(isTimeUp){
            return;
        }
        float waitTimeNormalized = waitTimeCurrent / GetCurrentMaxWaitTime();
        if(waitTimeNormalized >= .8f){
            if(!botIsAlreadySpawned){
                SetBots();
                botIsAlreadySpawned = true;
            }
        }
        if(waitTimeCurrent >= GetCurrentMaxWaitTime()){
            isTimeUp = true;
            waitTimeCurrent = GetCurrentMaxWaitTime();
            if(botIsAlreadySpawned){
                uIControllers.WaitforPlayerToJoinWindow(waitTimeCurrent,false);
                uIControllers.ShowHideEndGameWindow(false);
                uIControllers.ShowHideStartingTimeWindow(false);
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                CancelInvoke(nameof(PlaySoundAfterDelay));
                Invoke(nameof(PlaySoundAfterDelay),KeysHolder.START_SOUND_DELAY);
                gameState = GameState.Starting;
            }
        }else{
            waitTimeCurrent += Time.deltaTime;
            uIControllers.WaitforPlayerToJoinWindow(waitTimeCurrent,true,PhotonNetwork.IsMasterClient);
        }
    }
    private void PlaySoundAfterDelay(){
        AudioManager.Current.StopAudio(Sounds.SoundType.TrackDay_Loop_1);
        AudioManager.Current.PlayOneShotMusic(Sounds.SoundType.Start_Count_Down_Beep);
    }
    public void OnLocalPlayerFinishRace(int actorId){
        uIControllers.ShowHideWaitForOthersToFinishWindow(true,GetPlayerRank(actorId));
    }
    private void SetBots(){
        // botActorNumber = currentActorNumbers[0];
        spawnIndex = PhotonNetwork.CurrentRoom.PlayerCount - 1;
        int actorToSpawn = PhotonNetwork.CurrentRoom.MaxPlayers - PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log("Cars Left to Spawn is " + actorToSpawn);
        for (int a = 0; a < actorToSpawn; a++) {
            Debug.Log("Spawning Bots");
            photonView.RPC(nameof(SpawnBotsRPC),RpcTarget.AllBuffered);
        }
    }
    private CarDriverAI GetBotDrivers(){
        int randomCar = Random.Range(0,parkedBotDriversCar.Count);
        for (int i = 0; i < parkedBotDriversCar.Count; i++) {
            if(i == randomCar){
                CarDriverAI driverAI = parkedBotDriversCar[randomCar];
                parkedBotDriversCar.Remove(driverAI);
                return driverAI;
            }
        }
        return null;
    }

    [PunRPC]
    private void SpawnBotsRPC(){
        int botActorNumber = currentActorNumbers[GetTotalActivePlayer()];
        string botName = string.Concat("Bot ",Random.Range(10,10000));
        Debug.Log("Bot Spawned = Actor Number " + botActorNumber);
        spawnIndex ++;
        CarDriverAI botAi = GetBotDrivers();
        Transform currentSpawnPoint = GetCurrentTrackManagers().spawnPointsForTrack[spawnIndex];
        GameProifileData botProfile = new GameProifileData(botName,botAi.GetCarTypeIndex());
        NewPlayer_S(botProfile,true,botActorNumber);
        botAi.transform.position = currentSpawnPoint.position;
        botAi.transform.rotation = currentSpawnPoint.rotation;
        botAi.gameObject.SetActive(true);
        botAi.SetTotalLapsRpc(GetTotalLapCount());
        botAi.SetCheckPointList(GetCurrentTrackManagers().trackCheckPoint.GetCheckPointList());
        botAi.SetActorNumber(botActorNumber);
    }
    public TrackTypes GetTracks(){
        return (TrackTypes)PhotonNetwork.CurrentRoom.CustomProperties[KeysHolder.TRACK_KEY];
    }
    public TrackManagers GetCurrentTrackManagers(){
        TrackManagers currentTrackManager = new TrackManagers();
        for (int i = 0; i < trackManagersAreasList.Count; i++) {
            if(trackManagersAreasList[i].tracks == GetTracks()){
                currentTrackManager = trackManagersAreasList[i];
            }
        }
        return currentTrackManager;
    }
    public int GetTotalLapCount(){
        return (int)PhotonNetwork.CurrentRoom.CustomProperties[KeysHolder.LAP_KEY];
    }
    public override void OnDisconnected(DisconnectCause cause){
        Debug.LogError("Disconnected Due to " + cause);
        if (this.CanRecoverFromDisconnect(cause)) {
            this.Recover();
        }else {
            SceneManager.LoadScene(1);
        }
        base.OnDisconnected(cause);
    }
    private bool CanRecoverFromDisconnect(DisconnectCause cause) {
        switch (cause) {
            // the list here may be non exhaustive and is subject to review
            case DisconnectCause.Exception:
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.DisconnectByServerLogic:
            case DisconnectCause.DisconnectByServerReasonUnknown:
                return true;
        }
        return false;
    }

    private void Recover() {
        if (!PhotonNetwork.ReconnectAndRejoin()) {
            Debug.LogError("ReconnectAndRejoin failed, trying Reconnect");
            if (!PhotonNetwork.Reconnect()) {
                Debug.LogError("Reconnect failed, trying ConnectUsingSettings");
                if (!PhotonNetwork.ConnectUsingSettings()) {
                    Debug.LogError("ConnectUsingSettings failed");
                    SceneManager.LoadScene(1);
                }
            }
        }
    }
    public int GetTotalCheckPointCount(){
        return currentTrackCheckPoint.GetTotalCheckPointCount();
    }
    private void InitializeUI () {
        uIControllers.ShowHidePlayHud(true);
        RefreshMyStats();
    }
    public int GetTotalActivePlayer(){
        int totalActivePlayer = 0;
        foreach(GameObject playerObject in GameObject.FindGameObjectsWithTag("Player")){
            if(playerObject.TryGetComponent(out CarDriver driver)){
                totalActivePlayer++;
            }
            if(playerObject.TryGetComponent(out CarDriverAI driverAI)){
                if(driverAI.IsReady()){
                    totalActivePlayer++;
                }
            }
        }
        Debug.Log("Total Active Cars Live is = " + totalActivePlayer);// !last Coded.........
        return totalActivePlayer;
    }
    public bool IsRoomFull(){
        return PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
    }
    public GameState GetGameState(){
        return gameState;
    }


#region Codes

    public enum EventCodes : byte {
        NewPlayer,
        UpdatePlayers,
        ChangeStat,
        NewMatch,
        RefreshTimer,
    }

    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code >= KeysHolder.PHOTON_CODE_MAX) return;

        EventCodes eventCodes = (EventCodes) photonEvent.Code;
        object[] customDataObjects = (object[]) photonEvent.CustomData;

        switch (eventCodes){
            case EventCodes.NewPlayer:
                NewPlayer_R(customDataObjects);
                break;

            case EventCodes.UpdatePlayers:
                UpdatePlayers_R(customDataObjects);
                break;

            case EventCodes.ChangeStat:
                ChangeStat_R(customDataObjects);
                break;

            case EventCodes.NewMatch:
                NewMatch_R();
                break;
            case EventCodes.RefreshTimer:
                RefreshTimer_R(customDataObjects);
                break;
        }
    }
    public override void OnEnable() {
        PhotonNetwork.AddCallbackTarget(this);
        base.OnEnable();
    }
    public override void OnDisable() {
        PhotonNetwork.RemoveCallbackTarget(this);
        base.OnDisable();
    }



#endregion

    public void LeaveRoom(){
        // EndGame();
        // disconnect
        uIControllers.OnLeaveRoomBtnClick();
        PhotonNetwork.AutomaticallySyncScene = false;
        SceneManager.LoadSceneAsync(KeysHolder.MENU_SCENE_INDEX);
    }
    public void NewPlayer_S (GameProifileData p,bool bot,int botNumber = -1) {
        // bool isBot = bot;
        int actor = bot ? botNumber : PhotonNetwork.LocalPlayer.ActorNumber;
        if(bot){
            if(!CanAddeNewBotPlayers(actor)){
                return;
            }
        }
        object[] package = new object[KeysHolder.MAX_PACKAGE_ARRAY_LENGTH];
        Debug.Log(p.username + "With Car " + p.carTypeShop);
        package[0] = p.username;
        package[1] = /* PhotonNetwork.LocalPlayer.ActorNumber */actor;//& Local Actor Number......
        package[2] = (int) 0;//? currentLaps
        package[3] = (int) 0; //* current CheckPoint Count PerLevel.
        package[4] = (bool) false;//^ Check for ReachedFinishedLine...
        package[5] = (bool) bot; //! Check is Bot.....
        package[6] = (float) 0;// finishing Time..
        package[7] = (int) p.carTypeShop;// Car Type Index

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }
    private bool CanAddeNewBotPlayers(int actor){
        bool addingBot = true;// flag to check adding a bot is Not already present...
        foreach(GamePlayerInfo playerInfo in playerInfoList){
            if(playerInfo.actorId == actor){
                addingBot = false;
                break;
            }
        }
        return addingBot;
    }
    private bool CanAddeBotPlayersToFinalEndList(int actor){
        bool addingBot = true;// flag to check adding a bot is Not already present...
        foreach(GamePlayerInfo playerInfo in finishedCarList){
            if(playerInfo.actorId == actor){
                addingBot = false;
                break;
            }
        }
        return addingBot;
    }
    public void NewPlayer_R (object[] data) {
        GameProifileData profileData = new GameProifileData((string)data[0],(int)data[7]/* Car Type Index */);//& setting UserName.....
        GamePlayerInfo p = new GamePlayerInfo (
            profileData,
            (int) data[1],//! Actor Number..
            (int) data[2],//^ currentLap Number..
            (int)data[3],//* current CheckPoint Count PerLevel.
            (bool) data[4],// ?reached Finished Line.
            (bool)data[5],// ~isBot
            (float)data[6]// finishedLineReachedTime.
            
        );
        if(p.isBot){
            if(CanAddeNewBotPlayers(p.actorId)){
                playerInfoList.Add(p);
            }
        }else{
            playerInfoList.Add(p);
        }
        RefershJoiningWindow();

        //resync our local player information with the new player
        foreach (GameObject playerObject in GameObject.FindGameObjectsWithTag("Player")){
            if(playerObject.TryGetComponent(out CarDriver carDrivers)){
                carDrivers.TrySync();
            }
            if(playerObject.TryGetComponent(out CarDriverAI carDriverAI)){
                carDriverAI.TrySync();
            }
        }

        UpdatePlayers_S((int)gameState, playerInfoList);
    }

    public void UpdatePlayers_S (int state, List<GamePlayerInfo> info) {
        object[] package = new object[info.Count + 1];

        package[0] = state;
        for (int i = 0; i < info.Count; i++) {
            object[] piece = new object[KeysHolder.MAX_PACKAGE_ARRAY_LENGTH];

            piece[0] = info[i].gameProifile.username;
            piece[1] = info[i].actorId;// & Local Player Actor Number
            piece[2] = info[i].currentLap;//^ currentLap...
            piece[3] = info[i].currentCheckPointCount;//~ Check Point Crossed Count.
            piece[4] = info[i].reachedFinishedLine;//? reached Finised Line...
            piece[5] = info[i].isBot;// gameFinishedTime........
            piece[6] = info[i].gameFinishedTime;// gamefinished Time.
            piece[7] = info[i].gameProifile.carTypeShop;// *Car Type Index
            package[i + 1] = piece;
        }

        PhotonNetwork.RaiseEvent (
            (byte)EventCodes.UpdatePlayers, 
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, 
            new SendOptions { Reliability = true }
        );
    }
    public void UpdatePlayers_R (object[] data) {
        gameState = (GameState)data[0];

        //* check if there is a new player
        if (playerInfoList.Count < data.Length - 1) {
            foreach (GameObject playerObject in GameObject.FindGameObjectsWithTag("Player")) {
                //* if so, resync our local player information
                if(playerObject.TryGetComponent(out CarDriver carDriver)){
                    carDriver.TrySync();
                }
                if(playerObject.TryGetComponent(out CarDriverAI carDriverAI)){
                    if(carDriverAI.IsReady()){
                        carDriverAI.TrySync();
                    }
                }
            }
        }

        playerInfoList = new List<GamePlayerInfo>();

        for (int i = 1; i < data.Length; i++) {
            object[] extract = (object[]) data[i];

            GamePlayerInfo p = new GamePlayerInfo (
                new GameProifileData (
                    (string) extract[0],
                    (int)extract[7]// CarType Index.
                ),
                (int) extract[1],//! Actor Number.
                (int) extract[2],//^ currentLap...
                (int) extract[3],//~ Check Point Crossed Count.
                (bool) extract[4],//? reached Finised Line...
                (bool) extract[5], //! Check isBot........
                (float) extract[6]//* Finished Time...........
            );

            playerInfoList.Add(p);
            RefershJoiningWindow();
            if (PhotonNetwork.LocalPlayer.ActorNumber == p.actorId) {
                myind = i - 1;

                //* if Not Master Client then spawn us in
                if (!playerAdded) {
                    playerAdded = true;
                    Debug.LogError("Spawn not from Master Clint");
                    SpawnPlayer();// to Activate......
                }
            }
        }

        StateCheck();
    }
    private void ScoreCheck () {
        // define temporary variables
        bool detectwin = true;

        // check to see if any player has met the win conditions
        for (int i = 0; i < playerInfoList.Count; i++) {
            if(!playerInfoList[i].reachedFinishedLine /* && !playerInfoList[i].isBot */){
                detectwin = false;
                // Debug.LogError("Game Not Finished......");
                break;
            }else{
                GamePlayerInfo currentInfo = playerInfoList[i];
                if(!finishedCarList.Contains(currentInfo)){
                    if(currentInfo.isBot){
                        if(CanAddeBotPlayersToFinalEndList(currentInfo.actorId)){
                            finishedCarList.Add(currentInfo);
                        }
                    }else{
                        finishedCarList.Add(currentInfo);
                    }
                    // finishedCarList.Add(currentInfo);
                    Debug.LogError(currentInfo.gameProifile.username + " Reached Finished Line");
                }
            }
        }
        /* foreach (GamePlayerInfo a in playerInfoList) {
            // free for all
            if(a.currentLap < totalLaps) {
                detectwin = false;
                break;
            }else{
            }
        } */

        // did we find a winner?
        if (detectwin) {
            Debug.LogError("Game Over Reached..........");
            // are we the master client? is the game still going?
            if (PhotonNetwork.IsMasterClient && gameState != GameState.Ending) {
                // finishedCarList.Reverse();
                // if so, tell the other players that a winner has been detected
                UpdatePlayers_S((int)GameState.Ending, playerInfoList);
            }
        }
    }
    private void ServerEndingGame(){
        for (int i = 0; i < playerInfoList.Count; i++) {
            if(!playerInfoList[i].reachedFinishedLine){
                GamePlayerInfo currentInfo = playerInfoList[i];
                if(!finishedCarList.Contains(currentInfo)){
                    if(currentInfo.isBot){
                        if(CanAddeBotPlayersToFinalEndList(currentInfo.actorId)){
                            finishedCarList.Add(currentInfo);
                        }
                    }else{
                        finishedCarList.Add(currentInfo);
                    }
                    Debug.LogError(currentInfo.gameProifile.username + " Settings all the UnReached Drivers");
                }
            }
        }
        // did we find a winner?
        Debug.LogError("Game Over After First Car Reached..........");
        // are we the master client? is the game still going?
        if (PhotonNetwork.IsMasterClient && gameState != GameState.Ending) {
            // finishedCarList.Reverse();
            // if so, tell the other players that a winner has been detected
            UpdatePlayers_S((int)GameState.Ending, playerInfoList);
        }
    }
    private void StateCheck () {
        if (gameState == GameState.Ending) {
            EndGame();
        }
    }

    public void ChangeStat_S (int actor, byte isDeathOrKiller, byte currentLap,int checkPointCrossCount,byte reachedFinishedLine,byte isBot,float finishedLineReachedTime){
        object[] package = new object[] { actor, isDeathOrKiller, currentLap,checkPointCrossCount, reachedFinishedLine,isBot,finishedLineReachedTime};

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ChangeStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }
    
    public void ChangeStat_R (object[] data){
        int actor = (int) data[0];// actor Number...
        byte deathorkilled = (byte) data[1];// is Death Or Killer.....
        byte lapAmt = (byte) data[2];// current Laps Count;...........
        int checkPointCount  = (int)data[3]; // checkPointCrossCount
        byte reachedFinishedLine = (byte) data[4];// finishedLine.........
        byte isBot = (byte)data[5];// isBot
        float finishedLineReachedTime = (float)data[6];// Finished Race Time.........
        // int cartypeIndex = (int)data[7];carTypeIndex..........
        for (int i = 0; i < playerInfoList.Count; i++){
            if(playerInfoList[i].actorId == actor) {
                if(deathorkilled == 0){//!Check Laps
                    playerInfoList[i].currentLap = lapAmt;
                    playerInfoList[i].isBot = isBot == 1 ? true : false;
                    playerInfoList[i].currentCheckPointCount = checkPointCount;// CheckPoint Count.
                    // playerInfoList[i].gameProifile.carTypeShop = cartypeIndex;// carTypeIndex
                    playerInfoList[i].reachedFinishedLine = reachedFinishedLine == 1 ? true : false;// 1 is true and 0 is false.........
                    playerInfoList[i].gameFinishedTime = finishedLineReachedTime;// Finished Line RecahedTime.....
                    // Debug.LogError($"Player {playerInfoList[i].gameProifile.username} : Current Laps = {playerInfoList[i].currentLap} : Reached Finished Line = {playerInfoList[i].reachedFinishedLine}");
                }
                if(i == myind) RefreshMyStats();
                /* if (LeaderBoardWindowInGame.activeSelf) */RefershJoiningWindow(); 
                break;
            }
        }

        ScoreCheck();
    }
    public void RefreshTimer_R(object[] data) {
        finalTimeToFinishRace = (float)data[0];
        // Refreshing The Ending Timer If Any Car Reached the FinishedLine;
        RefreshGameEndTimeUI();
    }
    private void RefreshGameEndTimeUI(){//TODO Working Here............
        if(gameState == GameState.Playing){
            if(finishedCarList.Count > 0){
                uIControllers.SetWinnerGameEndTime(Mathf.CeilToInt(finalTimeToFinishRace));
                uIControllers.SetOtherGameEndTime(Mathf.CeilToInt(finalTimeToFinishRace));
                for (int i = 0; i < playerInfoList.Count; i++) {
                    if(playerInfoList[i].actorId == PhotonNetwork.LocalPlayer.ActorNumber){
                        if(playerInfoList[i].reachedFinishedLine){
                            /* <Summury>
                             Check if this car reached the finished Line if Yes then Show the Rank Window Text Box.
                            </Summury> */
                            uIControllers.ShowHideFinalRaceEndTimeWinnerPlayerHud(true);
                            uIControllers.ShowHideFinalRaceEndTimeOtherPlayerHud(false);
                        }else{
                            /* <Summury>
                             Check if this car Not Reached the finished Line if Yes then Show the Driving Window Text Box.
                            </Summury> */
                            uIControllers.ShowHideFinalRaceEndTimeOtherPlayerHud(true);
                            uIControllers.ShowHideFinalRaceEndTimeWinnerPlayerHud(false);
                        }
                    }
                }
                if(finalTimeToFinishRace <= 0f){
                    uIControllers.ShowHideFinalRaceEndTimeOtherPlayerHud(false);
                    uIControllers.ShowHideFinalRaceEndTimeWinnerPlayerHud(false);
                    ServerEndingGame();
                }
            }else{
                uIControllers.ShowHideFinalRaceEndTimeOtherPlayerHud(false);
                uIControllers.ShowHideFinalRaceEndTimeWinnerPlayerHud(true);
            }
        }
    }
    private void RefreshMyStats() {
        List<GamePlayerInfo> driversInfoSorted = SortPlayers(playerInfoList,false);
        if(driversInfoSorted.Count >= 2){
            uIControllers.ShowHideRankWindow(true);
            GamePlayerInfo thirdPlace = driversInfoSorted.Count > 2 ? driversInfoSorted[2] : null;
            uIControllers.SetPositions(driversInfoSorted[0],driversInfoSorted[1],thirdPlace);
        }else{
            uIControllers.ShowHideRankWindow(false);
        }
    }

    public void NewMatch_S (){
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewMatch,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }
    public void RefreshTimer_S() {
        object[] package = new object[] { finalTimeToFinishRace };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.RefreshTimer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }
    public void NewMatch_R (){
        // set game state to waiting
        gameState = GameState.WaitingForPlayer;
        myind = -1;
        

        // deactivate map camera
        // mapcam.SetActive(false);

        // hide end game ui
        uIControllers.ShowHideEndGameWindow(false);

        // reset scores
        foreach (GamePlayerInfo p in playerInfoList){
            p.currentLap = 0;
            p.currentCheckPointCount = 0;
            p.reachedFinishedLine = false;
            p.gameFinishedTime = 0f;
        }

        // reset ui
        RefreshMyStats();
        RefreshGameEndTimeUI();

        // reinitialize time
        // InitializeTimer();

        // spawn
        SpawnPlayer();
    }
    
    private void EndGame() {
        // set game state to ending
        gameState = GameState.Ending;
        spawnIndex = 0;
        gamePlayTimeManager.StopTimer();
        if (PhotonNetwork.IsMasterClient) {
            // PhotonNetwork.DestroyAll();
            if (!perpetual) {
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }

        // show end game ui
        Debug.Log("Game Over");
        uIControllers.ShowHideEndGameWindow(true);
        RefreshEndLeaderBoard();
        uIControllers.SetPlayerDetailsAfterGameEnd(GetPlayerRank(PhotonNetwork.LocalPlayer.ActorNumber));
        CheckEndingDataRecived();

    }
    private void CheckEndingDataRecived(){
        int levelGain = Mathf.RoundToInt(100 / GetPlayerRank(PhotonNetwork.LocalPlayer.ActorNumber));
        int coinGain = 100 / GetPlayerRank(PhotonNetwork.LocalPlayer.ActorNumber);
        int cashGain = 50 / GetPlayerRank(PhotonNetwork.LocalPlayer.ActorNumber);
        if(GetPlayerRank(PhotonNetwork.LocalPlayer.ActorNumber) <= 3){
            gameSettings.gameData.IncreaseMatchWon(2);
            if(GetPlayerRank(PhotonNetwork.LocalPlayer.ActorNumber) <= 2){
                gameSettings.gameData.IncreaseMedalsCount(GetTotalPlayerCount() / GetPlayerRank(PhotonNetwork.LocalPlayer.ActorNumber));
            }
        }
        if(GetPlayerRank(PhotonNetwork.LocalPlayer.ActorNumber) == 1){
            gameSettings.gameData.IncreaseGoldTropyCount(1);
        }
        Debug.LogError($"Level Gain As Per Rank : {levelGain}");
        Debug.LogError($"Coin Gain Per Rank : {coinGain}");
        Debug.LogError($"Cash Gain Per Rank : {cashGain}");
        Debug.LogError($"Increase Level Count {gameSettings.gameData.GetTotalMedalsWon()}");
        uIControllers.ChangeLevelSystem(levelGain,cashGain,coinGain);
        gameSettings.Save();
    }
    private IEnumerator End (float p_wait) {
        yield return new WaitForSeconds(p_wait);

        if(perpetual) {
            // new match
            if(PhotonNetwork.IsMasterClient) {
                NewMatch_S();
            }
        } else {
            // disconnect
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
    }
    private void SpawnPlayer(){
        if(PhotonNetwork.IsConnected){
            spawnIndex = PhotonNetwork.CurrentRoom.PlayerCount - 1;
            Transform currentSpawnPoint = GetCurrentTrackManagers().spawnPointsForTrack[spawnIndex];
            Debug.LogError("Player Spawned " + gameSettings.gameProfileData.username);
            Debug.LogError("Spawning Over Network");
            PhotonNetwork.Instantiate(gameSettings.GetCurrentPlayerCar(),currentSpawnPoint.position,currentSpawnPoint.rotation);
            
        }else{
            spawnIndex = PhotonNetwork.CurrentRoom.PlayerCount - 1;
            Transform currentSpawnPoint = GetCurrentTrackManagers().spawnPointsForTrack[spawnIndex];
            Debug.LogError("Player Spawned " + gameSettings.gameProfileData.username);
            Debug.LogError("Spawning Not from Network");
            Instantiate(humanPlayerCar, currentSpawnPoint.position, currentSpawnPoint.rotation);
        }
    }
    private void RefershJoiningWindow(){
        foreach(TextMeshProUGUI lapCountTxt in lapsCountTmProList){
            lapCountTxt.SetText("LAPS : " + GetTotalLapCount());
        }
        foreach(TextMeshProUGUI playerCountTxts in TotalPlayerCountTmproList){
            playerCountTxts.SetText(string.Concat("TOTAL PLAYERS :",GetTotalPlayerCount() ," / ",PhotonNetwork.CurrentRoom.MaxPlayers));
        }
        int totalEntry = Mathf.Min(joinedPlayerCardList.Count,playerInfoList.Count);
        for (int i = 0; i < joinedPlayerCardList.Count; i++) {
            if(i >= totalEntry){
                joinedPlayerCardList[i].HideCard();
            }else{
                joinedPlayerCardList[i].ShowCard();
            }
        }
        for (int i = 0; i < totalEntry; i++) {
            joinedPlayerCardList[i].SetCardsData(i + 1,playerInfoList[i].gameProifile.username);
        }
    }
    public void RefreshEndLeaderBoard(){
        List<GamePlayerInfo> sorted = SortPlayers(finishedCarList,true);
        int totalEntry = Mathf.Min(leaderboardPlayerCardFinalList.Count,sorted.Count);
        for (int i = 0; i < leaderboardPlayerCardFinalList.Count; i++) {
            if(i >= totalEntry){
                leaderboardPlayerCardFinalList[i].HideCard();
            }else{
                leaderboardPlayerCardFinalList[i].ShowCard();
            }
        }
        for (int i = 0; i < totalEntry; i++) {
            leaderboardPlayerCardFinalList[i].SetCardsData(i + 1,sorted[i].gameProifile.username,sorted[i].gameFinishedTime);
        }
    }
    private List<GamePlayerInfo> SortPlayers (List<GamePlayerInfo> p_info,bool raceEndingSorting) {
        List<GamePlayerInfo> sorted = p_info;        
        // Check for Highest CheckPointCount;
        for (int i = 0; i < sorted.Count; i++) {
            for (int j = i + 1; j < sorted.Count; j++){
                if(sorted[j].currentCheckPointCount > sorted[i].currentCheckPointCount){
                    GamePlayerInfo temp = sorted[i];
                    sorted[i] = sorted[j];
                    sorted[j] = temp;
                }
            }
        }
        // Check for Highest Laps;
        for (int i = 0; i < sorted.Count; i++) {
            for (int j = i + 1; j < sorted.Count; j++){
                if(sorted[j].currentLap > sorted[i].currentLap){
                    GamePlayerInfo temp = sorted[i];
                    sorted[i] = sorted[j];
                    sorted[j] = temp;
                }
            }
        }
        if(raceEndingSorting){
            // Check for Shortest Time After Finisheing Race.
            for (int i = 0; i < sorted.Count; i++) {
                for (int j = i + 1; j < sorted.Count; j++){
                    if(sorted[j].gameFinishedTime < sorted[i].gameFinishedTime){
                        GamePlayerInfo temp = sorted[i];
                        sorted[i] = sorted[j];
                        sorted[j] = temp;
                    }
                }
            }
        }
        return sorted;
    }
    public GameSettingsSO GetGameSettings(){
        return gameSettings;
    }
    public string GetPlayerName(int id){
        string carName = gameSettings.gameProfileData.username;
        if(playerInfoList.Count > 0){
            carName = playerInfoList[0].gameProifile.username;
            for (int i = 0; i < playerInfoList.Count; i++) {
                if(playerInfoList[i].actorId == id){
                    carName = playerInfoList[i].gameProifile.username;
                }
            }
        }
        return carName;
    }
    public int GetPlayerRank(int actorId){
        List<GamePlayerInfo> driversInfoSorted = SortPlayers(playerInfoList,false);
        int rank = 0;
        for (int i = 0; i < driversInfoSorted.Count; i++) {
            if(driversInfoSorted[i].actorId == actorId){
                int index = driversInfoSorted.IndexOf(driversInfoSorted[i]);
                rank = index + 1;
            }
        }
        return rank;
    }

    public int GetTotalPlayerCount(){
        return playerInfoList.Count;
    }
}