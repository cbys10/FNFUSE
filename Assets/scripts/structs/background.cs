using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Vec3
{
    public float x;
    public float y;
    public float z;
}


[System.Serializable]
public struct ThingyData
{
    public string thingyName;
    public Vec3 pos;
    public Vec3 scale;
    public int layer;
    public bool isPrefab;
}

[System.Serializable]
public struct CharVisualData
{
    public Vec3 pos;
    public int layer;
}

[System.Serializable]
public struct CharsData
{
    public CharVisualData bf;
    public CharVisualData gf;
    public CharVisualData op;
}

[System.Serializable]
public struct BackgroundData
{
    public List<ThingyData> thingys;
    public CharsData chars;
}
