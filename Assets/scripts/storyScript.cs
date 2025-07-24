using System.IO;
using UnityEngine;
using System;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StoryScript : MonoBehaviour
{
    public TextMeshPro titleText;
    public TextRenderer textRender;
    private string currentlySelected = "";
    private int indexFile;
    public GameObject weeksObj;
    private GameObject[] weeks;
    private int currentMode = 0;
    public Animator leftArrow;
    public Animator rightArrow;
    public GameObject diff;
    private int weekCount = 0;
    private int currentlySelectedInt = 0;
    private List<string> weekFileNames = new List<string>();
    private List<string> weekTitles = new List<string>();
    public GameObject selectAud;
    public GameObject songsObj;
    public GameObject bf;
    public GameObject gf;
    public GameObject op;
    private List<string> currentSongs = new List<string>();
    void Start()
    {

        List<GameObject> weeksList = new List<GameObject>();
        string dirPath = Path.Combine(Application.streamingAssetsPath, "weeks");

        if (Directory.Exists(dirPath))
        {
            string[] files = Directory.GetFiles(dirPath);

            foreach (string filePath in files)
            {
                if (filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    string json = File.ReadAllText(filePath);
                    WeekData data = JsonUtility.FromJson<WeekData>(json);

                    Sprite mySprite = Resources.Load<Sprite>($"sprites/storymode/titles/{Path.GetFileNameWithoutExtension(data.story.titleImg)}");

                    GameObject obj = new GameObject(data.title == "LEARNING TIME" ? "week0" : $"week{indexFile}");
                    SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                    sr.sprite = mySprite;
                    sr.color = Color.white;
                    if (data.title == "LEARNING TIME")
                    {
                        currentlySelected = Path.GetFileName(filePath);
                    }
                    float gap = 0.1f;
                    int row = indexFile;

                    obj.transform.SetParent(weeksObj.transform);
                    obj.transform.localPosition = new Vector3(0, -row * gap, 0);
                    weeksList.Add(obj);
                    weekFileNames.Add(Path.GetFileName(filePath));
                    weekTitles.Add(data.title);
                    indexFile++;
                    weekCount++;
                    currentlySelectedInt = 0;
                }
            }

            if (weekFileNames.Count > 0)
            {
                currentlySelected = weekFileNames[0];
            }

            weeksObj.transform.localPosition = new Vector3(weeksObj.transform.localPosition.x, -0.193f, weeksObj.transform.localPosition.z);
            weeksObj.transform.localScale = new Vector3(1.659175f, 1.659175f, 1.659175f);
            weeks = weeksList.ToArray();
        }
        else
        {
            Debug.LogWarning("Directory does not exist: " + dirPath);
        }
    }

    WeekData ReadJsonFileForWeek(string FileName)
    {
        foreach (string name in weekFileNames)
        {
            if (FileName == name)
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, "weeks", name);
                string json = File.ReadAllText(filePath);
                WeekData data = JsonUtility.FromJson<WeekData>(json);
                return data;
            }
        }

        return new WeekData();
    }
    void Update()
    {
        titleText.sortingOrder = 10;
        for (int i = 0; i < weeks.Length; i++)
        {
            SpriteRenderer spr = weeks[i].GetComponent<SpriteRenderer>();
            if (weekFileNames[i] == currentlySelected)
            {
                WeekData week = ReadJsonFileForWeek(weekFileNames[i]);
                titleText.text = weekTitles[i].ToUpper();
                spr.color = Color.gray;
                weeks[i].transform.localPosition = Vector3.Lerp(weeks[i].transform.localPosition, new Vector3(0, 0, 0), 0.1f); // Move to the center
                for (int j = 0; j < weeks.Length; j++)
                {
                    if (j != i)
                    {
                        weeks[j].transform.localPosition = Vector3.Lerp(weeks[j].transform.localPosition, new Vector3(0, -(j - currentlySelectedInt) * 0.1f, 0), 0.1f);
                    }
                }
                foreach (Transform child in songsObj.transform)
                {
                    Destroy(child.gameObject);
                }
                if (week.story.gf == "none")
                {
                    gf.SetActive(false);
                }
                else
                {
                    gf.SetActive(true);
                    Animator gfAnimate = gf.GetComponent<Animator>();
                    RuntimeAnimatorController gfController = Resources.Load<RuntimeAnimatorController>($"animations/story/{week.story.gf}");
                    gfAnimate.runtimeAnimatorController = gfController;
                    gfAnimate.Play($"{week.story.gf}idle");
                }

                if (week.story.bf == "none")
                {
                    bf.SetActive(false);
                }
                else
                {
                    bf.SetActive(true);
                    Animator bfAnimate = bf.GetComponent<Animator>();
                    RuntimeAnimatorController bfController = Resources.Load<RuntimeAnimatorController>($"animations/story/{week.story.bf}");
                    bfAnimate.runtimeAnimatorController = bfController;
                    bfAnimate.Play($"{week.story.bf}idle");
                }

                if (week.story.op == "none")
                {
                    op.SetActive(false);
                }
                else
                {
                    op.SetActive(true);
                    Animator opAnimate = op.GetComponent<Animator>();
                    RuntimeAnimatorController opController = Resources.Load<RuntimeAnimatorController>($"animations/story/{week.story.op}");
                    opAnimate.runtimeAnimatorController = opController;
                    opAnimate.Play($"{week.story.op}idle");
                }

                for (int s = 0; s < week.songs.Length; s++)
                {
                    string song = week.songs[s];
                    GameObject obj = new GameObject(song);
                    TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
                    tmp.text = song.ToUpper();
                    tmp.fontSize = 3.2f;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = Color.red;

                    float gap = 0.05f;
                    int row = s;

                    obj.transform.SetParent(songsObj.transform);
                    obj.transform.localPosition = new Vector3(0, -row * gap, 0);
                }
            }
            else
            {
                spr.color = Color.white;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            leftArrow.Play("lArrowSwitch");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            rightArrow.Play("rArrowSwitch");
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            selectAud.SetActive(false);
            selectAud.SetActive(true);
            leftArrow.Play("lArrow");
            currentMode = currentMode == 0 ? 2 : currentMode - 1;
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            selectAud.SetActive(false);
            selectAud.SetActive(true);
            rightArrow.Play("rArrow");
            currentMode = currentMode == 2 ? 0 : currentMode + 1;
        }

        SpriteRenderer diffSpr = diff.GetComponent<SpriteRenderer>();
        if (currentMode == 0)
        {
            diffSpr.sprite = Resources.Load<Sprite>("sprites/storymode/ui/easy");
            diff.transform.localScale = new Vector3(0.07863173f, 0.2987176f, 2.086899f);
        }
        else if (currentMode == 1)
        {
            diffSpr.sprite = Resources.Load<Sprite>("sprites/storymode/ui/normal");
            diff.transform.localScale = new Vector3(0.0559276f, 0.2124659f, 1.484328f);
        }
        else if (currentMode == 2)
        {
            diffSpr.sprite = Resources.Load<Sprite>("sprites/storymode/ui/hard");
            diff.transform.localScale = new Vector3(0.07863173f, 0.2987176f, 2.086899f);
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            selectAud.SetActive(false);
            selectAud.SetActive(true);
            currentlySelectedInt++;
            if (currentlySelectedInt >= weekCount)
                currentlySelectedInt = 0;

            currentlySelected = weekFileNames[currentlySelectedInt];
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            selectAud.SetActive(false);
            selectAud.SetActive(true);
            currentlySelectedInt--;
            if (currentlySelectedInt < 0)
                currentlySelectedInt = weekCount - 1;

            currentlySelected = weekFileNames[currentlySelectedInt];
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SceneManager.LoadScene("mainMenu");
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            WeekData week = ReadJsonFileForWeek(currentlySelected);
            currentSongs.Clear();
            for (int s = 0; s < week.songs.Length; s++)
            {
                currentSongs.Add(week.songs[s]);
            }

            currentWeekSong globalD = new currentWeekSong
            {
                diff = currentMode == 0 ? "easy" : currentMode == 1 ? "normal" : "hard",
                songs = currentSongs,
                gf = week.story.gf,
                bf = week.story.bf,
                op = week.story.op
            };

            GlobalData.currentSong = globalD;
            SceneManager.LoadScene("game");
        }
    }

}
