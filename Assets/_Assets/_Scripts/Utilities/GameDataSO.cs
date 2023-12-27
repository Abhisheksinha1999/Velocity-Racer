using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
[CreateAssetMenu(fileName = "Game Data",menuName = "Utils/Game Data")]
public class GameDataSO : ScriptableObject {
    public event EventHandler OnMoneyAmountChanged/* ,OnInputStyleChange */;
    [SerializeField] private bool OnPc;
    [SerializeField] private LevelSystemSO levelSystem;
    [SerializeField] private PlayerSaveData saveData;
    [SerializeField] private List<TrackSO> trackToSaveList;
    [SerializeField] private List<CarTypeSO> carsDataToSaveList;
    public bool GetOnPc(){
        return OnPc;
    }

#region  In Game Save Data..................

    public void IncreaseCash(int amount){
        saveData.totalCash += amount;
        OnMoneyAmountChanged?.Invoke(this,EventArgs.Empty);
    }
    public void IncreaseCoin(int amount){
        saveData.totalCoins += amount;
        OnMoneyAmountChanged?.Invoke(this,EventArgs.Empty);
    }
    public void SpendCash(int amount){
        saveData.totalCash -= amount;
        if(saveData.totalCash <= 0){
            saveData.totalCash = 0;
        }
        OnMoneyAmountChanged?.Invoke(this,EventArgs.Empty);
    }
    public void SpendCoins(int amount){
        saveData.totalCoins -= amount;
        if(saveData.totalCoins <= 0){
            saveData.totalCoins = 0;
        }
        OnMoneyAmountChanged?.Invoke(this,EventArgs.Empty);
    }
    public void SetReviewd(){
        saveData.settingsSaveData.isReviewd = true;
    }
    public int GetTotalCash(){
        return saveData.totalCash;
    }
    public int GetTotalCoins(){
        return saveData.totalCoins;
    }

#endregion



#region Settings Save Data.......................


    public void SetHasAdsInGame(bool value){
        saveData.settingsSaveData.hasAdsInGame = value;
    }
    public bool GetHasAds(){
        return saveData.settingsSaveData.hasAdsInGame;
    }
    public bool IsRevived(){
        return saveData.settingsSaveData.isReviewd;
    }
    public void SetQualityLevel(int index){
        saveData.settingsSaveData.quaityIndex = index;
    }
    public void SetFrameRate(int index){
        saveData.settingsSaveData.frameRate = index;
    }
    public void ToggleSpeedType(){
        saveData.settingsSaveData.speedIsKmph = !saveData.settingsSaveData.speedIsKmph;
    }
    public void ToggleShowNetworkPing(){
        saveData.settingsSaveData.showNetworkPing = !saveData.settingsSaveData.showNetworkPing;
    }
    public void ToggleVFX(){
        saveData.settingsSaveData.isVFXOn = !saveData.settingsSaveData.isVFXOn;
    }
    public void ToggleHaptics(){
        saveData.settingsSaveData.isHapticsOn = !saveData.settingsSaveData.isHapticsOn;
    }
    public void SetMusicVolumeAmount(float amount){
        saveData.settingsSaveData.musicVolumeAmount = amount;
    }
    public void SetSfxVolumeAmount(float amount){
        saveData.settingsSaveData.sfxVolumeAmount = amount;
    }
    public int GetFrameRate(){
        return saveData.settingsSaveData.frameRate;
    }
    public int GetQualityIndex(){
        return saveData.settingsSaveData.quaityIndex;
    }
    public bool GetSpeedType(){
        return saveData.settingsSaveData.speedIsKmph;
    }
    public bool GetShowPing(){
        return saveData.settingsSaveData.showNetworkPing;
    }
    public float GetMusicVolumeAmount(){
        return saveData.settingsSaveData.musicVolumeAmount;
    }
    public float GetSfxVolumeAmount(){
        return saveData.settingsSaveData.sfxVolumeAmount;
    }
    public bool GetVFXState(){
        return saveData.settingsSaveData.isVFXOn;
    }
    public bool GetHapticState(){
        return saveData.settingsSaveData.isHapticsOn;
    }
    public void SetRegion(string regionName){
        saveData.settingsSaveData.regionName = regionName;
    }
    public string GetRegion(){
        return saveData.settingsSaveData.regionName;
    }
    public void SetCurrentControllType(ControllsSelectionManager.ControllType controllType){
        saveData.settingsSaveData.controllType = controllType;
    }
    public ControllsSelectionManager.ControllType GetControllType(){
        return saveData.settingsSaveData.controllType;
    }

#endregion
    

#region Profile Data...........
    public bool IsFirstRun(){
        return saveData.settingsSaveData.firstRun;
    }

