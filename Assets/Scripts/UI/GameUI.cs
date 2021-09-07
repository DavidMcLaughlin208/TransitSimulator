using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class GameUI : MonoBehaviour {
    Datastore datastore;

    private void Awake() {
        datastore = this.GetComponent<Datastore>();
    }

    private void Start() {
        var timeScaleSlider = datastore.canvasParent.transform.Find("TimeScaleSlider").GetComponent<Slider>();
        timeScaleSlider.OnValueChangedAsObservable().Subscribe(newValue => {
            datastore.tickModifier.Value = newValue;
        });

        var populationValue = datastore.canvasParent.transform.Find("PopulationValueText").GetComponent<Text>();
        datastore.totalPopulation.SubscribeToText(populationValue);

        var completedTripsValue = datastore.canvasParent.transform.Find("CompletedTripsValueText").GetComponent<Text>();
        datastore.completedTrips.SubscribeToText(completedTripsValue);
    }
}