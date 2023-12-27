using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LeaderboardPlayerCard : MonoBehaviour {
    [SerializeField] private Image cardImage,rankHeader;
    [SerializeField] private TextMeshProUGUI usernameText,rankText,finalTimeText;
    [SerializeField] private Sprite firstRankBodySprite,firstRankHeader,normalRankBodySprite,normalRankHeader;
    public void SetCardsData(int rank,string username,float finalTime = 0f){
        transform.SetSiblingIndex(rank - 1);
        if(rank == 1){
            finalTimeText.color = Color.blue;
            usernameText.color = Color.yellow;
            cardImage.sprite = firstRankBodySprite;
            rankHeader.sprite = firstRankHeader;
        }else {
            finalTimeText.color = Color.white;
            usernameText.color = Color.white;
            cardImage.sprite = normalRankBodySprite;
            rankHeader.sprite = normalRankHeader;
        }
        NormalizedTime(finalTime);
        usernameText.SetText(username);
        SetRank(rank);
    }
    private void NormalizedTime(float currentTime){
        float seconds = Mathf.FloorToInt(currentTime % 60);
        float minutes = Mathf.FloorToInt(currentTime / 60);
        finalTimeText.SetText(string.Format("{0:00M}:{1:00s}",minutes,seconds));
    }

    public void SetRank(int rank) {
        this.rankText.SetText(string.Concat("#",rank));
    }
    public void ShowCard(){
        gameObject.SetActive(true);
    }
    public void HideCard(){
        gameObject.SetActive(false);
    }
}