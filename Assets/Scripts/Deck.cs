using System;
using System.Collections.Generic;
using System.Linq;
using Transit.Templates;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Transit {
    public class Deck : MonoBehaviour {
        Datastore datastore;
        Prefabs prefabs;
        GameObject pile;

        public void Awake() {
            datastore = GetComponent<Datastore>();
            prefabs = GetComponent<Prefabs>();
        }

        public void Start() {
            pile = GameObject.Find("DrawPile");
            var startingCards = new List<CardData> {
                CardTemplates.BuildBlock,
                CardTemplates.BuildBlock,
                CardTemplates.BuildBlock,
                
                CardTemplates.BuildHotel,
                CardTemplates.BuildHotel,
                
                CardTemplates.BuildShop,
                CardTemplates.BuildShop,
                
                CardTemplates.SpawnPeds,
                CardTemplates.SpawnPeds,
                CardTemplates.SpawnPeds,
            };
            
            startingCards.Shuffled().ForEach(cardType => {
                var card = Instantiate(prefabs.card, pile.transform);
                
                card.transform.Find("Text").GetComponent<Text>().text = cardType.description;
                card.name = cardType.name;
                datastore.deck.Add(card);
            });
            
            datastore.deck.Take(3).ToList().ForEach(card => {
                datastore.cardsInHand.Add(card);
            });
            datastore.deck.Skip(3).ToList().ForEach(card => datastore.cardsInDrawPile.Add(card));

            datastore.completedTrips
                .Where(value => value > 0 && value % datastore.tripsToEnergyConversion.Value == 0)
                .Subscribe(_ => datastore.energy.Value++);

            datastore.gameEvents.Receive<CardDrawnEvent>().Subscribe(_ => datastore.energy.Value -= datastore.drawEnergyCost);
        }
    }
}