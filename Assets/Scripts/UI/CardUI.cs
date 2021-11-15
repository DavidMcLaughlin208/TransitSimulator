using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using UnityEditorInternal;

public class CardUI : MonoBehaviour {
    Datastore datastore;
    Prefabs prefabs;

    public Transform cardHandRegion;
    public Transform playArea;

    
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
            switch (datastore.cardsInDrawPile.Count) {
                case 0 when datastore.cardsInDiscard.Count == 0:
                    // all cards are in hand, can't draw or shuffle
                    return;
                case 0:
                    // shuffle discard back into draw pile
                    datastore.cardsInDrawPile = datastore.cardsInDiscard.Shuffled();
                    datastore.cardsInDiscard.Clear();
                    break;
            }
            var card = datastore.cardsInDrawPile.First();
            card.SetActive(true);
            datastore.cardsInDrawPile.Remove(card);
            
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
            
            datastore.cardsInHand.Add(card);
            RecalculateCardCenters();
        });

        datastore.clickedCard.Where(i => i != null).Subscribe(clickedCard => {
            if (lastClickedCard != null) {
                datastore.cardsInHand.Insert(lastClickedIndex, lastClickedCard);    
            }
            
            lastClickedIndex = datastore.cardsInHand.FindIndex(i => i == clickedCard);
            datastore.cardsInHand.Remove(clickedCard);
            clickedCard.transform.DOMove(playArea.transform.position, 0.2f);
            lastClickedCard = clickedCard;

            RecalculateCardCenters();
        });

        datastore.hoveredCard.Subscribe(hoveredCard => {
            datastore.cardsInHand
                .Select((card, index) => new {card, index}).ToList()
                .ForEach(i => {
                    var intendedPosition = cardsInHandCenters[i.index];
                    i.card.transform.SetSiblingIndex(i.index);
                    if (i.card == hoveredCard) {
                        intendedPosition = cardsHoveredCenters[i.index];
                        i.card.transform.SetAsLastSibling();
                    }
                    if ((Vector2) i.card.transform.position != intendedPosition) {
                        i.card.transform.DOMove(intendedPosition, 0.2f);    
                    }
                });
        });
        
        datastore.gameEvents.Receive<CityChangedEvent>().Subscribe(_ => {
            var card = datastore.clickedCard.Value;
            datastore.cardsInDiscard.Add(card);
            card.SetActive(false);
            datastore.clickedCard.Value = null;
            lastClickedCard = null;
            lastClickedIndex = 0;
            
        });
    }

    void RecalculateCardCenters() {
        cardsInHandCenters = Utils.getCenterPointsInHorizontalSpread(
            cardHandRegion.position,
            ((RectTransform)cardHandRegion.transform).rect.width - 300,
            datastore.cardsInHand.Count,
            ((RectTransform)prefabs.card.transform).rect.width
        );
        cardsHoveredCenters = cardsInHandCenters.Select(i => i + Vector2.up * 30).ToList();
        for (var i = 0; i < datastore.cardsInHand.Count; i++) {
            var curCard = datastore.cardsInHand[i];
            curCard.transform.SetSiblingIndex(i);
            var newCenter = cardsInHandCenters[i];
            curCard.transform.DOMove(newCenter, 0.3f).SetEase(Ease.OutCubic);
        }
    }
}