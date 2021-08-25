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
        var buildingPlacerButton = datastore.canvasParent.transform.Find("CoffeeShopPlacer").GetComponent<Button>();
        MapButtonToToolType(buildingPlacerButton, ToolType.BUILDING_PLACER);
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
}

public enum ToolType {
    BLOCK_PLACER,
    BUILDING_PLACER,
}