using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinate
{
    public float latitude { get; set; }
    public float longitude { get; set; }

    public Coordinate(float lat, float lng)
    {
        latitude = lat;
        longitude = lng;
    }
}
