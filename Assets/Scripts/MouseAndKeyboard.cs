using System.Collections.ObjectModel;
using UnityEngine;
using UniRx;
using System;
using System.Linq;
using System.Collections.Generic;

public class MouseAndKeyboard : MonoBehaviour {

    Datastore datastore;

    IObservable<long> clickStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
    Vector3Int hoveredCoord;
    List<KeyCode> pressedKeys;
    List<KeyCode> watchedKeys = new List<KeyCode>() {
        KeyCode.R, KeyCode.E,
    };

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

        Observable.EveryUpdate()
            .Where(_ => {
                var checkedKeys = watchedKeys.Where(keyCode => Input.GetKeyDown(keyCode)).ToList();
                if (checkedKeys.Count() > 0) {
                    pressedKeys = checkedKeys;
                    return true;
                } else {
                    return false;
                }
            })
            .Subscribe(_ => {
                pressedKeys.ForEach(keyCode => {
                    datastore.inputEvents.Publish(
                        new KeyEvent() {
                            cell = hoveredCoord,
                            keyCode = keyCode,
                        });
                });
            });
    }

    Vector3Int GetMouseCellPosition() {
        var cellPoint = datastore.validTiles.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        return new Vector3Int(cellPoint.x, cellPoint.y, 0);
    }
}