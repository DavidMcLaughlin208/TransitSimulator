﻿using System.Collections.Generic;
using UnityEngine;

public class RoadNode : Node
{
    public RoadNodeLocation location;
    public bool disabled = false;
    public List<Car> cars = new List<Car>();

    public List<Car> getCarsAfterCar(Car car)
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
