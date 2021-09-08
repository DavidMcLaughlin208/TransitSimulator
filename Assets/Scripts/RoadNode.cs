using System.Collections.Generic;
using UnityEngine;

public class RoadNode : Node
{
    public RoadNodeLocation location;
    public bool disabled = false;
    public List<Car> cars = new List<Car>();


    public bool IsIntersectionNode()
    {
        if (location == RoadNodeLocation.NOUT
            || location == RoadNodeLocation.EOUT
            || location == RoadNodeLocation.SOUT
            || location == RoadNodeLocation.WOUT)
        {
            return false;
        }
        else if (owningTile != null && owningTile.IsIntersection())
        {
            return true;
        }
        else
        {
            return false;
        }   
    }

    public void RemoveCarFromIntersectionQueue(Car car, List<IntersectionTile> tilesToRelease)
    {
        owningTile.RemoveCarFromIntersectionQueue(this, car, tilesToRelease);
    }

    public void PlaceCarInIntersectionQueue(Car car)
    {
        owningTile.PlaceCarInIntersectionQueue(this, car);
    }

    public List<Car> GetCarsAfterCar(Car car)
    {
        int index = cars.IndexOf(car);
        if (index <= 0)
        {
            return new List<Car>();
        } else
        {
            return cars.GetRange(0, index);
        }
    }

    public void AddCar(Car car)
    {
        cars.Add(car);
    }

    public void RemoveCar(Car car)
    {
        cars.Remove(car);
    }
}
