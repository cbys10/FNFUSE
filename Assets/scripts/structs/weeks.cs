[System.Serializable]
public struct StoryCharacterData
{
    public string op;
    public string bf;
    public string gf;
    public string titleImg;
}

[System.Serializable]
public struct WeekData
{
    public string title;
    public string[] songs;
    public string[] diffs;
    public StoryCharacterData story;
}
