using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarDestination : Destination
{
    public static int initialCarCount = 5;
    private PedestrianDestination backingShop;
    public PedestrianDestination attachedShop
    {
        get { return backingShop; }
        set
        {
            if (value != null)
            {
                value.attachedParkingLot = this;
                destType = value.destType;
            }
            backingShop = value;
        }
    }
    private Residence backingResidence;
    public Residence attachedResidence {
        get { return backingResidence; }
        set
        {
            if (value != null)
            {
                value.attachedParkingLot = this;
            }
            backingResidence = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        lot.pedestrianConnectionsEnabled = false;
        lot.carConnectionsEnabled = true;

        if (attachedResidence != null)
        {
            for (int i = 0; i < initialCarCount; i++)
            {
                GameObject carObj = Object.Instantiate(prefabs.car, new Vector3(), Quaternion.identity);
                carObj.transform.position = lot.carExitNode.transform.position;
                Car car = carObj.GetComponent<Car>();
                car.homeNode = lot.carEntranceNode;
                car.desiredDestType = attachedResidence.housingType;
                ReceiveCar(car);
            }
            
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (!carServicePending && carQueue.Count > 0)
        {
            StartCoroutine(QueueForSeconds(carQueue.Dequeue(), datastore.baseQueueTime));
        }
    }

    public void ReceiveCar(Car car)
    {
        carQueue.Enqueue(car);
        car.transform.position = lot.carExitNode.transform.position;
        car.carBodySprite.enabled = false;
        car.itinerary.Clear();
        car.SetNewCurrentNode(lot.carExitNode);
        car.targetNode = null;
        car.currentCurve.currentPlace = 0;
    }

    IEnumerator QueueForSeconds(Car car, int seconds)
    {        
        carServicePending = true;
        yield return new WaitForSeconds(seconds);
        car.headingHome = !car.headingHome;
        car.carBodySprite.enabled = true;
        car.CalculateItinerary();
        carServicePending = false;
    }
}
