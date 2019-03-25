using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HexCoord {

    //所有坐标是只读的，创建后就不能改变
    public int x { get; private set; }
    public int z { get; private set; }
    public int y { get { return -x - z; } }

    public HexCoord()
    {

    }

    public HexCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public static HexCoord FromOffsetCoordinates(int x,int z)
    {
        int _x = x - z / 2;
        int _z = z;
        return new HexCoord(_x, _z);
    }

    public Vector2Int ToOffsetCoordinates()
    {
        return new Vector2Int(x + z / 2,z);
    }

    //public static HexCoord FromWorldPos(Vector3 pos_world)
    //{
    //    float offset = pos_world.z /( 3 * HexMesh.OutterRadius);
    //    float x = pos_world.x / (2 * HexMesh.InnerRadius);
    //    float y = -x;
    //    x -= offset;
    //    y -= offset;

    //    int _x = Mathf.RoundToInt(x);
    //    int _y = Mathf.RoundToInt(y);
    //    int _z = Mathf.RoundToInt(-x-y);

    //    Debug.Log(_x.ToString() + "_"   + _y.ToString()+ "_"+ _z.ToString());
    //    var go = new HexCoord(_x, _z);
    //    Debug.Log(go.ToString());
    //    return new HexCoord();
    //}

    public static implicit operator Vector3(HexCoord coord)
    {
        return new Vector3(coord.x, coord.y, coord.z);
    }

    public static HexCoord FromLocalPos(Vector3 pos)
    {

        float dist_z = pos.z / Mathf.Sin(60 * Mathf.Deg2Rad);
        float z = dist_z / (2 * HexMetrics.InnerRadius);
        int _z = Mathf.RoundToInt(z);

        float x = pos.x / (2 * HexMetrics.InnerRadius);
        float offset = _z * 0.5f;
        int _x = Mathf.RoundToInt(x - offset);

        HexCoord go = new HexCoord(_x, _z);
        return go;
    }


    public override string ToString()
    {
        return x.ToString() + "," + y.ToString() + "," + z.ToString();
    }
}
