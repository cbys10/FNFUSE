using System;
using System.Collections.Generic;

[Serializable]
public class Song
{
    public string song;
    public int bpm;
    public string bf;
    public string gf;
    public string op;
    public float speedMultiplier;
    public string background;
    public List<NoteData> bfNotes;
    public List<NoteData> opNotes;
    public List<DialogData> dialog;
    public string openingcut;
}

[Serializable]
public struct NoteData
{
    public string note;
    public float time;
    public float hold;
    public string isAlt;
}

[Serializable]
public struct DialogData
{
 public string speaker;
 public string box;
 public string text;
}
