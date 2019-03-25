using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexMetrics{

    public const float OutterRadius = 10f;
    public const float InnerRadius = OutterRadius * 0.86602540f;
    public const float SolidFactor = 0.75f;
    public const float BridgeFactor = 1 - SolidFactor;

    public const float HeightPerElevationLevel = 4f;
    public const float TerraceAmount = 2f;
    public const float StepAmount = 2 * TerraceAmount + 1;
    public const float StepSize_H = 1 / StepAmount;
    public const float StepSize_V = 1 / (TerraceAmount + 1);

    public const float NoiseDisturbStrength = 2f;
    public const float NoiseSampleScale = 0.003f;
    public static Texture2D noise;

    public const int ChunkSize_X = 5;
    public const int ChunkSize_Z = 5;


    public static Vector3[] corners =
    {
        new Vector3(0,0,OutterRadius),
        new Vector3(InnerRadius,0,0.5f * OutterRadius),
        new Vector3(InnerRadius,0,-0.5f * OutterRadius),
        new Vector3(0,0,-OutterRadius),
        new Vector3(-InnerRadius,0,-0.5f * OutterRadius),
        new Vector3(-InnerRadius,0,0.5f * OutterRadius)
    };



    public static Vector3 StepLerp(Vector3 start, Vector3 end,int step)
    {
        if (step > StepAmount)
            Debug.LogWarning("step is more than ElevationStep");

        Vector3 res = new Vector3();
        float t_H = step * StepSize_H;
        float t_V = ((step + 1) / 2) * StepSize_V;

        res.x = Mathf.Lerp(start.x, end.x,t_H);
        res.z = Mathf.Lerp(start.z, end.z, t_H);
        res.y = Mathf.Lerp(start.y, end.y, t_V);

        return res;
    }

    public static Vector3 SolidCorner(Direction dir)
    {
        return corners[(int)dir] * SolidFactor;
    }

    public static Vector3 SolidCorner(int index)
    {
        return corners[index] * SolidFactor;
    }

    public static Vector3 GetNextSolidCorner(Direction dir)
    {
        return corners[(int)(dir + 1) % 6] * SolidFactor;
    }

    public static Vector3 GetNextSolidCorner(int dir)
    {
        return corners[(dir + 1) % 6] * SolidFactor;
    }

    public static Vector3 HexCorner(Direction dir)
    {
        return corners[(int)dir];
    }

    public static Vector3 HexCorner(int index)
    {
        return corners[index];
    }

    public static Vector3 GetNextHexCorner(Direction dir)
    {
        return corners[(int)(dir + 1)%6];
    }

    public static Vector3 GetNextHexCorner(int dir)
    {
        return corners[(dir + 1) % 6];
    }

    public static float GetElevationHeight(int elevationLevel)
    {
        return elevationLevel * HeightPerElevationLevel;
    }

    public static Vector3 GetBridge(HexCell cell, Direction dir)
    {
        HexCell neigbor = cell.GetNeigbors(dir);

        if (neigbor == null)
            Debug.LogError("neigbor is not exist");

        Vector3 bridge = (HexCorner(dir) + GetNextHexCorner(dir)) * BridgeFactor;
        bridge.y = neigbor.Position.y - cell.Position.y;

        return bridge; 
    }

    public static BridgeType GetBridgeType(int elevationLevel_1,int elevationLevel_2)
    {
        int delta = Mathf.Abs(elevationLevel_1 - elevationLevel_2);
        if (delta > 1)
            return BridgeType.Cliff;

        return (BridgeType)delta;
    }

    public static Vector3 NoiseVector(Vector3 pos)
    {
        Vector4 col = noise.GetPixelBilinear(pos.x * NoiseSampleScale, pos.z * NoiseSampleScale);
        return (Vector3)col * 2 - Vector3.one;
    }

    public static Vector3 Disturb(Vector3 vec)
    {
        vec += NoiseVector(vec) * NoiseDisturbStrength;
        return vec;
    }

    public static Vector3 DisturbXZ(Vector3 vec)
    {
        Vector3 noiseVec = NoiseVector(vec) * NoiseDisturbStrength;
        vec.x += noiseVec.x;
        vec.z += noiseVec.z;
        return vec;
    }

    public static Vector3 DisturbY(Vector3 vec)
    {
        Vector3 noiseVec = NoiseVector(vec) * NoiseDisturbStrength;
        vec.y += noiseVec.y;
        return vec;
    }


}
