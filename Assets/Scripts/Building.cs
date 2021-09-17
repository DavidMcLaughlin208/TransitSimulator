using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Lot parentLot;

    PedestrianDestination pedDest;
    Generator generator;
    Residence residence;
    Transporter transporter;

    public void Start() {
        pedDest = this.GetComponent<PedestrianDestination>();
        generator = this.GetComponent<Generator>();
        residence = this.GetComponent<Residence>();
        transporter  = this.GetComponent<Transporter>();
    }

    public void ReceivePedestrian(Pedestrian pedestrian) {
        // TODO this is gross - but do we have some weird summing function for pedestrian reception across all components?
        // gah
        if (pedDest != null) {
            pedDest.ReceivePedestrian(pedestrian);
        }
        if (generator != null) {
            generator.ReceivePedestrian(pedestrian);
        }
        if (residence != null) {
            residence.ReceivePedestrian(pedestrian);
        }
        if (transporter != null) {
            transporter.ReceivePedestrian(pedestrian);
        }
    }

    public void ReleasePedestrian(Pedestrian pedestrian) {
        if (transporter != null) {
            transporter.ReleasePedestrian(pedestrian);
        }
    }
}
