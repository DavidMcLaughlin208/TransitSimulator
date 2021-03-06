using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class TrainNetwork : MonoBehaviour {
    Prefabs prefabs;
    Datastore datastore;

    public List<List<Transporter>> lines = new List<List<Transporter>>();
    public List<LineRenderer> lineRenderers = new List<LineRenderer>();
    public List<Color> lineColors = new List<Color>();
    public Transporter? extendOrigin = null;
    public Transporter? newLineOrigin = null;

    public void Start() {
        prefabs = this.GetComponent<Prefabs>();
        datastore = this.GetComponent<Datastore>();

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
                    var connectedStations = new List<Transporter>() {newLineOrigin, clickedStation};
                    var newLineNumber = ConstructNewLine(connectedStations);
                    RefreshLineConnections(newLineNumber);
                    newLineOrigin.lineNumsConnected.Add(newLineNumber);
                    clickedStation.lineNumsConnected.Add(newLineNumber);
                    datastore.gameEvents.Publish(new TrainNetworkChangedEvent() {lineChanged = newLineNumber});
                    datastore.gameEvents.Publish(new CityChangedEvent() {});
                    newLineOrigin = null;
                    SpawnNewTrain(clickedStation, newLineNumber);
                }
            });

        datastore.inputEvents // create a new connection between to a single station from an established line
            .Receive<ClickEvent>()
            .Where(_ =>
                datastore.activeTool.Value == ToolType.TRAINSTATION_PLACER
                && datastore.activeLineToolType.Value == TrainLineToolType.EXTEND
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
                if (extendOrigin == null && clickedStation.lineNumsConnected.Count > 0) {
                    extendOrigin = clickedStation;
                } else {
                    var lineToExtend = extendOrigin.lineNumsConnected.First();
                    var stationsInLine = lines[lineToExtend];
                    if (!clickedStation.lineNumsConnected.Contains(lineToExtend) && !stationsInLine.Contains(clickedStation)) {
                        var indexOfOriginStation = stationsInLine.FindIndex(i => i == extendOrigin);
                        stationsInLine.Insert(indexOfOriginStation + 1, clickedStation);
                        RefreshLineConnections(lineToExtend);
                        UpdateLineRenderer(lineToExtend);
                        datastore.gameEvents.Publish(new TrainNetworkChangedEvent() {lineChanged = lineToExtend});
                        extendOrigin = null;
                        SpawnNewTrain(clickedStation, lineToExtend);
                    }
                }
            });
    }

    public int ConstructNewLine(List<Transporter> stationsToConnect) {
        var newLineNumber = lines.Count;
        lines.Add(stationsToConnect);

        var newLineConnector = GameObject.Instantiate(prefabs.trainLineConnector).GetComponent<LineRenderer>();
        lineRenderers.Add(newLineConnector);

        var possibleColors = ColorUtils.solColors.Select(i => i.Value).Except(lineColors);
        lineColors.Add(possibleColors.getRandomElement());

        UpdateLineRenderer(newLineNumber);
        return newLineNumber;
    }

    public void UpdateLineRenderer(int lineNumber) {
        var allStations = lines[lineNumber];
        var numStations = allStations.Count;
        var line = lineRenderers[lineNumber];
        line.positionCount = allStations.Count;
        line.startColor = lineColors[lineNumber];
        line.endColor = lineColors[lineNumber];
        line.SetPositions(
            allStations.Select(i => i.transform.position).ToArray()
        );
    }

    public void RefreshLineConnections(int lineNumber) {
        var stationsToConnect = lines[lineNumber];
        for (var i = 0; i < stationsToConnect.Count - 1; i++) {
            stationsToConnect[i].nextStation = stationsToConnect[i+1];
            stationsToConnect[i+1].prevStation = stationsToConnect[i];
        }
    }

    public Train SpawnNewTrain(Transporter station, int lineNum) {
        var train = GameObject.Instantiate(prefabs.train, station.transform.position, Quaternion.identity).GetComponent<Train>();
        train.currentNode = station.lot.trainStationNode;
        train.lineNum = lineNum;
        return train;
    }
}

public enum TrainLineToolType {
    EXTEND,
    NEW
}