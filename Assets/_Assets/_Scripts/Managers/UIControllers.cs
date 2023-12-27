using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using System.Collections.Generic;
using UnityEngine.Rendering;
using DG.Tweening;
public class UIControllers : MonoBehaviour {
    [SerializeField] private Volume blurVolume;
    [SerializeField] private GameObject endGameWindow;
    [SerializeField] private GameObject settingsWindow;
    [SerializeField] private GameObject startingTimeWindow,playerHud,waitingForPlayerToJoinTimeWindow,waitingForPlayersToFinishTheRace;
    [SerializeField] private TextMeshProUGUI endScreenWaitingTimeRankTxt,startingTimeText,waitingTimeText,pingTxt;
    [SerializeField] private TextMeshProUGUI finalRaceEndTimeWinnerPlayerHud,finalRaceEndTimePlayeOtherPlayerHud;
    [SerializeField] private GameObject rankWindow,finalDataWindow,finalLeaderBoardWindow;
    [SerializeField] private RankPreviewSlide firstRankViewTween,secondRankViewTween,thirdRankViewTween;
    [SerializeField] private List<CarTypeSO> allCarsList;
    [SerializeField] private FinalGainData finalGainData;

    [SerializeField] private LevelSystemSO levelSystem;
    [SerializeField] private GameDataSO gameData;
    [SerializeField,Range(0f,0.1f)] private float updateTimerMax = .16f;
    private float updateTimerLevel,updateTimerCoin,updateTimerCash;

    private int currentLevel,currentCashAmount,currentCoinAmount;
    private int currentExperiance;
    private int currentExperianceToNextLevel;
    private bool isAnimatingLevel,isAnimatingCash,isAnimatingCoin;

    private FunctionUpdater functionUpdaterLevelSystem,functionUpdaterCash,functionUpdaterCoin;

    [Serializable]
    public class FinalGainData{
        public TextMeshProUGUI levelIncrmentedAmountTxt;
        public TextMeshProUGUI cashIncrmentedAmountTxt;
        public TextMeshProUGUI coinIncrmentedAmountTxt;
        public TextMeshProUGUI finalRankText,levelAmount,cashAmount,coinAmount;
        public Image exPBar,cashGainBar,coinGainBar;
    }
    private void Awake(){
        finalLeaderBoardWindow.SetActive(true);
        finalDataWindow.SetActive(false);
        blurVolume.weight = 1f;
        isAnimatingLevel = isAnimatingCash = isAnimatingCoin = false;
        settingsWindow.SetActive(false);
    }
    public void OpenSettingWindow(){
        settingsWindow.SetActive(true);
    }
    public void CloseSettingWindow(){
        settingsWindow.SetActive(false);
    }
    public void SetPing(float ping){
        if(gameData.GetShowPing()){
            if(ping > 100){
                pingTxt.SetText(string.Concat("Ping : ","<color=red>",ping,"</color>"));
            }else{
                pingTxt.SetText(string.Concat("Ping : ","<color=green>",ping,"</color>"));
            }
        }else{
            pingTxt.gameObject.SetActive(false);
        }
    }

