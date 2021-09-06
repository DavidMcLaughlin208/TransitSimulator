using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class TrainNetwork : MonoBehaviour {
    Datastore datastore;

    public List<List<Transporter>> lines = new List<List<Transporter>>();
    public int numLines = 0;
    public Transporter? extendOrigin = null;
    public Transporter? newLineOrigin = null;

    public void Start() {
        var god = GameObject.Find("God");
        datastore = god.GetComponent<Datastore>();

        datastore.inputEvents // create a new line between two train stations
            .Receive<ClickEvent>()
            .Where(_ =>
                datastore.activeTool.Value == ToolType.TRAINSTATION_PLACER
                && datastore.activeLineToolType.Value == TrainLineToolType.NEW
            )
            .Where(e => {
                if (datastore.city.TileIsOccupiedByLot(e.cell.ToVec2())) {
                    var lot = datastore.city[e.cell.ToVec2()].occupier.GetComponent<Lot>();
                    return lot.GetBuildingComponents()[typeof(Transporter)] != null;
                }
                return false;
            })
            .Subscribe(e => {
                var clickedStation = datastore.city[e.cell.ToVec2()].occupier.GetComponent<Lot>()
                    .GetBuildingComponents()[typeof(Transporter)].GetComponent<Transporter>();
                if (newLineOrigin == null) {
                    newLineOrigin = clickedStation;
                } else {
                    numLines++;
                    newLineOrigin.lineNumsConnected.Add(numLines);
                    clickedStation.lineNumsConnected.Add(numLines);
                    newLineOrigin.nextStation = clickedStation;
                    clickedStation.prevStation = newLineOrigin;
                    lines.Add(new List<Transporter>() {newLineOrigin, clickedStation});
                    datastore.gameEvents.Publish(new TrainNetworkChangedEvent() {lineChanged = numLines});
                    datastore.gameEvents.Publish(new CityChangedEvent() {});
                }

            });

        // datastore.inputEvents // create a new connection between two train stations on an established line
        //     .Receive<ClickEvent>()
        //     .Where(_ =>
        //         datastore.activeTool.Value == ToolType.TRAINSTATION_PLACER
        //         && datastore.activeLineToolType.Value == TrainLineToolType.EXTEND
        //     )
        //     .Where(e => {
        //         if (datastore.city.TileIsOccupiedByLot(e.cell.ToVec2())) {
        //             var lot = datastore.city[e.cell.ToVec2()].occupier.GetComponent<Lot>();
        //             return lot.GetBuildingComponents()[typeof(Transporter)] != null;
        //         }
        //         return false;
        //     })
        //     .Subscribe(e => {
        //         var clickedStation = datastore.city[e.cell.ToVec2()].occupier.GetComponent<Lot>()
        //             .GetBuildingComponents()[typeof(Transporter)].GetComponent<Transporter>();
        //         if (extendOrigin == null && clickedStation.lineNumsConnected.Count > 0) {
        //             extendOrigin = clickedStation;
        //         } else if (clickedStation.lineNumsConnected.Contains()) {

        //             datastore.gameEvents.Publish(new TrainNetworkChangedEvent());
        //         }

        //     });
    }
}

public enum TrainLineToolType {
    EXTEND,
    NEW
}