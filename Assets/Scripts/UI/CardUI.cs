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
    public Transform drawPile;
    public Transform discardPile;

    public Text drawCount;
    public Text discardCount;
    
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
        drawPile = cardHandRegion.Find("DrawPile").transform;
        discardPile = cardHandRegion.Find("DiscardPile").transform;

        drawCount = datastore.canvasParent.transform.Find("DrawPileCount").GetComponent<Text>();
        drawCount.text = datastore.cardsInDrawPile.Count.ToString();
        datastore.cardsInDrawPile.ObserveCountChanged().SubscribeToText(drawCount);
        discardCount = datastore.canvasParent.transform.Find("DiscardPileCount").GetComponent<Text>();
        discardCount.text = datastore.cardsInDiscard.Count.ToString();
        datastore.cardsInDiscard.ObserveCountChanged().SubscribeToText(discardCount);
        
        RecalculateCardCenters();
        
        datastore.deck.ToList().ForEach(card => {
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
        });
        
        datastore.cardsInDrawPile.ObserveAdd().Subscribe(card => {
            card.Value.transform.position = drawPile.position;
        });

        datastore.cardsInHand.ObserveAdd().Subscribe(card => {
            RecalculateCardCenters();
        });

        datastore.cardsInDiscard.ObserveAdd().Subscribe(card => {
            card.Value.transform.DOMove(discardPile.position, 0.3f);
        });
        
        var drawCardButton = datastore.canvasParent.transform.Find("DrawCardButton").GetComponent<Button>();
        drawCardButton.OnClickAsObservable()
            .Where(_ => datastore.energy.Value >= datastore.drawEnergyCost)
            .Subscribe(_ => {
                datastore.gameEvents.Publish(new CardDrawnEvent());
                
                // the remainder in this block should probably exist in Deck and subscribe to the CardDrawnEvent
                switch (datastore.cardsInDrawPile.Count) {
                    case 0 when datastore.cardsInDiscard.Count == 0:
                        // all cards are in hand, can't draw or shuffle
                        return;
                    case 0:
                        // shuffle discard back into draw pile
                        datastore.cardsInDiscard.ToList().Shuffled().ForEach(i => datastore.cardsInDrawPile.Add(i));
                        datastore.cardsInDiscard.Clear();
                        break;
                }
                var card = datastore.cardsInDrawPile.First();
                datastore.cardsInDrawPile.Remove(card);
                datastore.cardsInHand.Add(card);
            });

        datastore.clickedCard.Where(i => i != null).Subscribe(clickedCard => {
            if (lastClickedCard != null) {
                datastore.cardsInHand.Insert(lastClickedIndex, lastClickedCard);    
            }
            
            lastClickedIndex = datastore.cardsInHand.ToList().FindIndex(i => i == clickedCard);
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

        datastore.gameEvents.Receive<CityChangedEvent>()
            .Where(_ => datastore.clickedCard.Value != null)
            .Subscribe(_ => {
                var card = datastore.clickedCard.Value;
                datastore.cardsInDiscard.Add(card);
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