    public void ShowHideStartingTimeWindow(bool waiting){
        blurVolume.weight = waiting ? 1f : 0f;
        startingTimeWindow.SetActive(waiting);
    }
    public void ShowHideWaitForOthersToFinishWindow(bool waiting,int rank = -1){
        if(rank > 0){
            string rankSufix;
            if (rank == 1){
                rankSufix = "st";
            }else if(rank == 2){
                rankSufix = "nd";
            }else if(rank == 3){
                rankSufix = "rd";
            }else{
                rankSufix = "th";
            }
            endScreenWaitingTimeRankTxt.SetText(string.Concat(rank,rankSufix));
        }
        waitingForPlayersToFinishTheRace.SetActive(waiting);
    }
    public void ShowStartTime(float time){
        startingTimeText.SetText(time.ToString());
    }
    public void SetWinnerGameEndTime(float time){
        finalRaceEndTimeWinnerPlayerHud.SetText(string.Concat("Wait for the Other Racer.. \n Race Ending in ",time,"s"));
    }
    public void SetOtherGameEndTime(float time){
        finalRaceEndTimePlayeOtherPlayerHud.SetText(string.Concat("Race Ending in ",time,"s"));
    }
    public void ShowHideFinalRaceEndTimeWinnerPlayerHud(bool show){
        finalRaceEndTimeWinnerPlayerHud.gameObject.SetActive(show);
    }
    public void ShowHideFinalRaceEndTimeOtherPlayerHud(bool show){
        finalRaceEndTimePlayeOtherPlayerHud.gameObject.SetActive(show);
    }
    public void WaitforPlayerToJoinWindow(float timer,bool showWindow,bool isMasterPlayer = false){
        if(isMasterPlayer){
            waitingTimeText.gameObject.SetActive(true);
            waitingTimeText.SetText(string.Concat("WAITING TIME : ",Mathf.CeilToInt(timer)));
        }else{
            waitingTimeText.gameObject.SetActive(false);
        }
        waitingForPlayerToJoinTimeWindow.SetActive(showWindow);
    }
    public void ShowHideEndGameWindow(bool show){
        endGameWindow.SetActive(show);
    }
    public void ShowHidePlayHud(bool show){
        playerHud.SetActive(show);
    }
    public void ShowHideRankWindow(bool show){
        rankWindow.SetActive(show);
    }
    public void SetPositions(GamePlayerInfo first,GamePlayerInfo second,GamePlayerInfo third){
        firstRankViewTween.SetRankNames(StringShorting(first.gameProifile.username,4).ToUpper(),GetCarIconSprite(first.gameProifile.carTypeShop));
        secondRankViewTween.SetRankNames(StringShorting(second.gameProifile.username,4).ToUpper(),GetCarIconSprite(second.gameProifile.carTypeShop));
        if(third != null){
            thirdRankViewTween.SetRankNames(StringShorting(third.gameProifile.username,4).ToUpper(),GetCarIconSprite(third.gameProifile.carTypeShop));
        }else{
            thirdRankViewTween.Hide();
        }
    }
    private Sprite GetCarIconSprite(int cartypeIndex){
        Array values = Enum.GetValues(typeof(CarTypeShop));
        for (int i = 0; i < allCarsList.Count; i++) {
            CarTypeShop carType = (CarTypeShop)values.GetValue(cartypeIndex);
            if(allCarsList[i].carSaveData.carTypeShop == carType){
                return allCarsList[i].carIcon;
            }
        }
        return null;
    }
    private string StringShorting(string inpuName,int maxCharacterLength = 6){
        string uName = inpuName;
        char[] setName = new char[uName.Length];
        if(uName.Length > maxCharacterLength){
            for (int i = 0; i < setName.Length; i++) {
                if(i < maxCharacterLength){
                    setName[i] = uName[i];
                }else{
                    setName[i] ='.';
                }
            }
            uName = new string(setName);
        }
        return uName;
    }
    public void SetPlayerDetailsAfterGameEnd(int r) {
        blurVolume.weight = 1f;
        string rankSufix;
        if (r == 1){
            rankSufix = "st";
        }else if(r == 2){
            rankSufix = "nd";
        }else if(r == 3){
            rankSufix = "rd";
        }else{
            rankSufix = "th";
        }
        finalGainData.finalRankText.SetText(string.Concat(r,rankSufix));

    }
    public void ChangeLevelSystem(int newLevel,int addedCash,int addedCoin){
        currentCashAmount = gameData.GetTotalCash();
        currentCoinAmount = gameData.GetTotalCoins();

        finalGainData.coinGainBar.fillAmount = GetCoinAmountNormalized();
        finalGainData.cashGainBar.fillAmount = GetCashAmountNormalized();


        finalGainData.coinAmount.SetText(string.Concat("$",GetAmountNormalized(addedCoin)));
        finalGainData.cashAmount.SetText(string.Concat("$",GetAmountNormalized(addedCash)));


        currentLevel = levelSystem.GetLevelNumber();
        currentExperiance = levelSystem.experiance;
        currentExperianceToNextLevel = levelSystem.experianceToNextLevel;
        finalGainData.exPBar.fillAmount = GetExperianceNormalized();
        finalGainData.levelAmount.SetText(string.Concat("LEVEL ",GetLevelNumber()));
        finalGainData.levelIncrmentedAmountTxt.SetText(string.Concat("+",newLevel));
        levelSystem.AddExperiance(newLevel);
        gameData.IncreaseCash(addedCash);
        gameData.IncreaseCoin(addedCoin);
        
    }
    public void OnLeaveRoomBtnClick(){
        SavingAndLoadingManager.Current?.SaveGame();
        FunctionUpdater.DestroyUpdater(functionUpdaterCash);
        FunctionUpdater.DestroyUpdater(functionUpdaterCoin);
        FunctionUpdater.DestroyUpdater(functionUpdaterLevelSystem);
    }
    public void StartTheFinalRewards(){
        isAnimatingLevel = isAnimatingCoin = isAnimatingCash = true;
        finalGainData.coinIncrmentedAmountTxt.SetText(string.Concat("$",gameData.GetTotalCoins() - currentCoinAmount,"+"));
        finalGainData.cashIncrmentedAmountTxt.SetText(string.Concat("$",gameData.GetTotalCash() - currentCashAmount,"+"));


        finalGainData.cashIncrmentedAmountTxt.rectTransform.DOShakeScale(1,1,10,90,false).SetDelay(.7f).SetEase(Ease.OutQuad).SetLoops(-1,LoopType.Yoyo);
        finalGainData.coinIncrmentedAmountTxt.rectTransform.DOShakeScale(1,1,10,90,false).SetDelay(.7f).SetEase(Ease.OutQuad).SetLoops(-1,LoopType.Yoyo);
        finalGainData.levelIncrmentedAmountTxt.rectTransform.DOShakeScale(1,1,10,90,false).SetDelay(.7f).SetEase(Ease.OutQuad).SetLoops(-1,LoopType.Yoyo);



        functionUpdaterLevelSystem = FunctionUpdater.Create(AnimatedLevelSystemUI);
        functionUpdaterCoin = FunctionUpdater.Create(AnimateCoinAmountUI);
        functionUpdaterCash = FunctionUpdater.Create(AnimateCashAmountUI);
    }
    
