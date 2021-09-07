using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ToolPicker : MonoBehaviour {

    Datastore datastore;

    private void Awake() {
        datastore = this.GetComponent<Datastore>();
    }

    private void Start() {
        var handButton = datastore.canvasParent.transform.Find("Hand").GetComponent<Button>();
        MapButtonToToolType(handButton, ToolType.HAND);
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
                button.gameObject.GetComponent<Image>().color = ColorUtils.solColors[ColorUtils.SolarizedColors.blue];
            } else {
                button.gameObject.GetComponent<Image>().color = ColorUtils.solColors[ColorUtils.SolarizedColors.brblack];
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
                button.gameObject.GetComponent<Image>().color = ColorUtils.solColors[ColorUtils.SolarizedColors.brblack];
            }
        });
    }

    public void MapKeyPressToToolColor(KeyCode keyCode, DestinationType destType) {
        datastore.inputEvents
            .Receive<KeyEvent>()
            .Where(e => e.keyCode == keyCode)
            .Subscribe(_ => datastore.activeToolColor.Value = destType);
    }
}

public enum ToolType {
    HAND,
    BLOCK_PLACER,
    SHOP_PLACER,
    HOTEL_PLACER,
    APARTMENT_PLACER,
    PARKINGLOT_PLACER
}