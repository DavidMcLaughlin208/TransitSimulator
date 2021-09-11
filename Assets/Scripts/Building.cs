using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Lot parentLot;

    public void ReceivePedestrian(Pedestrian pedestrian) {
        var destinationComp = this.GetComponent<PedestrianDestination>();
        var generatorComp = this.GetComponent<Generator>();
        var residenceComp = this.GetComponent<Residence>();
        var transporterComp  = this.GetComponent<Transporter>();

        // TODO this is gross - but do we have some weird summing function for pedestrian reception across all components?
        // gah
        if (destinationComp != null) {
            destinationComp.ReceivePedestrian(pedestrian);
        }
        if (generatorComp != null) {
            generatorComp.ReceivePedestrian(pedestrian);
        }
        if (residenceComp != null) {
            residenceComp.ReceivePedestrian(pedestrian);
        }
    }
}
