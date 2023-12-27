using TMPro;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using GamerWolf.Utils;
using Photon.Realtime;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using ExitGames.Client.Photon;
public class MultiplayerMatchMakingManager : MonoBehaviourPunCallbacks,IConnectionCallbacks {
    
    /* #region Constant................
    private const int TRACK_SCENE_INDEX = 2;
    private const int MAX_PLAYERS = 6;
    private const int MAX_LAPS = 7;
    public static string TRACK_KEY = "TRACKS";
    public static string LAP_KEY = "LAP";
    public static string WAITING_FOR_PLAYER_TIME_KEY = "WaitngForPlayerTime";

    #endregion */
    [SerializeField] private TrackSO[] trackArray;
    [SerializeField] private TrackSelectionUiBtn[] trackSelectionUiBtns;
    [SerializeField] private GameSettingsSO gameSettingsSo;
    [SerializeField] private Slider maxPlayersSlider,lapCountSlider;
    [SerializeField] private TextMeshProUGUI maxPlayersValue,lapCountSliderValue;
    [SerializeField] private GameObject connectingWindow;
    [SerializeField] private OpponnentSerachingWindow competitorsSearchingWindow;
    [SerializeField] private UIButtonCustom quickStartBtn;
    [SerializeField] private TMP_InputField roomNameField;
    [SerializeField] private Toggle hasBotsToggle;
    [SerializeField] private List<RoomInfo> currentRoomList;
    private bool randomRoom;
    private void Awake(){
        gameSettingsSo.gameData.SetRegion(PhotonNetwork.CloudRegion);
        hasBotsToggle.isOn = gameSettingsSo.hasBots;
        lapCountSlider.value = gameSettingsSo.currentLap;
        lapCountSliderValue.SetText(string.Concat("LAPS : ",gameSettingsSo.currentLap));
        maxPlayersSlider.value = gameSettingsSo.maxPlayer;
        maxPlayersValue.SetText(string.Concat("MAX PLAYER : ",gameSettingsSo.maxPlayer));
        quickStartBtn.interactable = true;
        connectingWindow.SetActive(false);
    }

