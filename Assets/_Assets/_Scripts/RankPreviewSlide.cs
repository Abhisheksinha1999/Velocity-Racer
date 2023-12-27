using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class RankPreviewSlide : MonoBehaviour {
    [SerializeField] private Image carIconImage;
    [SerializeField] private Ease moveingEaseType;
    [SerializeField] private Vector3 moveToPoint;
    [SerializeField] private TextMeshProUGUI rankNameTmPro;
    [SerializeField] private Vector3 startingPoint;
    [SerializeField] private RectTransform rectTransform;
    private void Awake(){
        rectTransform = GetComponent<RectTransform>();
        startingPoint = rectTransform.anchoredPosition;
    }
    public void SetRankNames(string rankName,Sprite carIcon){
        rankNameTmPro.SetText(rankName);
        carIconImage.sprite = carIcon;
        Show();
    }
    public void Show(){
        rectTransform.DOKill(false);
        rectTransform.anchoredPosition = startingPoint;
        rectTransform.DOAnchorPos(moveToPoint,.3f,false).SetEase(moveingEaseType);
    }
    public void Hide(){
        rectTransform.DOAnchorPos(startingPoint,.3f,false).SetEase(moveingEaseType);
    }
}