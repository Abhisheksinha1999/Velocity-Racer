using UnityEngine;
[CreateAssetMenu(menuName = "Configs/TrackSO", fileName = "TrackSO")]
public class TrackSO : ScriptableObject {
    public Texture2D trackTrackTexture;
    public TrackSaveData trackSaveData;
    public bool CanUnlock(int LevelNumber){
        if(LevelNumber >= trackSaveData.levelRequiredToUnlock){
            if(!trackSaveData.isLocked) {
                trackSaveData.isLocked = false;
                return true;
            }
        }
        return false;
    }
    public void SetLoadedData(TrackSaveData trackSaveData){
        if(trackSaveData.tracks == this.trackSaveData.tracks){
            this.trackSaveData = trackSaveData;
        }
    }
    [ContextMenu("Ranodmize UnlockAmount")]
    private void RandomizeUnlockAmount(){
        trackSaveData.levelRequiredToUnlock = Random.Range(1,100);
    }
}
[System.Serializable]
public class TrackSaveData {
    public bool isLocked;
    public int levelRequiredToUnlock = 10;
    public TrackTypes tracks;
}