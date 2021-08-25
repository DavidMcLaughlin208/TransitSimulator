using System.Data;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniRx;

public class ToolPicker : MonoBehaviour {

    Datastore datastore;

    public Dictionary<ToolType, MonoBehaviour> toolComponentMap = new Dictionary<ToolType, MonoBehaviour>();

    private void Awake() {
        datastore = this.GetComponent<Datastore>();
        datastore.activeTool.Value = this.GetComponent<Placer>();

        toolComponentMap.Add(ToolType.BLOCK_PLACER, this.GetComponent<Placer>());
    }

    private void Start() {
        var blockPlacerButton = datastore.canvasParent.transform.Find("BlockPlacer").GetComponent<Button>();
        blockPlacerButton.OnClickAsObservable().Subscribe(_ => {
            var correspondingTool = toolComponentMap[ToolType.BLOCK_PLACER];
            if (correspondingTool == datastore.activeTool.Value) {
                datastore.activeTool.Value = null;
                blockPlacerButton.gameObject.GetComponent<Image>().color = ColorUtils.solColors[ColorUtils.SolarizedColors.brblack];
            } else {
                datastore.activeTool.Value = correspondingTool;
                blockPlacerButton.gameObject.GetComponent<Image>().color = ColorUtils.solColors[ColorUtils.SolarizedColors.blue];
            }
        });
    }
}

public enum ToolType {
    BLOCK_PLACER
}