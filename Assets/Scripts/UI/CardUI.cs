using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class CardUI : MonoBehaviour {
    Datastore datastore;
    Prefabs prefabs;
    public List<GameObject> cards = new List<GameObject>();
    public List<IDisposable> hoverAnims = new List<IDisposable>();

    private void Awake() {
        datastore = this.GetComponent<Datastore>();
        prefabs = this.GetComponent<Prefabs>();
    }

    private void Start() {
        var cardHandRegion = datastore.canvasParent.transform.Find("CardHandRegion").transform;
        var drawCardButton = datastore.canvasParent.transform.Find("DrawCardButton").GetComponent<Button>();
        drawCardButton.OnClickAsObservable().Subscribe(_ => {
            hoverAnims.ForEach(anim => anim.Dispose());
            hoverAnims.Clear();
            var card = Instantiate(prefabs.card, cardHandRegion);
            cards.Add(card);
            var newCenters = Utils.getCenterPointsInHorizontalSpread(
                cardHandRegion.position,
                ((RectTransform)cardHandRegion.transform).rect.width - 300,
                cards.Count,
                ((RectTransform)prefabs.card.transform).rect.width
            );
            for (var i = 0; i < cards.Count; i++) {
                var curCard = cards[i];
                var newCenter = newCenters[i];
                curCard.transform.DOMove(newCenter, 0.3f).SetEase(Ease.OutCubic);
                var cardHoverEnter = curCard.GetComponent<Button>().OnPointerEnterAsObservable().Subscribe(_ => {
                    curCard.transform.DOMove(newCenter + Vector2.up * 30, 0.2f);
                });
                var cardHoverExit = curCard.GetComponent<Button>().OnPointerExitAsObservable().Subscribe(_ => {
                    curCard.transform.DOMove(newCenter, 0.2f);
                });
                hoverAnims.AddRange(new List<IDisposable>(){ cardHoverEnter, cardHoverExit });
            }
            
        });
    }
}