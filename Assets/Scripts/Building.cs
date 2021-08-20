using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Lot parentLot;

    public void ReceivePedestrian(Pedestrian pedestrian) {
        var destinationComp = this.GetComponent<Destination>();
        var generatorComp = this.GetComponent<Generator>();

        // TODO this is gross - but do we have some weird summing function for pedestrian reception across all components?
        // gah
        if (destinationComp != null) {
            destinationComp.ReceivePedestrian(pedestrian);
        }
        if (generatorComp != null) {
            generatorComp.ReceivePedestrian(pedestrian);
        }
        pedestrian.transform.position = parentLot.exitNode.transform.position;
        pedestrian.currentNode = parentLot.exitNode;
        pedestrian.CalculateItinerary();
    }
}
