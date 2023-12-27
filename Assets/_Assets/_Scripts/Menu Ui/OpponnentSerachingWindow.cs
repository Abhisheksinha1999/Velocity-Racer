using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
public class OpponnentSerachingWindow : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI timeInQueue;
    private bool startFind;
    private float currentSerachTime;
    private float maxSerachingTime;
    public Action onSerachComplete;
    private bool foundout;
    public void StartFinding(){
        gameObject.SetActive(true);
        currentSerachTime = 0f;
        maxSerachingTime = Random.Range(3f,10f);
        startFind = true;
        foundout = false;
    }
    public void StopFinding(){
        startFind = false;
        currentSerachTime = 0f;
        maxSerachingTime = Random.Range(4f,6f);
        gameObject.SetActive(false);
    }


    private void Update(){
        if(startFind){
            currentSerachTime += Time.deltaTime;
            timeInQueue.SetText(string.Concat("TIME IN QUEUE : ",KeysHolder.NormalizedTime(currentSerachTime),"s"));
            if(currentSerachTime >= maxSerachingTime){
                if(!foundout){
                    onSerachComplete?.Invoke();
                    foundout = true;
                }
            }
        }
    }
}