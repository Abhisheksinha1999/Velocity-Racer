using UnityEngine;
using System.Collections.Generic;
using Baracuda.Monitoring;

public class TrackCheckPoint : MonoBehaviour {

    [Header("Display")]
    [SerializeField] private Transform currentTrackCheckPointHolder;
    [Monitor] private List<Transform> carList;
    private List<CheckPointSingle> CheckPointList;
    [Monitor,MGroupColor(ColorPreset.Cyan)] private List<int> currentCheckPointIndexList = new List<int>();
    private void Awake(){
        Monitor.StartMonitoring(this);
        // Or use this extension method:
        this.StartMonitoring();
        carList = new List<Transform>();
    }
    private void OnDestroy() {
        Monitor.StopMonitoring(this);
        // Or use this extension method:
        this.StopMonitoring();
    }
    public void SetUp(List<Transform> driversList){
        carList = driversList;
        CheckPointList = new List<CheckPointSingle>();
        currentCheckPointIndexList = new List<int>();
        foreach(Transform checkPoint in currentTrackCheckPointHolder){
            checkPoint.GetChild(0).gameObject.SetActive(true);
            if(checkPoint.GetChild(0).TryGetComponent(out CheckPointSingle checkPointSingle)){
                checkPointSingle.SetTrackCheckPoint(this);
                if(!CheckPointList.Contains(checkPointSingle)){
                    CheckPointList.Add(checkPointSingle);
                }
            }
        }
        CheckPointList.Reverse();
        foreach(Transform cars in carList){
            currentCheckPointIndexList.Add(0);
        }
        currentTrackCheckPointHolder.gameObject.SetActive(true);
    }
    public void DriverFromList(Transform driverT){
        if(carList.Contains(driverT)){
            int carIndex = carList.IndexOf(driverT);
            currentCheckPointIndexList.RemoveAt(carIndex);
            carList.RemoveAt(carIndex);
        }
    }
    public List<CheckPointSingle> GetCheckPointList(){
        List<CheckPointSingle> returningList = new List<CheckPointSingle>();
        foreach(Transform checkPoint in currentTrackCheckPointHolder){
            if(checkPoint.GetChild(0).TryGetComponent(out CheckPointSingle checkPointSingle)){
                if(!returningList.Contains(checkPointSingle)){
                    returningList.Add(checkPointSingle);
                }
            }
        }
        returningList.Reverse();
        return returningList;
    }
    public void PlayerThrowCheckPoint(CheckPointSingle checkPointSingle,Transform carT){
        int currentCheckPointIndex = currentCheckPointIndexList[carList.IndexOf(carT)];
        if(CheckPointList.IndexOf(checkPointSingle) == currentCheckPointIndex){
            currentCheckPointIndexList[carList.IndexOf(carT)] = (currentCheckPointIndex + 1) % CheckPointList.Count;
            if(carT.TryGetComponent(out CarDriver currentDriver)){
                currentDriver.OnCorrectCheckPointCrossed(checkPointSingle.transform.position,CheckPointList[currentCheckPointIndexList[carList.IndexOf(carT)] + 1].transform);
                if(currentCheckPointIndexList[carList.IndexOf(carT)] == 0){
                    currentDriver.IncreaseLapsCount();
                }
            }
            if (carT.TryGetComponent(out CarDriverAI driverAI)){
                driverAI.OnCorrectCheckPointCrossed();
                if(currentCheckPointIndexList[carList.IndexOf(carT)] == 0){
                    driverAI.IncreaseLapsCount();
                }
            }
        }else{
            if(carT.TryGetComponent(out CarDriver driver)){
                driver.OnWrongCheckPointCrossed();
            }
            /* if(carT.TryGetComponent(out CarDriverAI driverAI)){
                // driverAI.OnWrongCheckPointCrossed();
            } */
        }
    }
    public int GetTotalCheckPointCount(){// Gives total Number of CheckPoint.
        return currentTrackCheckPointHolder.childCount;
    }
}