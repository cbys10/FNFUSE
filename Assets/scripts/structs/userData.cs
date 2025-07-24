using System;
using System.Collections.Generic;

[Serializable]
public class GameSettings
{
    public bool upscroll;
    public string left;
    public string down;
    public string up;
    public string right;
}

[Serializable]
public class Songscores
{
    public int n;
    public string sn;
    public string d;
}

[Serializable]
public class GameData
{
    public GameSettings settings;
    public List<Songscores> scores;
}

