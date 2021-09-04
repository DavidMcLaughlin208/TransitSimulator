using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using System;
using System.Linq;
using System.Collections.Generic;

public class MouseAndKeyboard : MonoBehaviour {

    Datastore datastore;

    IObservable<long> clickStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
    IObservable<long> mouseUpStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonUp(0));
    IObservable<long> mouseMoveStream = Observable.EveryUpdate();
    public bool mouseOverCanvasElement = false;
    public Vector3 prevMousePosition;
    Vector3Int hoveredCoord;
    List<KeyCode> pressedKeys;
    List<KeyCode> watchedKeys = new List<KeyCode>() {
        KeyCode.R, KeyCode.E, KeyCode.C, KeyCode.T, KeyCode.B,
    };

    public void Start() {
        prevMousePosition = Input.mousePosition;
        datastore = this.GetComponent<Datastore>();
        clickStream
            .Where(_ => !mouseOverCanvasElement)
            .Subscribe(_ => {
                datastore.inputEvents.Publish(
                    new ClickEvent() {
                        cell = GetMouseCellPosition(),
                    }
                );
            });

        mouseUpStream.Subscribe(_ =>
        {
            datastore.inputEvents.Publish(
                new MouseUpEvent() {
                    mouseLocation = Input.mousePosition
                }
            );
        });

        Observable.EveryUpdate()
            .Where(_ => !mouseOverCanvasElement)
            .Where(_ => {
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
            .Where(_ => prevMousePosition != Input.mousePosition)
            .Subscribe(_ => {
                mouseOverCanvasElement = GetMouseCanvasCollision().Count > 0;
                this.prevMousePosition = Input.mousePosition;
                datastore.inputEvents.Publish(new MouseMoveEvent());
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

    // I copied this from stackoverflow, I don't know what it does, but it works, don't @ me
    List<RaycastResult> GetMouseCanvasCollision() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results;
    }
}