using TMPro;
using System;
using UnityEngine;
using Baracuda.Monitoring;
public class TimeManager : MonoBehaviour {
    private const string TIMER_SAVE_KEY = "LastTimeClicked";
    public event EventHandler OnClick,OnTimeOver;
    public event EventHandler<OnTimeTickingArgs> OnTimeTicking;
    public class OnTimeTickingArgs : EventArgs{
        public int hours,minit,seconds;
    }
    [SerializeField] private bool canClick;
    [SerializeField] private float msToWait = 86400000;
    [SerializeField] private TextMeshProUGUI[] timeTextArray;
    [Monitor,MTextColor(ColorPreset.Green)]private long lastTimeClicked;
    private int hours,minit,seconds;
    [ContextMenu("Delete Save Time")]
    private void DeleateSaveData(){
        SaveSystemManager.DeleteFile(TIMER_SAVE_KEY);
    }
    private void Awake() {
        Monitor.StartMonitoring(this);
        // Or use this extension method:
        this.StartMonitoring();
        Debug.Log(TIMER_SAVE_KEY + lastTimeClicked);
        
        if(SaveSystemManager.HasSaveFile(TIMER_SAVE_KEY)){
            lastTimeClicked = /* long.Parse(PlayerPrefs.GetString(TIMER_SAVE_KEY)); */SaveSystemManager.Load<long>(TIMER_SAVE_KEY);
        }else{
            lastTimeClicked = 0;
            // PlayerPrefs.SetString(TIMER_SAVE_KEY, lastTimeClicked.ToString());
            SaveSystemManager.Save<long>(lastTimeClicked,TIMER_SAVE_KEY);
        }


        if (!Ready()){
            canClick = false;
        }
    }
    private void OnDestroy() {
        Monitor.StopMonitoring(this);
        // Or use this extension method:
        this.StopMonitoring();
    }
    

    private void Update(){
        if (Ready()){
            OnTimeOver?.Invoke(this,EventArgs.Empty);
            canClick = true;
            SetTimerText("Ready!");
            return;
        }
        long diff = ((long)DateTime.Now.Ticks - lastTimeClicked);
        long m = diff / TimeSpan.TicksPerMillisecond;
        float secondsLeft = (float)(msToWait - m) / 1000.0f;

        // string r = "";
        
        //HOURS
        hours = (Mathf.RoundToInt(secondsLeft) / 3600);
        secondsLeft -= (Mathf.RoundToInt(secondsLeft) / 3600) * 3600;
        //MINUTES
        minit = (Mathf.RoundToInt(secondsLeft) / 60);
        //SECONDS
        seconds = Mathf.RoundToInt((secondsLeft % 60));
        OnTimeTicking?.Invoke(this,new OnTimeTickingArgs {hours = hours,minit = minit,seconds = seconds});
        SetTimerText( "Reward Available in "+ string.Format("{0:00H} : {1:00M} : {2:00S}",hours,minit,seconds));
    }
    private void SetTimerText(string time){
        foreach(TextMeshProUGUI texts in timeTextArray){
            texts.SetText(time);
        }

    }
    public float GetTimeLeftToUnlock(int dayIndex){
        return hours * (dayIndex + 1);
    }



    public void Click() {
        lastTimeClicked = (long)DateTime.Now.Ticks;
        SaveSystemManager.Save<long>(lastTimeClicked,TIMER_SAVE_KEY);
        canClick = false;
        OnClick?.Invoke(this,EventArgs.Empty);
    }
    public bool Ready(){
        long diff = ((long)DateTime.Now.Ticks - lastTimeClicked);
        long m = diff / TimeSpan.TicksPerMillisecond;

        float secondsLeft = (float)(msToWait - m) / 1000.0f;

        if (secondsLeft < 0){
            //DO SOMETHING WHEN TIMER IS FINISHED
            return true;
        }

        return false;
    }

}

