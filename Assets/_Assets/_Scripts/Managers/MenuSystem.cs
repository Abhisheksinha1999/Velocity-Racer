using TMPro;
using System;
using UnityEngine;
using GamerWolf.Utils;
using UnityEngine.Rendering;
public class MenuSystem : MonoBehaviour {

    [SerializeField] private GameDataSO gameData;
    [SerializeField] private Volume blurVolume;
    [SerializeField] private GameObject mainMenu,garage,personalRoomCreationWindow,Settings,profile,worldLeaderboard,profileNameChangeWindow,dailyRewardWindow;
    [SerializeField] private TextMeshProUGUI[] totalCoins,totalCash;
    #region Events...............

    public event EventHandler OnGarageOpen,OnMenuOpen,OnRoomCreationWindowOpen,OnSettingsWindowOpen,OnProfileOpen,OnWorldLeaderBoardOpen,OnNameChangeWindowOpen,OnDailyRewardWindowOpen;

    #endregion
    private void Start(){
        OpenMainMenu();
        SetTotalCoinsNcash();
        AudioManager.Current.StopAudio(Sounds.SoundType.TrackDay_Loop_2);
        AudioManager.Current.PlayMusic(Sounds.SoundType.TrackDay_Loop_Standerd_1);
        gameData.OnMoneyAmountChanged += GameData_OnMoneyAmountChanged;
    }

    private void GameData_OnMoneyAmountChanged(object sender, EventArgs e) {
        SetTotalCoinsNcash();
    }

    public void SetTotalCoinsNcash(){
        foreach(TextMeshProUGUI coins in totalCoins){
            coins.SetText(string.Concat("$",GetCoinsTextAmountNormalized(gameData.GetTotalCoins())));
        }
        foreach(TextMeshProUGUI coins in totalCash){
            coins.SetText(string.Concat("$",GetCoinsTextAmountNormalized(gameData.GetTotalCash())));
        }
    }
    public string GetCoinsTextAmountNormalized(int amount){
        if(amount >= 100000){
            float cashAmount = (float)amount / 100000;
            return string.Concat(cashAmount.ToString("f1"),"M");
        }
        if(amount >= 1000){
            float cashAmount = (float)amount / 1000;
            return string.Concat(cashAmount.ToString("f1"),"K");
        }
        return amount.ToString();
    }
    public void OpenGarage(){
        HideAllWindows();
        blurVolume.weight = 0f;
        garage.SetActive(true);
        OnGarageOpen?.Invoke(this,EventArgs.Empty);
    }
    public void OpenMainMenu(){
        HideAllWindows();
        blurVolume.weight = 1f;
        mainMenu.SetActive(true);
        OnMenuOpen?.Invoke(this,EventArgs.Empty);
    }
    public void OpenRoomCreationWindow(){
        HideAllWindows();
        blurVolume.weight = 1f;
        personalRoomCreationWindow.SetActive(true);
        OnRoomCreationWindowOpen?.Invoke(this,EventArgs.Empty);
    }
    public void OpenSettings(){
        HideAllWindows();
        blurVolume.weight = 1f;
        mainMenu.SetActive(true);
        Settings.SetActive(true);
        OnSettingsWindowOpen?.Invoke(this,EventArgs.Empty);
    }
    public void OpenProfile(){
        HideAllWindows();
        blurVolume.weight = 1f;
        mainMenu.SetActive(true);
        profile.SetActive(true);
        OnProfileOpen?.Invoke(this,EventArgs.Empty);
    }
    public void OpenWorldLeaderBoard(){
        HideAllWindows();
        mainMenu.SetActive(true);
        worldLeaderboard.SetActive(true);
        OnWorldLeaderBoardOpen?.Invoke(this,EventArgs.Empty);
    }
    public void OpenProfileChangeWindow(){
        HideAllWindows();
        mainMenu.SetActive(true);
        profile.SetActive(true);
        profileNameChangeWindow.SetActive(true);
        OnNameChangeWindowOpen?.Invoke(this,EventArgs.Empty);
    }
    public void OpenDailyChangeWindow(){
        HideAllWindows();
        dailyRewardWindow.SetActive(true);        
        OnDailyRewardWindowOpen?.Invoke(this,EventArgs.Empty);
    }
    private void HideAllWindows(){
        // blurVolume.weight = 0f;
        mainMenu.SetActive(false);
        garage.SetActive(false);
        personalRoomCreationWindow.SetActive(false);
        Settings.SetActive(false);
        profile.SetActive(false);
        worldLeaderboard.SetActive(false);
        profileNameChangeWindow.SetActive(false);
        dailyRewardWindow.SetActive(false);
    }
}