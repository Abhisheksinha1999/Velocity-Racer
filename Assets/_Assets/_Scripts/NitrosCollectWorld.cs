using Photon.Pun;
using UnityEngine;

public class NitrosCollectWorld : MonoBehaviourPun,IPunObservable {
    [SerializeField] private GameObject visual;
    [SerializeField] private float RespawnTime,nostIncreaseAmount = 2f;
    private float currentRespawnTime;
    private bool show;
    private void Start(){
        show = true;
        currentRespawnTime = RespawnTime;
        if(photonView.IsMine){
            photonView.RPC(nameof(ShowHideVisualRpc),RpcTarget.AllBuffered);
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if(stream.IsWriting){
            stream.SendNext(currentRespawnTime);
        }else{
            currentRespawnTime = (float) stream.ReceiveNext();
        }
    }
    private void Update(){
        if(!show){
            currentRespawnTime -= Time.deltaTime;
            if(currentRespawnTime <= 0f){
                currentRespawnTime = RespawnTime;
                show = true;
                if(photonView.IsMine){
                    photonView.RPC(nameof(ShowHideVisualRpc),RpcTarget.AllBuffered);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider coli){
        if(coli.transform.TryGetComponent(out CarDriver driver)){
            show = false;
            driver.IncreaseNosTime(nostIncreaseAmount);
            if(photonView.IsMine){
                photonView.RPC(nameof(ShowHideVisualRpc),RpcTarget.AllBuffered);
            }
        }
        if(coli.transform.TryGetComponent(out CarDriverAI driverAi)){
            show = false;
            driverAi.IncreaseNosTime(nostIncreaseAmount);
            if(photonView.IsMine){
                photonView.RPC(nameof(ShowHideVisualRpc),RpcTarget.AllBuffered);
            }
        }
    }
    [PunRPC]
    private void ShowHideVisualRpc(){
        visual.SetActive(show);
    }
}