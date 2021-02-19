using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ElevationDTO
{
    //public float[] shapePoints;
    public ElevationProfile[] elevationProfile;
}

[System.Serializable]
public class ElevationProfile
{
    public float distance;
    public float height;
}
