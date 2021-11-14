using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class CardUI : MonoBehaviour {
    Datastore datastore;
    Prefabs prefabs;

    public Transform cardHandRegion;
    public Transform playArea;

    public int totalDrawnCards = 0; // for debugging before cards have text
    public List<GameObject> cardsInHand = new List<GameObject>();
    public List<Vector2> cardsInHandCenters = new List<Vector2>();
    public List<Vector2> cardsHoveredCenters = new List<Vector2>();
    public int lastClickedIndex = 0;
    public GameObject lastClickedCard;

    private void Awake() {
        datastore = this.GetComponent<Datastore>();
        prefabs = this.GetComponent<Prefabs>();
    }

    private void Start() {
        cardHandRegion = datastore.canvasParent.transform.Find("CardHandRegion").transform;
        playArea = cardHandRegion.Find("PlayArea").transform;
        
        var drawCardButton = datastore.canvasParent.transform.Find("DrawCardButton").GetComponent<Button>();
        drawCardButton.OnClickAsObservable().Subscribe(_ => {
            totalDrawnCards++;
            var card = Instantiate(prefabs.card, cardHandRegion);
            card.transform.Find("Text").GetComponent<Text>().text = totalDrawnCards.ToString();
            card.GetComponent<Button>().OnPointerEnterAsObservable().Subscribe(_ => {
                datastore.hoveredCard.Value = card;
            });
            card.GetComponent<Button>().OnPointerExitAsObservable().Subscribe(_ => {
                datastore.hoveredCard.Value = null;
            });
            card.GetComponent<Button>().OnPointerClickAsObservable().Subscribe(_ => {
                if (datastore.clickedCard.Value != card) {
                    datastore.clickedCard.Value = card;    
                }
            });
            
            cardsInHand.Add(card);
            RecalculateCardCenters();
        });

        datastore.clickedCard.Where(i => i != null).Subscribe(clickedCard => {
            if (lastClickedCard != null) {
                cardsInHand.Insert(lastClickedIndex, lastClickedCard);    
            }
            
            lastClickedIndex = cardsInHand.FindIndex(i => i == clickedCard);
            cardsInHand.Remove(clickedCard);
            clickedCard.transform.DOMove(playArea.transform.position, 0.2f);
            lastClickedCard = clickedCard;

            RecalculateCardCenters();
        });

        datastore.hoveredCard.Subscribe(hoveredCard => {
            cardsInHand
                .Select((card, index) => new {card, index}).ToList()
                .ForEach(i => {
                    var intendedPosition = i.card == hoveredCard
                        ? cardsHoveredCenters[i.index]
                        : cardsInHandCenters[i.index];
                    if ((Vector2) i.card.transform.position != intendedPosition) {
                        i.card.transform.DOMove(intendedPosition, 0.2f);    
                    }
                });
        });
    }

    void RecalculateCardCenters() {
        cardsInHandCenters = Utils.getCenterPointsInHorizontalSpread(
            cardHandRegion.position,
            ((RectTransform)cardHandRegion.transform).rect.width - 300,
            cardsInHand.Count,
            ((RectTransform)prefabs.card.transform).rect.width
        );
        cardsHoveredCenters = cardsInHandCenters.Select(i => i + Vector2.up * 30).ToList();
        for (var i = 0; i < cardsInHand.Count; i++) {
            var curCard = cardsInHand[i];
            var newCenter = cardsInHandCenters[i];
            curCard.transform.DOMove(newCenter, 0.3f).SetEase(Ease.OutCubic);
        }
    }
}