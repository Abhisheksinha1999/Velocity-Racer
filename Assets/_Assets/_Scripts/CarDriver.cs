using TMPro;
using System;
using ArcadeVP;
using Photon.Pun;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using GamerWolf.Utils;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
public class CarDriver : MonoBehaviourPunCallbacks,IPunObservable {
    [Header("Controlls")]
    [SerializeField] private GameObject leftRightStickControlls;
    [SerializeField] private GameObject steeringControlls,GyroControlls;
    [SerializeField] private GameObject firstRanksVisual;
    [SerializeField] private Volume nosVolume;
    [SerializeField] private SpriteRenderer miniMapTrackRender;
    [SerializeField] private CinemachineScreenShakeManager screenShakeManager;
    [SerializeField] private GameObject miniMapVisual;
    [SerializeField] private float fieldOfViewChangeSpeed = .2f;
    [SerializeField] private float normalFov,nosFov;
    [SerializeField] private GameSettingsSO gameSettings;
    [SerializeField] private GameDataSO gameData;
    [SerializeField] private GameProifileData profileData;
    [SerializeField] private LocalPlayerUiManager localPlayerUiManager;
    [SerializeField] private bool autoDrift;
    [SerializeField] private Image playerIndicatorImage;
    [SerializeField] private TextMeshProUGUI playerNickName,rankingVisualText;
    [SerializeField] private TextMeshPro playerNameShowingOnServer;
    [SerializeField] private GameObject mainCam;
    [SerializeField] private CinemachineVirtualCamera carCam,endCamera;
    [SerializeField] private ArcadeVehicleController arcadeVehicleController;
    [SerializeField] private int totalLaps;
    [SerializeField] private float nosUseSpeed = 2f;
    [SerializeField] private float maxNosTime = 20f;
    [SerializeField] private ParticleSystem nosEffect,smokeEffect;
    [SerializeField] private CarBodyFullWorld carBodyFullWorld;
    private bool isReady;
    private bool raceCompleted;
    private Vector2 input;
    private float brake;
    private int currentLap;
    private Color playerNamePlateColor;
    private bool holdingNos;
    private bool resetLapsTime;
    private float lapTimer = 0;
    private int checkPointCrossCount;
    private int totalCheckPointPerLap;
    private float currentNosTime;
    private bool nos;
    private Vector3 startPoint;
    private Vector3 lastCheckPointPos;
    private float currentLenseFov;
    private float velocity;
    private bool isReversing;
    private float currentForwardAmount,currentSideDriveAmount,currentBrakeAmount;
    private bool isDrifing;
    private Rigidbody rb;
    private Quaternion startRotation;
    private Transform nextCheckPoint;
    private float moveAmount;
    private void Awake(){
        rb = GetComponent<Rigidbody>();
    }
    public int GetCarTypeIndex(){
        return (int)carBodyFullWorld.GetCarTypeSO().carSaveData.carTypeShop;
    }
    public void SetInitPoint(Vector3 startPoint){
        this.startPoint = new Vector3(startPoint.x,rb.position.y,startPoint.z);
        startRotation = rb.rotation;
    }
    private void Start(){
        firstRanksVisual.SetActive(false);
        currentLenseFov = normalFov;
        isReady = true;
        localPlayerUiManager.HideWrongCheckPointIndicator();
        Debug.LogError("Player Actor Number " + PhotonNetwork.LocalPlayer.ActorNumber);
        nosEffect.gameObject.SetActive(false);
        SetTotalCheckPointPerLapCounts(MatchHandler.Current.GetTotalCheckPointCount());
        endCamera.gameObject.SetActive(false);
        miniMapTrackRender.gameObject.SetActive(true);
        nosVolume.weight = 0f;
        if(!photonView.IsMine){
            miniMapVisual.SetActive(false);
            miniMapTrackRender.color = Color.red;
            carCam.transform.SetParent(transform);
            localPlayerUiManager.gameObject.SetActive(false);
            carCam.gameObject.SetActive(false);
            mainCam.SetActive(false);
            photonView.RPC(nameof(ShowPlayerNameToNonLocalPlayer),RpcTarget.All);
        }else{
            MatchHandler.OnRaceStart += (object sender,EventArgs e)=>{
                isReversing = false;
                moveAmount = 1f;
            };
            miniMapVisual.SetActive(true);
            miniMapTrackRender.color = Color.yellow;
            currentNosTime = maxNosTime / 2f;
            mainCam.SetActive(true);
            localPlayerUiManager.gameObject.SetActive(true);
            carCam.gameObject.SetActive(true);
            photonView.RPC(nameof(SetTotalLapsRpc),RpcTarget.All,MatchHandler.Current.GetTotalLapCount());
            checkPointCrossCount = 0;
            SetInitPoint(transform.position);
            playerNameShowingOnServer.gameObject.SetActive(false);
        }
        
    }
    private void OnApplicationQuit(){
        if(photonView.IsMine){
            carCam.transform.SetParent(transform);
        }
    }
    private void OnDestroy(){
        if(photonView.IsMine){
            carCam.transform.SetParent(transform);
        }
    }
    private int currentRank;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if(stream.IsWriting){
            stream.SendNext(currentRank);
        }else{
            currentRank = (int)stream.ReceiveNext();
            if(!photonView.IsMine){
                if(MatchHandler.Current.GetGameState() == GameState.Playing){
                    // photonView.RPC(nameof(ShowPlayerNameToNonLocalPlayer),RpcTarget.AllBuffered);
                    if(currentRank < 2){
                        // photonView.RPC(nameof(ShowHideFirstRankVisual),RpcTarget.AllBuffered,true);
                        ShowHideFirstRankVisual(true);
                    }else{
                        // photonView.RPC(nameof(ShowHideFirstRankVisual),RpcTarget.AllBuffered,false);
                        ShowHideFirstRankVisual(false);
                    }
                }
            }
        }
    }
    
    private void Update(){
        if(photonView.IsMine){
            switch(MatchHandler.Current.GetGameState()){
                case GameState.Starting:
                    photonView.RPC(nameof(SetNickNameRpc),RpcTarget.AllBuffered,MatchHandler.Current.GetPlayerName(PhotonNetwork.LocalPlayer.ActorNumber));
                    mainCam.SetActive(true);
                    carCam.gameObject.SetActive(true);
                    localPlayerUiManager.gameObject.SetActive(false);
                    moveAmount = 0f;
                    currentForwardAmount = 0f;
                    carCam.transform.SetParent(this.transform);
                    rb.position = new Vector3(startPoint.x,rb.position.y,startPoint.z);
                    rb.rotation = startRotation;
                    smokeEffect.gameObject.SetActive(true);
                    nos = false;
                    input = Vector2.zero;
                    currentBrakeAmount = 1;
                    SetDrivingValues();
                    return;
                case GameState.WaitingForPlayer:
                    localPlayerUiManager.gameObject.SetActive(false);
                    currentForwardAmount = 0f;
                    moveAmount = 0;
                break;
                case GameState.Playing:
                    if(raceCompleted){
                        MatchHandler.Current.OnLocalPlayerFinishRace(photonView.OwnerActorNr);
                        carCam.transform.SetParent(this.transform);
                        localPlayerUiManager.ShowTrackMiniMap(false);
                        endCamera.gameObject.SetActive(true);
                        carCam.gameObject.SetActive(false);
                        smokeEffect.gameObject.SetActive(true);
                        nos = false;
                        currentLenseFov = normalFov;
                        input = Vector2.zero;
                        brake = 1;
                        SetDrivingValues();
                        localPlayerUiManager.gameObject.SetActive(false);
                        return;
                    }
                    localPlayerUiManager.gameObject.SetActive(true);
                    CheckControllsType();
                    switch(gameData.GetControllType()){
                        case ControllsSelectionManager.ControllType.Left_Right_Brake:
                        case ControllsSelectionManager.ControllType.SteeringWheel_Controlls:
                        case ControllsSelectionManager.ControllType.Accelrometer_Controlls:
                            moveAmount = isReversing ? -1f : 1f;
                        break;
                    }
                    if(moveAmount < 0){
                        currentForwardAmount = -1f;
                    }else{
                        currentForwardAmount = 1f;
                    }
                    carCam.transform.SetParent(null);
                    localPlayerUiManager.ShowTrackMiniMap(true);
                    smokeEffect.gameObject.SetActive(false);
                    if(nos){
                        nosVolume.weight = 1 - currentNosTime / maxNosTime;
                    }else{
                        nosVolume.weight = 0f;
                    }
                    if(!raceCompleted){
                        lapTimer += Time.deltaTime;
                    }
                    if(nos){
                        screenShakeManager.Shake();
                        currentLenseFov = nosFov;
                    }else{
                        currentLenseFov = normalFov;
                    }
                    if(gameData.GetOnPc()){
                        isReversing = Input.GetAxisRaw("Vertical") < 0 ? true: false;
                        currentSideDriveAmount = Input.GetAxisRaw("Horizontal");
                        float threshHoldforDrift = 12f;
                        currentBrakeAmount = currentForwardAmount < 0 && velocity >= threshHoldforDrift && arcadeVehicleController.GetRigidBodyVelocity().z > 0f ? 1f: 0f;
                        isDrifing = currentBrakeAmount == 1f ? true: false;
                        nos = Input.GetKey(KeyCode.LeftShift) && currentNosTime > 0f && currentForwardAmount>= 0.1f;
                        arcadeVehicleController.SetNOs(nos);
                    }
                    SetDrivingValues();
                break;
                case GameState.Ending:
                    localPlayerUiManager.gameObject.SetActive(false);
                    currentForwardAmount = 0f;
                    moveAmount = 0;
                break;
            }            
        }else{
            /* if(MatchHandler.Current.GetGameState() == GameState.Playing){
                if(MatchHandler.Current.GetPlayerRank(PhotonNetwork.LocalPlayer.ActorNumber) <= 1){
                    photonView.RPC(nameof(ShowHideFirstRankVisual),RpcTarget.AllBuffered,true);
                }else{
                    photonView.RPC(nameof(ShowHideFirstRankVisual),RpcTarget.AllBuffered,false);
                }
            } */
            photonView.RPC(nameof(ShowPlayerNameToNonLocalPlayer),RpcTarget.AllBuffered);
            currentRank = MatchHandler.Current.GetPlayerRank(photonView.OwnerActorNr);
        }
    }
    private void CheckControllsType(){
        leftRightStickControlls.SetActive(false);
        steeringControlls.SetActive(false);
        GyroControlls.SetActive(false);
        switch(gameData.GetControllType()){
            case ControllsSelectionManager.ControllType.Left_Right_Brake:
                leftRightStickControlls.SetActive(true);
            break;
            case ControllsSelectionManager.ControllType.SteeringWheel_Controlls:
                steeringControlls.SetActive(true);
            break;
            case ControllsSelectionManager.ControllType.Accelrometer_Controlls:
                GyroControlls.SetActive(true);
            break;
        }
    }

    /* [PunRPC] */
    private void ShowHideFirstRankVisual(bool show){
        firstRanksVisual.SetActive(show);
    }
    [PunRPC]
    private void ShowPlayerNameToNonLocalPlayer(){
        playerNameShowingOnServer.SetText(GetUserName());
    }
    private void LateUpdate(){
        if(photonView.IsMine){
            if(carCam.m_Lens.FieldOfView != currentLenseFov){
                carCam.m_Lens.FieldOfView = Mathf.Lerp(carCam.m_Lens.FieldOfView,currentLenseFov,fieldOfViewChangeSpeed * Time.deltaTime);
            }
            float smootingSpeed = 5f;
            if(input.x != currentSideDriveAmount){
                input.x = Mathf.Lerp(input.x,currentSideDriveAmount,smootingSpeed * Time.deltaTime);
            }
            if(input.y != currentForwardAmount){
                input.y = Mathf.Lerp(input.y,currentForwardAmount,smootingSpeed * Time.deltaTime);
            }
            if(brake != currentBrakeAmount){
                brake = Mathf.Lerp(brake,currentBrakeAmount,smootingSpeed * Time.deltaTime);
            }
        }
    }
    public void IncreaseNosTime(float incrementAmount){
        if(photonView.IsMine){
            photonView.RPC(nameof(IncrementNOsTimeRpc),RpcTarget.AllBuffered,incrementAmount);
        }
    }
    
    private void SetDrivingValues(){
        nosEffect.gameObject.SetActive(nos);
        if(nos){
            currentNosTime -= Time.deltaTime * nosUseSpeed;
        }
        localPlayerUiManager.SetTime(lapTimer);
        if(arcadeVehicleController.GetVelocity(gameData.GetSpeedType()) <= 0f){
            velocity = 0f;
        }else{
            velocity = arcadeVehicleController.GetVelocity(gameData.GetSpeedType());
        }
        localPlayerUiManager.SetCurrentSpeed(velocity);
        arcadeVehicleController.SetInput(input,brake);
        localPlayerUiManager.SetNos(currentNosTime / maxNosTime);
        localPlayerUiManager.SetPositions(MatchHandler.Current.GetPlayerRank(PhotonNetwork.LocalPlayer.ActorNumber),MatchHandler.Current.GetTotalPlayerCount());
    }
    public void SetUIInput(bool reverse,float sideWays,bool nosPressed) {
        if(gameData.GetOnPc()) return;
        this.isReversing = reverse;
        this.currentSideDriveAmount = sideWays;
        float threshHoldforDrift = 12f;
        this.currentBrakeAmount = currentForwardAmount < 0 && velocity >= threshHoldforDrift && arcadeVehicleController.GetRigidBodyVelocity().z > 0f ? 1f: 0f;
        this.nos = nosPressed && currentNosTime > 0f;
        if(this.nos){
            if(currentForwardAmount <= 0f){
                currentForwardAmount = 1f;
            }
        }
    }
    public void SetSteeringInput(float steering){
        if(gameData.GetOnPc()) return;
        if(gameData.GetControllType() == ControllsSelectionManager.ControllType.SteeringWheel_Controlls){
            this.currentSideDriveAmount = steering;
        }
    }

    public bool IsReady() {
        return isReady;
    }
    public string GetUserName(){
        return MatchHandler.Current.GetPlayerName(photonView.OwnerActorNr);
    }

    public void TrySync(){
        if(photonView.IsMine){
            photonView.RPC(nameof(SetRandomColorRPC),RpcTarget.All);
            photonView.RPC(nameof(SyncRpc),RpcTarget.All,gameSettings.gameProfileData.username);
        }
    }
    public void OnCorrectCheckPointCrossed(Vector3 lastCheckPoinPos,Transform nextCheckPoint){
        if(photonView.IsMine){
            this.lastCheckPointPos = lastCheckPoinPos;
            this.nextCheckPoint = nextCheckPoint;
            // localPlayerUiManager.ShowCheckPointIndicator();
            photonView.RPC(nameof(IncreaseCheckPointCountRpc),RpcTarget.AllBuffered);
        }
    }
    public void MoveToLastCheckPoint(){
        if(photonView.IsMine){
            photonView.RPC(nameof(MoveToLastCheckPointRPC),RpcTarget.AllBuffered);
        }
    }
    public void OnWrongCheckPointCrossed(){
        if(photonView.IsMine){
            localPlayerUiManager.ShowWrongCheckPointIndicator();
        }
    }
    public void IncreaseLapsCount(){
        if(photonView.IsMine){
            photonView.RPC(nameof(IncreaseLapsRpc),RpcTarget.All);
        }
    }
    public void SetTotalCheckPointPerLapCounts(int totalChecksPointPerLap){
        this.totalCheckPointPerLap = totalChecksPointPerLap;
    }
    public int GetCurrentLapsCount(){
        return currentLap;
    }

    #region  RPC Functions..............
    [PunRPC]
    private void IncrementNOsTimeRpc(float incrementAmount){
        currentNosTime += incrementAmount;
        if(currentNosTime >= maxNosTime){
            currentNosTime = maxNosTime;
        }
    }
    [PunRPC]
    private void IncreaseCheckPointCountRpc(){
        checkPointCrossCount = (checkPointCrossCount + 1) % totalCheckPointPerLap;
        if(checkPointCrossCount == 0){
            localPlayerUiManager.ShowLapCompletedIndicator();
            if((totalCheckPointPerLap - currentLap) == 1){
                localPlayerUiManager.ShowFinalLapIndicator();
            }
        }
        int temp = raceCompleted ? 1 : 0;// 1 is true and 0 is false.........
        MatchHandler.Current.ChangeStat_S(photonView.OwnerActorNr,0,(byte) currentLap,checkPointCrossCount,(byte) temp,(byte)0,lapTimer/* ,GetCarTypeIndex() */);// 1 for true.....
        // Debug.LogError("Check Point Count Crossed By : " + GetUserName() + " is " + checkPointCrossCount);
    }
    [PunRPC]
    private void SetRandomColorRPC(){
        playerNamePlateColor = Random.ColorHSV();
    }

    [PunRPC]
	private void IncreaseLapsRpc(){
        currentLap ++;
        // Debug.Log("lap increased in " + GetUserName());
        if(currentLap >= totalLaps){
            currentLap = totalLaps;
            isReady = false;
            raceCompleted = true;
        }else{
            raceCompleted = false;
            resetLapsTime = true;
            // MatchHandler.Current.ChangeStat_S(photonView.OwnerActorNr,0,(byte) currentLap,(byte)checkPointCrossCount,1);// 1 for true.....
        }
        // MatchHandler.Current.ChangeStat_S(photonView.OwnerActorNr,0,(byte) currentLap,(byte)checkPointCrossCount,0);// 0 for False.....
        int temp = raceCompleted ? 1 : 0;// 1 is true and 0 is false.........
        MatchHandler.Current.ChangeStat_S(photonView.OwnerActorNr,0,(byte) currentLap,checkPointCrossCount,(byte) temp,(byte)0,lapTimer/* ,GetCarTypeIndex() */);// 1 for true.....
        localPlayerUiManager.SetLapCounts(currentLap,totalLaps);
    }
    [PunRPC]
    private void SyncRpc(string userNameNew){
        Debug.LogError("Sync RPC " + GetUserName());
        playerIndicatorImage.color = playerNamePlateColor;
        profileData = new GameProifileData(userNameNew,GetCarTypeIndex());
    }
    [PunRPC]
    private void SetTotalLapsRpc(int maxLaps){
        raceCompleted = false;
        currentLap = 0;
        totalLaps = maxLaps;
        localPlayerUiManager.SetLapCounts(currentLap,totalLaps);
    }
    [PunRPC]
    private void SetNickNameRpc(string userName){
        playerNickName.SetText(userName.ToUpper());
        rankingVisualText.SetText(userName.ToUpper());
    }
    [PunRPC]
    private void MoveToLastCheckPointRPC(){
        transform.position = new Vector3(lastCheckPointPos.x,transform.position.y,lastCheckPointPos.z);
    }

    
    #endregion

}