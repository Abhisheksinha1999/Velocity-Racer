using System;
using Dan.Main;
using UnityEngine;
using Baracuda.Monitoring;
using System.Collections.Generic;
public class WorldLeaderboardSystem : MonoBehaviour {
    private const string leadBoardPublicKey = "26ed28ac70989fe9ac4cd9541af54698778bd2f8c445c4d88168b6e60eba4d9c";
    [SerializeField] private GameDataSO gameData;
    [SerializeField] private GameSettingsSO gameSettings;
    [SerializeField] private RectTransform worldLeaderBoardContantHolder;
    [SerializeField] private WorldLeaderboardCard newWorldLeaderBoardCard;
    [SerializeField] private List<WorldLeaderboardCard> worldLeaderboardCardsList;
    [SerializeField] private ScrollSelect scrollSelect;
    [Monitor,MTextColor(ColorPreset.Red)] private bool isLeaderBoardOnline;
    private bool isLeaderBoardOpen;
    private void Awake() {
        Monitor.StartMonitoring(this);
        this.StartMonitoring();
    }
    private void OnDestroy() {
        Monitor.StopMonitoring(this);
        // Or use this extension method:
        this.StopMonitoring();
    }
    private void Start(){
        isLeaderBoardOpen = false;
        LeaderboardCreator.Ping((isOnline) =>{
            this.isLeaderBoardOnline = isOnline;
            if(isOnline){
                RefreshWorldLeaderBoard();
            }
        });
    }
    public void RefreshLeaderBoardBtnCall(){
        isLeaderBoardOpen = true;
        if(isLeaderBoardOnline){
            RefreshWorldLeaderBoard();
        }
    }
    public void ClosingLeaderBoardBtnCall(){
        isLeaderBoardOpen = false;
    }
    private void RefreshWorldLeaderBoard(){
        
        SetLeaderBoardEntry();
        LeaderboardCreator.GetLeaderboard(leadBoardPublicKey,false,RecheckLeaderBoardUI);
    }

    private void RecheckLeaderBoardUI(Dan.Models.Entry[] receivingData) {
        if(isLeaderBoardOpen){
            Debug.Log("Refreshing LeaderBoard");
            if(receivingData.Length > worldLeaderboardCardsList.Count){
                int deff = receivingData.Length - worldLeaderboardCardsList.Count;
                for (int d = 0; d < deff; d++) {
                    WorldLeaderboardCard card = Instantiate(newWorldLeaderBoardCard,worldLeaderBoardContantHolder);
                    card.HideCard();
                    if(!worldLeaderboardCardsList.Contains(card)){
                        worldLeaderboardCardsList.Add(card);
                    }
                }
            }

            // Total Numbers of Entry in LeaderBoard.............
            int totalEntry = Mathf.Min(worldLeaderboardCardsList.Count,receivingData.Length);
            for (int i = 0; i < worldLeaderboardCardsList.Count; i++) {
                if(i >= totalEntry){
                    worldLeaderboardCardsList[i].HideCard();
                }else{
                    worldLeaderboardCardsList[i].ShowCard();
                }
            }
            // Set the User Name for Each Players in LeaderBoard.....
            for (int i = 0; i < totalEntry; i++) {
                if(receivingData[i].Username == gameSettings.gameProfileData.username){
                    scrollSelect.SetSelected(worldLeaderboardCardsList[i].gameObject);
                }
                worldLeaderboardCardsList[i].SetCardData(i+1,receivingData[i].Username,receivingData[i].Score);
            }

            // is The Leaderboard is Onpen Then Scroll the Local Player UserNames.......
            for (int i = 0; i < totalEntry; i++) {
                if(receivingData[i].Extra == gameSettings.gameData.GetUniqueIdNumber()){
                    scrollSelect.SetSelected(worldLeaderboardCardsList[i].gameObject);
                    break;
                }
            }
        }
    }

    public void SetLeaderBoardEntry(){
        if(isLeaderBoardOpen){
            int scoreAveraging = Mathf.RoundToInt((gameData.GetTotalMatchWon() + gameData.GetTotalMedalsWon() + gameData.GetTotalRacesJoined() + gameData.GetTotalGoldTropyWon()) / 4);
            LeaderboardCreator.UploadNewEntry(leadBoardPublicKey,gameSettings.gameProfileData.username,scoreAveraging,gameSettings.gameData.GetUniqueIdNumber(),(receivingData)=>{ RefreshWorldLeaderBoard(); });
        }
    }
    public void UpgradeEntryUserName() {
        if(isLeaderBoardOnline){
            LeaderboardCreator.UpdateEntryUsername(leadBoardPublicKey,gameSettings.gameProfileData.username,(receivingData)=>{ RefreshWorldLeaderBoard(); });
        }
    }
    public void CheckNewNameIsAlreadyPresent(string currentUserName,Action<bool> AfterCheckComplete){
        bool isPresent = false;
        LeaderboardCreator.GetLeaderboard(leadBoardPublicKey,(receivingData) =>{
            for (int i = 0; i < receivingData.Length; i++) {
                if(receivingData[i].Username == currentUserName){
                    isPresent = true;
                    AfterCheckComplete?.Invoke(isPresent);
                    break;
                }
            }
        });
        AfterCheckComplete?.Invoke(isPresent);
    }

}