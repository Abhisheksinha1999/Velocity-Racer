using TMPro;
using Dan.Main;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class InitializingManager : MonoBehaviourPunCallbacks {
    private const string leadBoardPublicKey = "26ed28ac70989fe9ac4cd9541af54698778bd2f8c445c4d88168b6e60eba4d9c";
    [SerializeField] private TextMeshProUGUI versionNum,connectiongTxt;
    [SerializeField] private GameSettingsSO gameSettings;
    [SerializeField] private GameDataSO gameData;
    [SerializeField] private GameObject setUsernameWindow;
    [SerializeField] private Button saveNewNameButton;
    [SerializeField] private TMP_InputField newUserNameTextField;
    [SerializeField] private TextMeshProUGUI warningMessageTxt;
    [SerializeField] private List<string> badWordsList;

    [SerializeField] private Image loadingBar;
    private void Start(){
        setUsernameWindow.SetActive(false);
        HideWarningText();
        loadingBar.fillAmount = 0f;
        saveNewNameButton.interactable = false;
        versionNum.SetText("v"+Application.version);
        connectiongTxt.SetText("Connnecting....");
        if(!PhotonNetwork.IsConnected){
            connectiongTxt.SetText("Connnecting....");
            Debug.Log("Starting To Connect");
            Connect();
        }
    }
    private void Connect(){
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Trying To Connect to Master");
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster() {
        
        connectiongTxt.SetText("Connnected....");
        if(gameData.IsFirstRun()){
            setUsernameWindow.SetActive(true);
        }else{
            // Load to the Menu Scene.
            LoadToMenu();
        }
        Debug.LogError("Connected To Master");
        base.OnConnectedToMaster();
    }
    public override void OnDisconnected(DisconnectCause cause) {
        connectiongTxt.SetText("Reconnecting..");
        base.OnDisconnected(cause);
        Connect();
    }
    private IEnumerator GetLoadSceneProgress(int SceneIndex){
        float totalProgress = 0f;
        loadingBar.fillAmount = 0f;
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneIndex);
        while(!operation.isDone){
            totalProgress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.fillAmount = totalProgress;
            yield return null;
            
        }
    }

    public void ChangeName() {
        // Call from Save Name Button.
        if(!string.IsNullOrEmpty(newUserNameTextField.text)){
            if(newUserNameTextField.text.Length > 6 && newUserNameTextField.text.Length < 12){
                HideWarningText();
                gameSettings.gameProfileData.username = newUserNameTextField.text;
                string uName = gameSettings.gameProfileData.username;
                char[] first3Letters = new char[3]{uName[0],uName[1],uName[2]};
                gameSettings.gameData.SetUntqueIdNumberRandom(first3Letters);
                saveNewNameButton.interactable = false;
                gameSettings.Save();
                int scoreAveraging = Mathf.RoundToInt((gameData.GetTotalMatchWon() + gameData.GetTotalMedalsWon() + gameData.GetTotalRacesJoined() + gameData.GetTotalGoldTropyWon()) / 4);
                LeaderboardCreator.UploadNewEntry(leadBoardPublicKey,gameSettings.gameProfileData.username,scoreAveraging,gameSettings.gameData.GetUniqueIdNumber(),((receivingData)=>{ 
                    LoadToMenu();
                    newUserNameTextField.text = string.Empty;
                }));
            }else{
                ShowWarningText("User name Must be atleast 6 Character Long");
                saveNewNameButton.interactable = true;
            }
        }else{
            ShowWarningText("User Name Cannot be Empty");
            saveNewNameButton.interactable = true;
        }
    }
    public void CheckUniequeName(string newName){
        if(string.IsNullOrEmpty(newName)){
            ShowWarningText($"UserName Cannot Be Nothing");
            saveNewNameButton.interactable = false;
            return;
        }
        if(IsBadWords(newName.ToLower())){
            ShowWarningText($"<color=yellow>[WARNNING]</color> : You Cannot Use <color=cyan>[{newName}]</color> as UserName ");
            saveNewNameButton.interactable = false;
            return;
        }
        if(newName.Length < 6){
            saveNewNameButton.interactable = false;
            ShowWarningText("User name Must be atleast 6 Character Long");
            return;
        }
        if(newName.Length > 12){
            saveNewNameButton.interactable = false;
            ShowWarningText("User name Cannot be Longer then 12 Character Long");
            return;
        }
        saveNewNameButton.interactable = false;
        HideWarningText();
        LeaderboardCreator.GetLeaderboard(leadBoardPublicKey,false,(recivingData) =>{
            for (int i = 0; i < recivingData.Length; i++) {
                if(recivingData[i].Username == newName){
                    ShowWarningText($"<color=yellow>[{newName}]</color> UserName Already Exist");
                    saveNewNameButton.interactable = false;
                }else{
                    HideWarningText();
                    saveNewNameButton.interactable = true;
                }
            }
        });
    }
    private bool IsBadWords(string word){
        return badWordsList.Contains(word);
    }

    public void LoadToMenu(){
        setUsernameWindow.SetActive(false);
        StartCoroutine(GetLoadSceneProgress(1));
    }
    private void ShowWarningText(string warning){
        warningMessageTxt.gameObject.SetActive(true);
        warningMessageTxt.SetText(warning);
        CancelInvoke(nameof(HideWarningText));
        Invoke(nameof(HideWarningText),5f);
    }
    private void HideWarningText(){
        warningMessageTxt.gameObject.SetActive(false);
        warningMessageTxt.SetText(string.Empty);
        CancelInvoke(nameof(HideWarningText));
    }

    

}