    public void SetUntqueIdNumberRandom(char[] playerNameSufix){
        if(saveData.settingsSaveData.firstRun){
            string nameSuf = new string(playerNameSufix);
            saveData.settingsSaveData.firstRun = false;
            int lastNumbersOfPlayerSuffix = 0;
            for (int i = 0; i < 9; i++){
                int incrementAmount = Random.Range(1,10);
                lastNumbersOfPlayerSuffix += incrementAmount;
            }
            saveData.profileSaveData.uniqueIdNumber = string.Concat("#",nameSuf,lastNumbersOfPlayerSuffix);
        }
    }
#region DailyRewrdsData...................
    public void SetClamedBonus(bool _value){
        saveData.dailyRewards.isClamedDailyBonus = _value;
    }
    public bool GetIsClamedBonus(){
        return saveData.dailyRewards.isClamedDailyBonus;
    }
    public void IncreaseCurrentDayNumber(){
        saveData.currentDay++;
        if(saveData.currentDay >= 7){
            saveData.currentDay = 0;
        }
    }
    public void SetDailyBonusAlreadyShown(bool isShown) {
        saveData.dailyRewards.DailyBonusAlreadyShown = isShown;
    }
    public int GetcurrentDay(){
        return saveData.currentDay;
    }

#endregion
    public string GetUniqueIdNumber(){
        return saveData.profileSaveData.uniqueIdNumber;
    }
    public void DecreaseExperacne(int amount){
        levelSystem.DecreaseExp(amount);
    }

    public void IncreaseMatchWon(int amount){
        saveData.profileSaveData.matchWonNumber += amount;
    }
    public void DecreaseMatchWon(int amount){
        saveData.profileSaveData.matchWonNumber -= amount;
    }
    public void IncreaseMedalsCount(int amount){
        saveData.profileSaveData.medalsTotalRecived += amount;
    }
    public void DecreaseMedals(int amount){
        saveData.profileSaveData.medalsTotalRecived -= amount;
    }
    public void IncreaseGoldTropyCount(int amount){
        saveData.profileSaveData.totalGoldTropy += amount;
    }
    public void IncreaseTotalRacesJoinedCount(int amount){
        saveData.profileSaveData.totalRacesJoined += amount;
    }
    public void IncreaseGamePlayed(float time){
        saveData.profileSaveData.totalTimePlayed = time;
    }

    public int GetTotalMatchWon(){
        return saveData.profileSaveData.matchWonNumber;
    }
    public int GetTotalMedalsWon(){
        return saveData.profileSaveData.medalsTotalRecived;
    }
    public int GetTotalGoldTropyWon(){
        return saveData.profileSaveData.totalGoldTropy;
    }
    public int GetTotalRacesJoined(){
        return saveData.profileSaveData.totalRacesJoined;
    }
    public float GetGamePlayedTime(){
        return saveData.profileSaveData.totalTimePlayed;
    }
    public void SetCurrentCar(CarTypeShop carTypeShop){
        saveData.carType = carTypeShop;
    }
    


#endregion

#region Saving and Loading................

    public void Save() {
        for(int i = 0; i < saveData.trackSaveDatasList.Count; i++){
            saveData.trackSaveDatasList[i] = trackToSaveList[i].trackSaveData;
        }
        for(int i = 0; i < saveData.carSaveDatas.Count; i++){
            saveData.carSaveDatas[i] = carsDataToSaveList[i].carSaveData;
        }
        SaveSystemManager.Save<PlayerSaveData>(saveData,KeysHolder.GAMEDATA_SAVE_KEY);
        levelSystem.SetLevelSaveData();
    }

    public void Load(){
        PlayerSaveData loadedData = SaveSystemManager.Load<PlayerSaveData>(KeysHolder.GAMEDATA_SAVE_KEY);
        saveData = loadedData;
        levelSystem.Load();
        for(int i = 0; i < trackToSaveList.Count; i++){
            trackToSaveList[i].SetLoadedData(saveData.trackSaveDatasList[i]);
        }
        for(int i = 0; i < carsDataToSaveList.Count; i++){
            carsDataToSaveList[i].SetCarSaveData(saveData.carSaveDatas[i]);
        }
    }


    [Serializable]
    private class PlayerSaveData{
        public int currentDay;
        public int totalCash;
        public int totalCoins;
        public CarTypeShop carType;
        public ProifleSaveData profileSaveData;
        public SettingsSaveData settingsSaveData;
        public DailyRewards dailyRewards;
        public List<TrackSaveData> trackSaveDatasList;
        public List<CarTypeSO.CarSaveData> carSaveDatas;
    }
    [Serializable]
    private class DailyRewards {
        public bool isClamedDailyBonus;
        public bool DailyBonusAlreadyShown;
    }
    [Serializable]
    private class SettingsSaveData{
        public bool firstRun = true;
        public bool hasAdsInGame;
        public bool isReviewd;
        public bool isVFXOn;
        public bool isHapticsOn;
        public float musicVolumeAmount;
        public float sfxVolumeAmount;
        public int quaityIndex;
        public int frameRate;
        public bool speedIsKmph;
        public bool showNetworkPing;
        public string regionName = "in";
        public ControllsSelectionManager.ControllType controllType;
    }
    [Serializable]
    private class ProifleSaveData {
        public string uniqueIdNumber = string.Empty;
        public int matchWonNumber = 10;
        public int totalRacesJoined = 45;
        public int medalsTotalRecived = 20,totalGoldTropy = 1;
        public float totalTimePlayed = 0;
    }


#endregion


}
