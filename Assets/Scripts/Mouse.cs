using UnityEngine;
using UniRx;
using System;
using System.Linq;

public class Mouse : MonoBehaviour {

    IObservable<long> clickStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
    Vector3Int hoveredCoord;
    Datastore datastore;

    public void Start() {
        datastore = this.GetComponent<Datastore>();
        clickStream.Subscribe(_ => {
            datastore.inputEvents.Publish(
                new ClickEvent() {
                    cell = GetMouseCellPosition(),
                }
            );
        });

        Observable.EveryUpdate().Where(_ => {
            if (hoveredCoord != GetMouseCellPosition()) {
                hoveredCoord = GetMouseCellPosition();
                return true;
            } else {
                return false;
            }
        }).Subscribe(_ => {
            datastore.inputEvents.Publish(
                new HoverEvent() {
                    cell = hoveredCoord,
                }
            );
        });
    }

    Vector3Int GetMouseCellPosition() {
        var cellPoint = datastore.validTiles.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        return new Vector3Int(cellPoint.x, cellPoint.y, 0);
    }
}