    public override void OnRegionListReceived(RegionHandler regionHandler) {
        Debug.Log("Regiion" + regionHandler.BestRegion);
        List<Region> regionList = regionHandler.EnabledRegions;
        Debug.Log("Regions List " + regionList);
        base.OnRegionListReceived(regionHandler);
    }
    private void Start(){
        currentRoomList = new List<RoomInfo>();
        // friendRoomList = new List<FriendInfo>();
        RefreshTrackTypesUi();
        if(!PhotonNetwork.IsConnected){
            Debug.Log("Starting To Connect");
            Connect();
        }
        competitorsSearchingWindow.onSerachComplete += FindOpenRoomMatches;
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
                }
            }
        }
    }
    private void Connect(){
        connectingWindow.SetActive(true);
        quickStartBtn.interactable = false;
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Trying To Connect to Master");
        // PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.Reconnect();
    }
    public override void OnConnectedToMaster() {
        quickStartBtn.interactable  = true;
        connectingWindow.SetActive(false);
        Debug.LogError("Connected To Master");
        base.OnConnectedToMaster();
    }
    public void UpdateLapCountSlider(float value){
        gameSettingsSo.currentLap = Mathf.RoundToInt(value);
        lapCountSliderValue.SetText(string.Concat("LAPS : ",value));
    }
    public void UpdateHasBots(bool value){
        gameSettingsSo.hasBots = value;
    }
    public void MakePersonalRoom() {
        AudioManager.Current.StopAudio(Sounds.SoundType.TrackDay_Loop_Standerd_1);
        connectingWindow.SetActive(true);
        quickStartBtn.interactable = false;
        if(PhotonNetwork.IsConnectedAndReady){
            RoomOptions roomOptions = new RoomOptions
            {
                IsVisible = true,
                IsOpen = true,
                CustomRoomPropertiesForLobby = new string[] { KeysHolder.TRACK_KEY, KeysHolder.LAP_KEY },
            };
            float roomPlayer = maxPlayersSlider.value;
            roomOptions.MaxPlayers = (byte)roomPlayer;
            gameSettingsSo.currentLap = Mathf.RoundToInt(lapCountSlider.value);
            Debug.LogError($"Tying To Find a Room with Laps Count of {gameSettingsSo.currentLap} at {(TrackTypes)gameSettingsSo.currentTrack.trackSaveData.tracks} Track , With Player Count of {roomPlayer}");
            Hashtable properties = new Hashtable
            {
                { KeysHolder.TRACK_KEY, gameSettingsSo.currentTrack.trackSaveData.tracks },
                { KeysHolder.LAP_KEY, gameSettingsSo.currentLap },
                { KeysHolder.WAITING_FOR_PLAYER_TIME_KEY, gameSettingsSo.maxWaitingTime },
                
            };
            roomOptions.CustomRoomProperties = properties;
            // PhotonNetwork.JoinRandomOrCreateRoom(properties,(byte)roomPlayer,MatchmakingMode.FillRoom,TypedLobby.Default,null,null,roomOptions);
            PhotonNetwork.JoinRandomRoom(properties,(byte)roomPlayer,MatchmakingMode.RandomMatching,TypedLobby.Default,null,null);
            // CreateNewCustomRoom();
        }else{
            PhotonNetwork.AutomaticallySyncScene = true;
            Debug.Log("Trying To Connect to Master");
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public void OpenOppnonentFindWindow(){
        competitorsSearchingWindow.StartFinding();
    }
    private void FindOpenRoomMatches(){
        AudioManager.Current.StopAudio(Sounds.SoundType.TrackDay_Loop_Standerd_1);
        randomRoom = true;
        Debug.LogError("Finding Random Game Rooms");
        if(currentRoomList.Count > 0 && currentRoomList != null){
            for (int i = 0; i < currentRoomList.Count; i++) {
                RoomInfo roomInfo = currentRoomList[i];
                if(roomInfo.PlayerCount < roomInfo.MaxPlayers){
                    PhotonNetwork.JoinRoom(roomInfo.Name);
                    break;
                }
            }
        }else{
            QuickRandomGameStart();
        }
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        // Debug.LogError("Current Avaialble Room Amount is " + roomList.Count);
        currentRoomList = roomList;
    }
    /* public override void OnFriendListUpdate(List<FriendInfo> friendList) {
        // Debug.LogError("Current Avaialble Friend Room Amount is " + friendList.Count);
        friendRoomList = friendList;
    } */
    public override void OnJoinedRoom() {
        PhotonNetwork.ReconnectAndRejoin();
        VerifyUsername();
        connectingWindow.SetActive(true);
        Debug.LogError(PhotonNetwork.LocalPlayer.NickName + " Joinning A room in " + (TrackTypes)PhotonNetwork.CurrentRoom.CustomProperties[KeysHolder.TRACK_KEY] + " Track With Laps of" + (int)PhotonNetwork.CurrentRoom.CustomProperties[KeysHolder.LAP_KEY] + " Player Count of " + PhotonNetwork.CurrentRoom.PlayerCount);
        StartGame();
        base.OnJoinedRoom();
    }

    public override void OnDisconnected(DisconnectCause cause) {
        connectingWindow.SetActive(true);
        quickStartBtn.interactable = false;
        // Connect();
        Debug.LogError("Disconnected Due to " + cause);
        if (this.CanRecoverFromDisconnect(cause)) {
            this.Recover();
        }
        base.OnDisconnected(cause);
    }
    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.LogError("Failed To Connect Random Room");
        if(randomRoom){
            CreateNewRandomCustomRoom();
        }else{
            CreateNewCustomRoom();
        }
        base.OnJoinRandomFailed(returnCode, message);
    }
    public override void OnJoinRoomFailed(short returnCode, string message) {
        Debug.LogError("Failed To Connect Random Room");
        base.OnJoinRoomFailed(returnCode, message);
    }

    #region Expersiance Gain Match.............................
    private void QuickRandomGameStart(){
        connectingWindow.SetActive(true);
        if(PhotonNetwork.IsConnectedAndReady){
            /* RoomOptions roomOptions = new RoomOptions {
                IsVisible = true,
                IsOpen = true,
                CustomRoomPropertiesForLobby = new string[] { TRACK_KEY, LAP_KEY }
            }; */
            int currentLapCount = Random.Range(1,KeysHolder.MAX_LAPS);
            TrackTypes randomTrack = GetTrackTypes();
            Debug.LogError($"Tying To Find Random Room with {randomTrack} Track = Player Count of {KeysHolder.MAX_PLAYERS} = Total Laps Count {currentLapCount}");
            Hashtable roomProperties = new Hashtable {
                { KeysHolder.TRACK_KEY, randomTrack },
                { KeysHolder.LAP_KEY, currentLapCount },
                { KeysHolder.WAITING_FOR_PLAYER_TIME_KEY, gameSettingsSo.maxWaitingTime }
            };
            // roomOptions.CustomRoomProperties = roomProperties;
            string randomRoomName = string.Concat("MRoom",Random.Range(10,2000));
            Debug.LogError("room Name " + randomRoomName);
            // PhotonNetwork.JoinRandomOrCreateRoom(roomProperties,(byte)MAX_PLAYERS,MatchmakingMode.FillRoom,TypedLobby.Default,null,randomRoomName,roomOptions);
            PhotonNetwork.JoinRandomRoom(roomProperties,(byte)KeysHolder.MAX_PLAYERS,MatchmakingMode.RandomMatching,TypedLobby.Default,null,null);
        }else{
            PhotonNetwork.AutomaticallySyncScene = true;
            Debug.Log("Disconnected : Trying To Reconnect to Master");
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    private void CreateNewRandomCustomRoom(){
        RoomOptions roomOptions = new RoomOptions {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = (byte)KeysHolder.MAX_PLAYERS,
            CustomRoomPropertiesForLobby = new string[] { KeysHolder.TRACK_KEY, KeysHolder.LAP_KEY }
        };
        int lapCountsTotal = Random.Range(1,KeysHolder.MAX_LAPS);
        TrackTypes randomTrack = GetTrackTypes();
        Debug.LogError($"Creating a Room with Laps Count of {lapCountsTotal} at {randomTrack} Track , With Player Count of {KeysHolder.MAX_PLAYERS}");
        Hashtable roomProperties = new Hashtable {
            { KeysHolder.TRACK_KEY, randomTrack },
            { KeysHolder.LAP_KEY, lapCountsTotal },
            { KeysHolder.WAITING_FOR_PLAYER_TIME_KEY, gameSettingsSo.maxWaitingTime }
        };
        roomOptions.CustomRoomProperties = roomProperties;
        string randomRoomName = string.Concat("MRoom",Random.Range(10,2000));
        Debug.LogError("room Name " + randomRoomName);
        PhotonNetwork.CreateRoom(randomRoomName,roomOptions);
    }
    private TrackTypes GetTrackTypes(){
        List<TrackTypes> trackTypesList = new List<TrackTypes>();
        for (int i = 0; i < trackArray.Length; i++) {
            if(!trackArray[i].trackSaveData.isLocked){
                if(!trackTypesList.Contains(trackArray[i].trackSaveData.tracks)){
                    trackTypesList.Add(trackArray[i].trackSaveData.tracks);
                }
            }
        }
        int randomTrackIndex = Random.Range(0,trackTypesList.Count);
        return trackTypesList[randomTrackIndex];
    }

#endregion


    private void CreateNewCustomRoom(){
        float roomPlayer = maxPlayersSlider.value;
        RoomOptions roomOptions = new RoomOptions {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = (byte)roomPlayer,
            CustomRoomPropertiesForLobby = new string[] { KeysHolder.TRACK_KEY, KeysHolder.LAP_KEY }
        };
        Debug.LogError($"Creating a Room with Laps Count of {gameSettingsSo.currentLap} at {(TrackTypes)gameSettingsSo.currentTrack.trackSaveData.tracks} Track , With Player Count of {roomPlayer}");
        gameSettingsSo.currentLap = Mathf.RoundToInt(lapCountSlider.value);
        Hashtable properties = new Hashtable {
            { KeysHolder.TRACK_KEY, gameSettingsSo.currentTrack.trackSaveData.tracks },
            { KeysHolder.LAP_KEY, gameSettingsSo.currentLap },
            { KeysHolder.WAITING_FOR_PLAYER_TIME_KEY, gameSettingsSo.maxWaitingTime }
        };
        roomOptions.CustomRoomProperties = properties;
        PhotonNetwork.CreateRoom(roomNameField.text,roomOptions,TypedLobby.Default,null);
    }
    public void UpdateMaxPlayerSliderView(float value){
        gameSettingsSo.maxPlayer = value;
        maxPlayersValue.SetText(string.Concat("MAX PLAYER : ",value));
    }
    private void VerifyUsername () {
        SavingAndLoadingManager.Current.SaveGame();
        PhotonNetwork.LocalPlayer.NickName = gameSettingsSo.gameProfileData.username;
    }
    public void StartGame () {
        connectingWindow.SetActive(true);
        VerifyUsername();
        if(PhotonNetwork.CurrentRoom.PlayerCount == 1) {
            PhotonNetwork.IsMessageQueueRunning = false;
            PhotonNetwork.LoadLevel(KeysHolder.TRACK_SCENE_INDEX);
        }
    }

    public void SetCurrentTrack(TrackSO tracks) {
        gameSettingsSo.currentTrack = tracks;
        RefreshTrackTypesUi();
    }
    private void RefreshTrackTypesUi(){
        for (int i = 0; i < trackSelectionUiBtns.Length; i++) {
            if(gameSettingsSo.currentTrack.trackSaveData.tracks != trackSelectionUiBtns[i].GetTracks()){
                trackSelectionUiBtns[i].ShowHideTrackBtn(true);
            }else{
                trackSelectionUiBtns[i].ShowHideTrackBtn(false);
            }
        }
    }
    
}