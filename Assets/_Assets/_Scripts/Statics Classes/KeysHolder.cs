using UnityEngine;

public static class KeysHolder {
    public const int TRACK_SCENE_INDEX = 2;
    public const int MENU_SCENE_INDEX = 1;
    public const int MAX_PLAYERS = 6;
    public const int MAX_LAPS = 7;
    public static string TRACK_KEY = "TRACKS";
    public static string LAP_KEY = "LAP";
    public static string WAITING_FOR_PLAYER_TIME_KEY = "WaitngForPlayerTime";

    public const byte PHOTON_CODE_MAX = 200;
    public const int MAX_PACKAGE_ARRAY_LENGTH = 8;
    public const float MAX_STARTING_TIME = 5f;
    public const float FINALFINISHINGTIME = 10f;
    public const float START_SOUND_DELAY = .5f;


    // Saving and Loading Keys.
    public const string GAMESETTINGS_SAVE_KEY = "Profile_Data";
    public const string GAMEDATA_SAVE_KEY = "Game_Data";
    public const string LEVEL_DATA_SAVE_KEY = "PlayerLevelSave";
    public static string NormalizedTime(float currentTime){
        float seconds = Mathf.FloorToInt(currentTime % 60);
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float hours = Mathf.FloorToInt(currentTime / 3600);
        return string.Format("{0:00H}:{1:00M}:{2:00s}",hours,minutes,seconds);
    }
    public static string GetAmountNormalized(int amount){
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