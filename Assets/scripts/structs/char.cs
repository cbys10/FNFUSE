using System;
using System.Collections.Generic;

[Serializable]

public struct scale{
    public float x;
    public float y;
    public float z;
}

[Serializable]

public struct newPos{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public struct Character
{
    public scale scale;
    public newPos newPos;
    public int opFlip;
}
