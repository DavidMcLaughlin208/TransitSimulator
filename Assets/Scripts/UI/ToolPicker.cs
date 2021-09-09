using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ToolPicker : MonoBehaviour {

    Datastore datastore;

    ToolType? lastSelectedTool;

    private void Awake() {
        datastore = this.GetComponent<Datastore>();
    }

    private void Start() {
        var handButton = datastore.canvasParent.transform.Find("Hand").GetComponent<Button>();
        MapButtonToToolType(handButton, ToolType.HAND);
        MapHeldKeyToTool(KeyCode.LeftAlt, ToolType.HAND);

        var blockPlacerButton = datastore.canvasParent.transform.Find("BlockPlacer").GetComponent<Button>();
        MapButtonToToolType(blockPlacerButton, ToolType.BLOCK_PLACER);

        var shopPlacerButton = datastore.canvasParent.transform.Find("ShopPlacer").GetComponent<Button>();
        MapButtonToToolType(shopPlacerButton, ToolType.SHOP_PLACER);

        var hotelPlacerButton = datastore.canvasParent.transform.Find("HotelPlacer").GetComponent<Button>();
        MapButtonToToolType(hotelPlacerButton, ToolType.HOTEL_PLACER);

        var apartmentPlacerButton = datastore.canvasParent.transform.Find("ApartmentPlacer").GetComponent<Button>();
        MapButtonToToolType(apartmentPlacerButton, ToolType.APARTMENT_PLACER);

        var parkingLotPlacerButton = datastore.canvasParent.transform.Find("ParkingLotPlacer").GetComponent<Button>();
        MapButtonToToolType(parkingLotPlacerButton, ToolType.PARKINGLOT_PLACER);

        var trainStationPlacerButton = datastore.canvasParent.transform.Find("TrainStationPlacer").GetComponent<Button>();
        MapButtonToToolType(trainStationPlacerButton, ToolType.TRAINSTATION_PLACER);

        var trainNewLineButton = datastore.canvasParent.transform.Find("TrainStationNewLine").GetComponent<Button>();
        var trainExtendLineButton = datastore.canvasParent.transform.Find("TrainStationExtendLine").GetComponent<Button>();
        datastore.activeTool.Subscribe(e => {
            trainNewLineButton.gameObject.SetActive(e == ToolType.TRAINSTATION_PLACER);
            trainExtendLineButton.gameObject.SetActive(e == ToolType.TRAINSTATION_PLACER);
            datastore.activeLineToolType.Value = null;
        });
        MapButtonToTrainLineToolType(trainNewLineButton, TrainLineToolType.NEW);
        MapButtonToTrainLineToolType(trainExtendLineButton, TrainLineToolType.EXTEND);


        var coffeeColorButton = datastore.canvasParent.transform.Find("CoffeeColorButton").GetComponent<Button>();
        MapButtonToToolColor(coffeeColorButton, DestinationType.COFFEE);
        MapKeyPressToToolColor(KeyCode.C, DestinationType.COFFEE);

        var teaColorButton = datastore.canvasParent.transform.Find("TeaColorButton").GetComponent<Button>();
        MapButtonToToolColor(teaColorButton, DestinationType.TEA);
        MapKeyPressToToolColor(KeyCode.T, DestinationType.TEA);

        var beerColorButton = datastore.canvasParent.transform.Find("BeerColorButton").GetComponent<Button>();
        MapButtonToToolColor(beerColorButton, DestinationType.BEER);
        MapKeyPressToToolColor(KeyCode.B, DestinationType.BEER);

        datastore.activeTool.Value = ToolType.HAND;
    }

    public void MapButtonToToolType(Button button, ToolType toolType) {
        button.OnClickAsObservable().Subscribe(_ => {
            datastore.activeTool.Value = toolType;
        });

        datastore.activeTool.Subscribe(e => {
            if (e == toolType) {
                button.gameObject.GetComponent<Image>().color = ColorUtils.getColor(ColorUtils.Colors.SelectedButton);
            } else {
                button.gameObject.GetComponent<Image>().color = ColorUtils.getColor(ColorUtils.Colors.UnselectedButton);
            }
        });
    }

    public void MapButtonToTrainLineToolType(Button button, TrainLineToolType lineToolType) {
        button.OnClickAsObservable().Subscribe(_ => {
            datastore.activeLineToolType.Value = lineToolType;
        });

        datastore.activeLineToolType.Subscribe(e => {
            if (e == lineToolType) {
                button.gameObject.GetComponent<Image>().color = ColorUtils.getColor(ColorUtils.Colors.SelectedButton);
            } else {
                button.gameObject.GetComponent<Image>().color = ColorUtils.getColor(ColorUtils.Colors.UnselectedButton);
            }
        });
    }

    public void MapButtonToToolColor(Button button, DestinationType destType) {
        button.OnClickAsObservable().Subscribe(_ => {
            datastore.activeToolColor.Value = destType;
        });

        datastore.activeToolColor.Subscribe(e => {
            if (e == destType) {
                button.gameObject.GetComponent<Image>().color = ColorUtils.GetColorForDestType(destType);
            } else {
                button.gameObject.GetComponent<Image>().color = ColorUtils.getColor(ColorUtils.Colors.UnselectedButton);
            }
        });
    }

    public void MapKeyPressToToolColor(KeyCode keyCode, DestinationType destType) {
        datastore.inputEvents
            .Receive<KeyEvent>()
            .Where(e => e.keyCode == keyCode)
            .Subscribe(_ => datastore.activeToolColor.Value = destType);
    }

    public void MapHeldKeyToTool(KeyCode keyCode, ToolType toolType) {
        datastore.inputEvents
            .Receive<KeyEvent>()
            .Where(e => e.keyCode == default(KeyCode))
            .Subscribe(e => {
                if (e.heldKeys.Contains(keyCode)) {
                    lastSelectedTool = datastore.activeTool.Value;
                    datastore.activeTool.Value = toolType;
                } else {
                    datastore.activeTool.Value = lastSelectedTool;
                }
            });
    }
}

public enum ToolType {
    HAND,
    BLOCK_PLACER,
    SHOP_PLACER,
    HOTEL_PLACER,
    APARTMENT_PLACER,
    PARKINGLOT_PLACER,
    TRAINSTATION_PLACER,
}