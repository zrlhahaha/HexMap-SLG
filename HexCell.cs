using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour {

    private int elevationLevel;
    public Color color;
    
    public int Elevation
    {
        get
        {
            return elevationLevel;
        }
        set
        {
            elevationLevel = value;
            Vector3 pos = transform.localPosition;
            pos.y = HexMetrics.GetElevationHeight(elevationLevel);
            transform.localPosition = HexMetrics.DisturbY(pos);
        }
    }

    public Vector3 Position { get { return transform.localPosition; } }

    public HexCoord coord { get; private set; }
    [SerializeField]
    private HexCell[] neigbors;

    private void Awake()
    {
        neigbors = new HexCell[6];
        Elevation = Random.Range(0, 4);
    }

    public void SetCoord(HexCoord hexcoord)
    {
        coord = hexcoord;
    }

    public HexCell GetNeigbors(Direction dir)
    {
        return neigbors[(int)dir];
    }

    public void SetNeigbors(Direction dir,HexCell hexcell)
    {
        neigbors[(int)dir] = hexcell;
        hexcell.neigbors[(int)dir.Inv()] = this;
    }



}
