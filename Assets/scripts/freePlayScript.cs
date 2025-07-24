using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using System.Text;
using System.Linq;
using System;

public class freePlayScript : MonoBehaviour
{
    public SpriteRenderer menuBg;
    public TextRenderer textRenderer;
    public GameObject songs;
    private int totalSongs = 0;
    private int currentIndex = 0;
    private float itemGap = 1.2f;
    private float lerpSpeed = 10f;
    public GameObject selectAud;
    public TextMeshPro diffText;
    public TextMeshPro scoreText;
    private int currentDiff = 0;
    private GameData userData;
    private List<GameObject> songObjects = new List<GameObject>();
    private List<float> targetYPositions = new List<float>();

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "data.txt");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            byte[] bytes = Convert.FromBase64String(json);
            string decodedData = Encoding.UTF8.GetString(bytes);
            userData = JsonUtility.FromJson<GameData>(decodedData);
        }
        else
        {
            print("User Data file not found");
        }
        float randomR = UnityEngine.Random.Range(0, 256) / 255f;
        float randomG = UnityEngine.Random.Range(0, 256) / 255f;
        float randomB = UnityEngine.Random.Range(0, 256) / 255f;
        menuBg.color = new Color(randomR, randomG, randomB, 1f);

        string dirPath = Path.Combine(Application.streamingAssetsPath, "songs");

        if (Directory.Exists(dirPath))
        {
            string[] directories = Directory.GetDirectories(dirPath);
            int songIndex = 0;

            foreach (string dir in directories)
            {
                string[] files = Directory.GetFiles(dir);
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName.EndsWith(".json") && fileName.StartsWith("hard"))
                    {
                        string json = File.ReadAllText(file);
                        SongMetaData song = JsonUtility.FromJson<SongMetaData>(json);
                        if (song != null)
                        {

                            GameObject sObj = new GameObject(song.song);
                            sObj.transform.SetParent(songs.transform);
                            sObj.transform.localScale = Vector3.one * 0.8f;

                            GameObject name = new GameObject("name");
                            name.transform.SetParent(sObj.transform);
                            name.transform.localPosition = Vector3.zero;

                            TextMeshPro nameText = name.AddComponent<TextMeshPro>();
                            nameText.text = textRenderer.GetStringIS(song.song, "bold", 0, "#FFFFFFFF") + $"  <sprite name=\"{(song.op != null ? song.op : "face")}\">"
;
                            nameText.fontSize = 10f;
                            nameText.alignment = TextAlignmentOptions.Center;
                            songObjects.Add(sObj);
                            targetYPositions.Add(0f);
                            songIndex++;
                        }
                    }
                }
            }

            totalSongs = songObjects.Count;
            UpdateVisibleSongs();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectAud.SetActive(false);
            selectAud.SetActive(true);
            float randomR = UnityEngine.Random.Range(0, 256) / 255f;
            float randomG = UnityEngine.Random.Range(0, 256) / 255f;
            float randomB = UnityEngine.Random.Range(0, 256) / 255f;
            menuBg.color = new Color(randomR, randomG, randomB, 1f);
            if (currentIndex < totalSongs - 1)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
            }
            UpdateVisibleSongs();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GameObject obj = songObjects[currentIndex];
            string name = obj.name;
            bool hardFound = false;
            bool normFound = false;
            bool easyFound = false;
            string dirPath = Path.Combine(Application.streamingAssetsPath, "songs", name);
            if (Directory.Exists(dirPath))
            {
                foreach (string file in Directory.GetFiles(dirPath))
                {
                    if (Path.GetFileName(file) == "hard.json")
                    {
                        hardFound = true;
                    }
                    if (Path.GetFileName(file) == "normal.json")
                    {
                        normFound = true;
                    }
                    if (Path.GetFileName(file) == "easy.json")
                    {
                        easyFound = true;
                    }
                }
            }
            if (hardFound && !easyFound && !normFound)
            {
                currentDiff = 1;
            }
            if (hardFound && easyFound && normFound)
            {
                currentDiff = currentDiff == 2 ? 0 : currentDiff + 1;
            }
            if (!hardFound && easyFound && !normFound)
            {
                currentDiff = 0;
            }
            if (!hardFound && !easyFound && normFound)
            {
                currentDiff = 2;
            }
            if (hardFound && !easyFound && normFound)
            {
                currentDiff = currentDiff == 2 ? 1 : currentDiff = 2;
            }
            if (!hardFound && easyFound && normFound)
            {
                currentDiff = currentDiff == 2 ? 0 : currentDiff = 2;
            }
            if (hardFound && easyFound && !normFound)
            {
                currentDiff = currentDiff == 1 ? 0 : currentDiff = 1;
            }
        }
        GameObject objSong = songObjects[currentIndex];
        string songName = objSong.name;
        bool scoreFound = false;
        foreach (Songscores score in userData.scores)
        {
            string currentDiffStr = currentDiff == 0 ? "easy" : currentDiff == 2 ? "normal" : currentDiff == 1 ? "hard" : "err";
            if (score.sn == songName && score.d == currentDiffStr)
            {
                scoreFound = true;
                scoreText.text = $"PERSONAL BEST: {score.n}";
            }
        }
        if (!scoreFound)
        {
            scoreText.text = $"PERSONAL BEST: 0";
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            GameObject obj = songObjects[currentIndex];
            string name = obj.name;
            bool hardFound = false;
            bool normFound = false;
            bool easyFound = false;
            string dirPath = Path.Combine(Application.streamingAssetsPath, "songs", name);
            if (Directory.Exists(dirPath))
            {
                foreach (string file in Directory.GetFiles(dirPath))
                {
                    if (Path.GetFileName(file) == "hard.json")
                    {
                        hardFound = true;
                    }
                    if (Path.GetFileName(file) == "normal.json")
                    {
                        normFound = true;
                    }
                    if (Path.GetFileName(file) == "easy.json")
                    {
                        easyFound = true;
                    }
                }
            }
            if (hardFound && !easyFound && !normFound)
            {
                currentDiff = 1;
            }
            if (hardFound && easyFound && normFound)
            {
                currentDiff = currentDiff == 0 ? 2 : currentDiff - 1;
            }
            if (!hardFound && easyFound && !normFound)
            {
                currentDiff = 0;
            }
            if (!hardFound && !easyFound && normFound)
            {
                currentDiff = 2;
            }
            if (hardFound && !easyFound && normFound)
            {
                currentDiff = currentDiff == 2 ? 1 : currentDiff = 2;
            }
            if (!hardFound && easyFound && normFound)
            {
                currentDiff = currentDiff == 2 ? 0 : currentDiff = 2;
            }
            if (hardFound && easyFound && !normFound)
            {
                currentDiff = currentDiff == 1 ? 0 : currentDiff = 1;
            }
        }
        if (currentDiff == 0)
        {
            string newString = "<EASY>";
            diffText.text = newString;
        }
        if (currentDiff == 2)
        {
            string newString = "<NORMAL>";
            diffText.text = newString;
        }
        if (currentDiff == 1)
        {
            string newString = "<HARD>";
            diffText.text = newString;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectAud.SetActive(false);
            selectAud.SetActive(true);
            float randomR = UnityEngine.Random.Range(0, 256) / 255f;
            float randomG = UnityEngine.Random.Range(0, 256) / 255f;
            float randomB = UnityEngine.Random.Range(0, 256) / 255f;
            menuBg.color = new Color(randomR, randomG, randomB, 1f);
            if (currentIndex > 0)
            {
                currentIndex--;
            }
            else
            {
                currentIndex = totalSongs - 1;
            }
            UpdateVisibleSongs();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameObject obj = songObjects[currentIndex];
            string name = obj.name;
            List<string> sList = new List<string>();
            sList.Add(name);
            GlobalData.currentSong.songs = sList;
            if (currentDiff == 0)
            {
                GlobalData.currentSong.diff = "easy";
            }
            if (currentDiff == 2)
            {
                GlobalData.currentSong.diff = "normal";
            }
            if (currentDiff == 1)
            {
                GlobalData.currentSong.diff = "hard";
            }
            SceneManager.LoadScene("game");
        }
        for (int i = 0; i < songObjects.Count; i++)
        {
            GameObject obj = songObjects[i];
            Transform nameTRFM = obj.transform.Find("name");
            TextMeshPro tmp = nameTRFM?.GetComponent<TextMeshPro>();

            if (tmp != null)
            {
                Color color = tmp.color;
                color.a = (i == currentIndex) ? 1f : 0.5f;
                tmp.color = color;
            }

            int relativeIndex = i - currentIndex;
            if (relativeIndex >= 0 && relativeIndex < 3)
            {
                obj.SetActive(true);
                float targetY = -relativeIndex * itemGap;
                obj.transform.localPosition = Vector3.Lerp(obj.transform.localPosition, new Vector3(0, targetY, 0), Time.deltaTime * lerpSpeed);
            }
            else
            {
                obj.SetActive(false);
            }
            GameObject sObj = songObjects[currentIndex];
            string name = sObj.name;
            bool hardFound = false;
            bool normFound = false;
            bool easyFound = false;
            string dirPath = Path.Combine(Application.streamingAssetsPath, "songs", name);
            if (Directory.Exists(dirPath))
            {
                foreach (string file in Directory.GetFiles(dirPath))
                {
                    if (Path.GetFileName(file) == "hard.json")
                    {
                        hardFound = true;
                    }
                    if (Path.GetFileName(file) == "normal.json")
                    {
                        normFound = true;
                    }
                    if (Path.GetFileName(file) == "easy.json")
                    {
                        easyFound = true;
                    }
                }
            }
            if (hardFound && !easyFound && !normFound)
            {
                currentDiff = 1;
            }
            if (!hardFound && easyFound && !normFound)
            {
                currentDiff = 0;
            }
            if (!hardFound && !easyFound && normFound)
            {
                currentDiff = 2;
            }
        }
    }


    void UpdateVisibleSongs()
    {
        for (int i = 0; i < songObjects.Count; i++)
        {
            if (i >= currentIndex && i < currentIndex + 3)
            {
                targetYPositions[i] = -(i - currentIndex) * itemGap;
            }
        }
    }
}