    private void AnimatedLevelSystemUI(){
        if(isAnimatingLevel){
            updateTimerLevel += Time.deltaTime;
            while(updateTimerLevel > updateTimerMax){
                updateTimerLevel -= updateTimerMax;
                UpdateAddExperaince();
            }
        }
    }
    private void AnimateCoinAmountUI(){
        if(isAnimatingCoin){
            updateTimerCoin += Time.deltaTime;
            while(updateTimerCoin > updateTimerMax){
                updateTimerCoin -= updateTimerMax;
                UpdateCoinAmount();
            }
        }
    }
    private void AnimateCashAmountUI(){
        if(isAnimatingCash){
            updateTimerCash += Time.deltaTime;
            while(updateTimerCash > updateTimerMax){
                updateTimerCash -= updateTimerMax;
                UpdateCashAmount();
            }
        }
    }
    private void UpdateCashAmount(){
        if(currentCashAmount < gameData.GetTotalCash()){
            AddCash();
        }else{
            isAnimatingCash = false;
            FunctionUpdater.DestroyUpdater(functionUpdaterCash);
        }
    }
    private void UpdateCoinAmount(){
        if(currentCoinAmount < gameData.GetTotalCoins()){
            AddCoin();
        }else{
            isAnimatingCoin = false;
            FunctionUpdater.DestroyUpdater(functionUpdaterCoin);
        }
    }
    private void AddCoin(){
        currentCoinAmount ++;
        finalGainData.coinGainBar.fillAmount = GetCoinAmountNormalized();
        finalGainData.coinAmount.SetText(string.Concat("$",GetAmountNormalized(currentCoinAmount)));
    }
    private void AddCash(){
        currentCashAmount ++;
        finalGainData.cashGainBar.fillAmount = GetCashAmountNormalized();
        finalGainData.cashAmount.SetText(string.Concat("$",GetAmountNormalized(currentCashAmount)));
    }
    private void UpdateAddExperaince(){
        if(currentLevel < levelSystem.GetLevelNumber()){
            AddExperiance();
        }else{
            if(currentExperiance < levelSystem.experiance){
                AddExperiance();
            }else{
                isAnimatingLevel = false;
                FunctionUpdater.DestroyUpdater(functionUpdaterLevelSystem);
            }
        }
    }
    private void AddExperiance(){
        currentExperiance += 40;
        if(currentExperiance >= currentExperianceToNextLevel){
            currentLevel++;
            currentExperiance = 0;
        }
        finalGainData.exPBar.fillAmount = GetExperianceNormalized();
        finalGainData.levelAmount.SetText(string.Concat("LEVEL ",GetLevelNumber()));
    }
    public int GetLevelNumber(){
        return currentLevel + 1;
    }
    public float GetExperianceNormalized(){
        return (float)currentExperiance / currentExperianceToNextLevel;
    }
    public float GetCashAmountNormalized(){
        return (float)currentCashAmount / gameData.GetTotalCash();
    }
    public float GetCoinAmountNormalized(){
        return (float)currentCoinAmount / gameData.GetTotalCoins();
    }
    private string GetAmountNormalized(int amount){
        if(amount >= 100000){
            int cashAmount = Mathf.RoundToInt((float)amount / 100000);
            return string.Concat(cashAmount.ToString("f1"),"M");
        }
        if(amount >= 1000){
            int cashAmount = Mathf.RoundToInt((float)amount / 1000);
            return string.Concat(cashAmount.ToString("f1"),"K");
        }
        return amount.ToString();
    }
}