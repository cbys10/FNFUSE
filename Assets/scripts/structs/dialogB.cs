using UnityEngine;

[System.Serializable]
public struct dialogB
{
    public string sprite;
    public Vector3 scale;
    public Vector3 pos;
    public int layer;
    public PortData port;

    [System.Serializable]
    public struct PortData
    {
        public Vector3 pos;
        public Vector3 scale;
    }
}
