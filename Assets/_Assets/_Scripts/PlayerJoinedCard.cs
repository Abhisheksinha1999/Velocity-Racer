using TMPro;
using UnityEngine;

public class PlayerJoinedCard : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI usernameText,rankText;
    public void SetCardsData(int rank,string username){
        transform.SetSiblingIndex(rank - 1);
        usernameText.color = Color.white;
        usernameText.SetText(username);
        SetRank(rank);
    }
    public void SetRank(int rank) {
        rankText.SetText(string.Concat("#",rank));
    }
    public void ShowCard(){
        gameObject.SetActive(true);
    }
    public void HideCard(){
        gameObject.SetActive(false);
    }
}