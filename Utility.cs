using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    NE = 0,
    E = 1,
    SE = 2,
    SW = 3,
    W = 4,
    NW = 5,
}

public enum BridgeType
{
    Flat = 0,
    Slope = 1,
    Cliff = 2
}

public static class ExtensionMethoud
{

    public static Direction Inv(this Direction direction)
    {
        return (Direction)(((int)direction + 3) % 6);
    }

    public static Direction Previous(this Direction direction)
    {
        return(Direction)((int)direction - 1).Loop(6);
    }

    public static Direction Next(this Direction direction)
    {
        return (Direction)(((int)direction + 1)%6);
    }

    public static int Loop(this int go, int max)
    {
        int t = go % max;
        if (t < 0)
            t += max;

        return t;
    }


    public static Vector3 Disturb(this Vector3 vec)
    {
        return HexMetrics.Disturb(vec);
    }

    public static Vector3 DisturbXZ(this Vector3 vec)
    {
        return HexMetrics.DisturbXZ(vec);
    }

    public static Vector3 DisturbY(this Vector3 vec)
    {
        return HexMetrics.DisturbY(vec);
    }

}

