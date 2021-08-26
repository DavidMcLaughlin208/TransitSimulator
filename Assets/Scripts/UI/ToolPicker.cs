using System.Data;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniRx;

public class ToolPicker : MonoBehaviour {

    Datastore datastore;

    private void Awake() {
        datastore = this.GetComponent<Datastore>();
    }

    private void Start() {
        var blockPlacerButton = datastore.canvasParent.transform.Find("BlockPlacer").GetComponent<Button>();
        MapButtonToToolType(blockPlacerButton, ToolType.BLOCK_PLACER);
        var shopPlacerButton = datastore.canvasParent.transform.Find("ShopPlacer").GetComponent<Button>();
        MapButtonToToolType(shopPlacerButton, ToolType.SHOP_PLACER);
        var hotelPlacerButton = datastore.canvasParent.transform.Find("HotelPlacer").GetComponent<Button>();
        MapButtonToToolType(hotelPlacerButton, ToolType.HOTEL_PLACER);

        var coffeeColorButton = datastore.canvasParent.transform.Find("CoffeeColorButton").GetComponent<Button>();
        MapButtonToToolColor(coffeeColorButton, DestinationType.COFFEE);
        var teaColorButton = datastore.canvasParent.transform.Find("TeaColorButton").GetComponent<Button>();
        MapButtonToToolColor(teaColorButton, DestinationType.TEA);
        var beerColorButton = datastore.canvasParent.transform.Find("BeerColorButton").GetComponent<Button>();
        MapButtonToToolColor(beerColorButton, DestinationType.BEER);
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
            Debug.Log("Clicked on a color button");
        });

        datastore.activeToolColor.Subscribe(e => {
            if (e == destType) {
                button.gameObject.GetComponent<Image>().color = ColorUtils.GetColorForDestType(destType);
            } else {
                button.gameObject.GetComponent<Image>().color = ColorUtils.solColors[ColorUtils.SolarizedColors.brblack];
            }
        });
    }
}

public enum ToolType {
    BLOCK_PLACER,
    SHOP_PLACER,
    HOTEL_PLACER,
}