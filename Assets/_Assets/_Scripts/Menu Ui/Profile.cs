using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Profile : MonoBehaviour {
    [SerializeField] private List<string> badWordsList;
    [SerializeField] private int maxProfileNameLength = 6;
    [SerializeField] private GameSettingsSO gameSettingsSo;
    [SerializeField] private GameDataSO gameDataSo;
    [SerializeField] private WorldLeaderboardSystem worldLeaderboardSystem;
    [SerializeField] private TMP_InputField newUserNameTextField;
    [SerializeField] private MenuSystem menuSystem;
    [SerializeField] private TextMeshProUGUI[] tropyCountTmPro,levelNumberTmPro;
    [SerializeField] private TextMeshProUGUI totalMedalsCountText,totalRacesJoinedText,totalMatchWonText,totalTimePlayed;
    [SerializeField] private TextMeshProUGUI idNumberTmPro;
    [SerializeField] private TextMeshProUGUI userNameMenuTmPro,userNameProfileTmpro;
    [SerializeField] private Image[] experianceAmountBar;
    [SerializeField] private LevelSystemSO levelSystem;
    [SerializeField] private Button saveNewNameButton;
    [SerializeField] private TextMeshProUGUI warningMessageTxt;
    private bool changingName;
    private void Awake(){
        HideWarningText();
        ShowProfileName();
        SetProfileData();
        SetExpAndLevelDatas();
    }
    private void SetExpAndLevelDatas(){
        foreach(TextMeshProUGUI levelNum in levelNumberTmPro){
            levelNum.SetText(string.Concat("LEVEL ",levelSystem.GetLevelNumber()));
        }
        foreach(Image expImage in experianceAmountBar){
            expImage.fillAmount = levelSystem.GetExperianceNormalized();
        }
    }
    private void SetProfileData(){
        totalMedalsCountText.SetText(KeysHolder.GetAmountNormalized(gameDataSo.GetTotalMedalsWon()));
        totalRacesJoinedText.SetText(KeysHolder.GetAmountNormalized(gameDataSo.GetTotalRacesJoined()));
        totalMatchWonText.SetText(KeysHolder.GetAmountNormalized(gameDataSo.GetTotalMatchWon()));
        totalTimePlayed.SetText(KeysHolder.NormalizedTime(gameDataSo.GetGamePlayedTime()));
    }
    private void ShowProfileName(){
        string uName = gameSettingsSo.gameProfileData.username;
        char[] setName = new char[uName.Length];
        if(uName.Length > maxProfileNameLength){
            for (int i = 0; i < setName.Length; i++) {
                if(i < maxProfileNameLength){
                    setName[i] = uName[i];
                }else{
                    setName[i] ='.';
                }
            }
            uName = new string(setName);
        }
        userNameMenuTmPro.SetText(uName);
        userNameProfileTmpro.SetText(gameSettingsSo.gameProfileData.username);
        idNumberTmPro.SetText(gameDataSo.GetUniqueIdNumber());
        foreach(TextMeshProUGUI tropy in tropyCountTmPro){
            tropy.SetText(KeysHolder.GetAmountNormalized(gameDataSo.GetTotalGoldTropyWon()));
        }
    }
    public void ChangeName(){
        // Call from Save Name Button.
        if(!string.IsNullOrEmpty(newUserNameTextField.text)){
            if(newUserNameTextField.text.Length > 6){
                HideWarningText();
                gameSettingsSo.gameProfileData.username = newUserNameTextField.text;
                worldLeaderboardSystem.UpgradeEntryUserName();
                SavingAndLoadingManager.Current.SaveGame();
                ShowProfileName();
                newUserNameTextField.text = string.Empty;
                menuSystem.OpenProfile();
            }else{
                ShowWarningText("User name Must be atleast 6 Character Long");
            }
        }else{
            ShowWarningText("User Name Cannot be Empty");
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
        saveNewNameButton.interactable = false;
        HideWarningText();
        worldLeaderboardSystem.CheckNewNameIsAlreadyPresent(newName,(isPresent) =>{
            if(isPresent){
                ShowWarningText($"<color=yellow>[{newName}]</color> UserName Already Exist");
                saveNewNameButton.interactable = false;
            }else{
                saveNewNameButton.interactable = true;
                HideWarningText();
            }
        });
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

    private bool IsBadWords(string word){
        return badWordsList.Contains(word);
    }


}