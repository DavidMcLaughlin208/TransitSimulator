using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace {
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

            for (int i = 0; i < datastore.initialDeckSize; i++) {
                var card = Instantiate(prefabs.card, pile.transform);
                
                card.SetActive(false);
                card.transform.Find("Text").GetComponent<Text>().text = (i+1).ToString();
                card.name = $"{i + 1}";
                datastore.deck.Add(card);
            }
            datastore.cardsInDrawPile.AddRange(datastore.deck);
        }
    }
}