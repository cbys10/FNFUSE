using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Linq;
using TMPro;
using System.Text;
using UnityEngine.Video;
using Unity.VisualScripting;

public class gameScript : MonoBehaviour
{
    public GameObject bfNotes;
    public GameObject opNotes;
    public GameObject bf;
    public GameObject op;
    public GameObject gf;
    public GameObject bfIcon;
    public GameObject opIcon;
    public GameObject bfHealth;
    public GameObject opHealth;
    public GameObject BackgroundGOBJ;

    public AudioSource intro;
    public AudioSource bfVoice;
    public AudioSource opVoice;
    public AudioSource instSrc;
    public AudioSource missAud;

    private Song currentSongJson;
    private bool isGameReady = false;
    private float beatTime;
    private float nextBeatTime;
    public float spawnOffset = 1.2f;

    KeyCode left;
    KeyCode right;
    KeyCode up;
    KeyCode down;
    private int misses = 0;
    //private int combo = 0;
    private bool noteHittt = false;
    private bool noteHitttOP = false;
    private HashSet<Transform> frozenOpNotes = new HashSet<Transform>();
    private HashSet<Transform> frozenBfNotes = new HashSet<Transform>();
    private bool changing = false;
    private Dictionary<int, Coroutine> activeHoldNotes = new Dictionary<int, Coroutine>();
    public Animator ratingAnimate;
    private int bfHealthInt = 50;
    private int opHealthInt = 50;
    public float maxBarWidth = 9.725f;
    public float speedMultiplier = 1.75f;
    public TextMeshPro sNameOnMenu;
    public TextMeshPro sDiffOnMenu;
    public GameObject menuOptions;
    public GameObject menu;
    private bool pause = false;
    private int currentOption = 0;
    public GameObject introScreen;
    private int score = 0;
    public TextMeshPro scoreObj;
    public GameObject dialog;
    private bool currentlyPlayingDialog;
    private int currentDialog = 0;
    private bool playingCut = false;
    public GameObject cutsceneObj;
    private bool opbgpos = false;
    private bool gfbgpos = false;
    private bool bfbgpos = false;
    void Start()
    {
        /*dev line
        GlobalData.currentSong.diff = "hard";
        List<string> sList = new List<string>();
        sList.Add("Fresh");
        GlobalData.currentSong.songs = sList;*/
        string path = Path.Combine(Application.streamingAssetsPath, "data.txt");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            byte[] bytes = Convert.FromBase64String(json);
            string decodedData = Encoding.UTF8.GetString(bytes);
            GameData data = JsonUtility.FromJson<GameData>(decodedData);
            if (!Enum.TryParse(data.settings.left, out left))
            {
                Debug.LogError("Invalid key string: " + data.settings.left);
                left = KeyCode.LeftArrow;
            }
            if (!Enum.TryParse(data.settings.right, out right))
            {
                Debug.LogError("Invalid key string: " + data.settings.right);
                right = KeyCode.RightArrow;
            }
            if (!Enum.TryParse(data.settings.up, out up))
            {
                Debug.LogError("Invalid key string: " + data.settings.up);
                up = KeyCode.UpArrow;
            }
            if (!Enum.TryParse(data.settings.down, out down))
            {
                Debug.LogError("Invalid key string: " + data.settings.down);
                down = KeyCode.DownArrow;
            }
        }
        else
        {
            Debug.LogError("File not found at " + path);
        }
        if (GlobalData.currentSong.songs.Count <= 0 || GlobalData.currentSong.songs == null || GlobalData.currentSong.diff == null)
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            string currentSong = GlobalData.currentSong.songs[0];
            string dirPath = Path.Combine(Application.streamingAssetsPath, "songs", currentSong);
            if (Directory.Exists(dirPath))
            {
                foreach (string filePath in Directory.GetFiles(dirPath))
                {
                    string fileName = Path.GetFileName(filePath);
                    if (fileName.EndsWith(".json") && fileName.StartsWith(GlobalData.currentSong.diff))
                    {
                        string json = File.ReadAllText(filePath);
                        currentSongJson = JsonUtility.FromJson<Song>(json);
                        GlobalData.currentSong.bf = currentSongJson.bf;
                        GlobalData.currentSong.op = currentSongJson.op;
                        GlobalData.currentSong.gf = currentSongJson.gf;
                        speedMultiplier = currentSongJson.speedMultiplier;
                        break;
                    }
                }

            }

            if (currentSongJson == null)
            {
                Debug.LogError("Failed to load song JSON for: " + currentSong);
                SceneManager.LoadScene("mainMenu");
            }
            else
            {
                renderBackground();
                if (currentSongJson.dialog == null || currentSongJson.dialog.Count == 0)
                {
                    if (currentSongJson.openingcut == null)
                    {
                        StartCoroutine(GameSetup());
                    }
                    else
                    {
                        playingCut = true;
                        StartCoroutine(RenderCut());
                    }
                }
                else
                {
                    if (currentSongJson.openingcut != null)
                    {
                        playingCut = true;
                        StartCoroutine(RenderCut());
                    }
                    currentDialog = 0;
                    RenderDialog();
                }
            }
        }
    }
    IEnumerator RenderCut()
    {
        VideoClip video = Resources.Load<VideoClip>($"Videos/{currentSongJson.openingcut}");
        VideoPlayer vp = cutsceneObj.GetComponent<VideoPlayer>();

        cutsceneObj.SetActive(true);
        vp.clip = video;
        cutsceneObj.transform.localPosition = new Vector3(cutsceneObj.transform.localPosition.x, cutsceneObj.transform.localPosition.y, -2);
        vp.Prepare();
        while (!vp.isPrepared)
            yield return null;

        vp.Play();

        while (!vp.isPlaying)
            yield return null;

        while (vp.isPlaying)
            yield return null;

        vp.clip = null;
        cutsceneObj.SetActive(false);
        if (currentSongJson.dialog == null || currentSongJson.dialog.Count == 0)
        {
            StartCoroutine(GameSetup());
        }
        playingCut = false;
    }

    private void RenderDialog()
    {
        dialog.SetActive(true);
        DialogData dialogData = currentSongJson.dialog[currentDialog];
        dialogB dialogBoxData = new dialogB();
        string dirPath2 = Path.Combine(Application.streamingAssetsPath, "dialogB");
        if (Directory.Exists(dirPath2))
        {
            foreach (string filePath in Directory.GetFiles(dirPath2))
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName.EndsWith(".json") && fileName.StartsWith(currentSongJson.dialog[0].box))
                {
                    dialogBoxData = JsonUtility.FromJson<dialogB>(File.ReadAllText(filePath));
                    break;
                }
            }
        }
        Sprite textBox = Resources.Load<Sprite>($"sprites/game/diaboxes/{dialogData.box}");
        GameObject tbgo = new GameObject("textBox");
        tbgo.transform.parent = dialog.transform;
        SpriteRenderer tbsr = tbgo.AddComponent<SpriteRenderer>();
        tbsr.sprite = textBox;
        tbsr.sortingOrder = dialogBoxData.layer;
        tbgo.transform.localPosition = dialogBoxData.pos;
        tbgo.transform.localScale = dialogBoxData.scale;
        if (dialogData.speaker != "none")
        {
            Sprite speaker = Resources.Load<Sprite>($"sprites/game/diaboxes/{dialogData.speaker}");
            GameObject spgo = new GameObject("port");
            spgo.transform.parent = dialog.transform;
            SpriteRenderer spsr = spgo.AddComponent<SpriteRenderer>();
            spsr.sprite = speaker;
            spsr.sortingOrder = dialogBoxData.layer;
            spgo.transform.localPosition = dialogBoxData.port.pos;
            spgo.transform.localScale = dialogBoxData.port.scale;
        }
        TextMeshPro tmpText = dialog.transform.Find("text").gameObject.GetComponent<TextMeshPro>();
        tmpText.sortingOrder = 26;
        tmpText.text = dialogData.text;
        currentlyPlayingDialog = true;
    }

    public void NoteHitBF(string note, string rating, bool isAlt)
    {
        Animator bfAnimate = bf.GetComponent<Animator>();
        print(rating);
        string clipName;
        if (isAlt)
        {
            clipName = $"{GlobalData.currentSong.bf}alt{note}";
        }
        else
        {
            clipName = $"{GlobalData.currentSong.bf}{note}";

        }

        if (StateExists(bfAnimate, clipName))
        {
            bfAnimate.Play(clipName, 0, 0f);
            noteHittt = true;
            StartCoroutine(ResetToBFIdle(bfAnimate, note));
        }
        else
        {
            Debug.LogWarning($"Note animation state '{clipName}' not found!");
        }
    }

    IEnumerator ResetToBFIdle(Animator bfAnimate, string note)
    {
        yield return new WaitForSeconds(0.5f);
        noteHittt = false;
    }

    public void NoteHitOp(string note, bool isAlt)
    {
        if (GlobalData.currentSong.op == "gf")
        {
            Animator gfAnimate = gf.GetComponent<Animator>();
            string clipName;
            if (isAlt)
            {
                clipName = $"{GlobalData.currentSong.op}{note}alt";
            }
            else
            {
                clipName = $"{GlobalData.currentSong.op}{note}";

            }
            if (StateExists(gfAnimate, clipName))
            {
                gfAnimate.Play(clipName, 0, 0f);
                noteHitttOP = true;
                StartCoroutine(ResetToOPIdle(gfAnimate, note));
            }
            else
            {
                Debug.LogWarning($"Note animation state '{clipName}' not found!");
            }
        }
        else
        {
            Animator opAnimate = op.GetComponent<Animator>();
            string clipName;
            if (isAlt)
            {
                clipName = $"{GlobalData.currentSong.op}{note}alt";
            }
            else
            {
                clipName = $"{GlobalData.currentSong.op}{note}";

            }
            if (StateExists(opAnimate, clipName))
            {
                opAnimate.Play(clipName, 0, 0f);
                noteHitttOP = true;
                StartCoroutine(ResetToOPIdle(opAnimate, note));
            }
            else
            {
                Debug.LogWarning($"Note animation state '{clipName}' not found!");
            }
        }
    }

    IEnumerator ResetToOPIdle(Animator bfAnimate, string note)
    {
        yield return new WaitForSeconds(0.5f);
        noteHitttOP = false;
    }

    bool StateExists(Animator animator, string stateName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == stateName)
                return true;
        }
        return false;
    }

    void renderBackground()
    {
        Vector3 ConvertVec3(Vec3 v) => new Vector3(v.x, v.y, v.z);

        string currentSong = GlobalData.currentSong.songs[0];
        string diffPrefix = GlobalData.currentSong.diff;

        string songDir = Path.Combine(Application.streamingAssetsPath, "songs", currentSong);
        string bgDir = Path.Combine(Application.streamingAssetsPath, "bgs");

        if (!Directory.Exists(songDir))
        {
            Debug.LogError($"Song directory not found: {songDir}");
            return;
        }

        string songJsonPath = Directory.GetFiles(songDir)
            .FirstOrDefault(path => Path.GetFileName(path).StartsWith(diffPrefix) && path.EndsWith(".json"));

        if (string.IsNullOrEmpty(songJsonPath))
        {
            Debug.LogError($"No song JSON found in {songDir} starting with '{diffPrefix}'");
            return;
        }

        Song song = JsonUtility.FromJson<Song>(File.ReadAllText(songJsonPath));
        print($"Loaded song JSON: {songJsonPath}, background: {song.background}");

        if (!Directory.Exists(bgDir))
        {
            Debug.LogError($"Background directory not found: {bgDir}");
            return;
        }

        string bgJsonPath = Directory.GetFiles(bgDir)
            .FirstOrDefault(path => Path.GetFileName(path).StartsWith(song.background, StringComparison.OrdinalIgnoreCase) && path.EndsWith(".json"));

        if (string.IsNullOrEmpty(bgJsonPath))
        {
            Debug.LogError($"No background JSON found in {bgDir} starting with '{song.background}'");
            return;
        }

        BackgroundData bg = JsonUtility.FromJson<BackgroundData>(File.ReadAllText(bgJsonPath));
        print($"Loaded background JSON: {bgJsonPath}, thingys count: {bg.thingys.Count}");

        foreach (ThingyData thingy in bg.thingys)
        {
            if (!thingy.isPrefab)
            {
                Vector3 position = ConvertVec3(thingy.pos);
                Vector3 scale = ConvertVec3(thingy.scale);

                print($"Spawning thingy: {thingy.thingyName} at {position}");

                Sprite sprite = Resources.Load<Sprite>($"sprites/game/bgs/{song.background}/{thingy.thingyName}");

                if (sprite == null)
                {
                    Debug.LogWarning($"Sprite not found: sprites/game/bgs/{song.background}/{thingy.thingyName}");
                    continue;
                }

                GameObject obj = new GameObject(thingy.thingyName);
                obj.transform.SetParent(BackgroundGOBJ.transform);
                obj.transform.localPosition = ConvertVec3(thingy.pos);
                obj.transform.localScale = ConvertVec3(thingy.scale);

                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = thingy.layer;
            }
            else
            {
                GameObject prefabBG = Resources.Load<GameObject>($"sprites/game/bgs/{song.background}/{thingy.thingyName}");

                if (prefabBG != null)
                {
                    GameObject obj = Instantiate(
                        prefabBG,
                        new Vector3(thingy.pos.x, thingy.pos.y, thingy.pos.z),
                        Quaternion.identity
                    );
                }
                else
                {
                    Debug.LogError($"Could not load prefab at path: sprites/game/bgs/{song.background}/{thingy.thingyName}");
                }
            }
        }

        if (bf != null && song.bf != "none")
        {
            bf.transform.localPosition = ConvertVec3(bg.chars.bf.pos);
            bf.GetComponent<SpriteRenderer>().sortingOrder = bg.chars.bf.layer;
            bfbgpos = true;
            print("Set BF position");
        }

        if (gf != null && song.op != "gf")
        {
            gf.transform.localPosition = ConvertVec3(bg.chars.gf.pos);
            gf.GetComponent<SpriteRenderer>().sortingOrder = bg.chars.gf.layer;
            gfbgpos = true;
            print("Set GF position (non-op)");
        }

        if (gf != null && song.op == "gf")
        {
            gf.transform.localPosition = ConvertVec3(bg.chars.gf.pos);
            gf.GetComponent<SpriteRenderer>().sortingOrder = bg.chars.gf.layer;
            opbgpos = true;
            print("Set GF position (as op)");
        }

        if (op != null && song.op != "none" && song.op != "gf")
        {
            op.transform.localPosition = ConvertVec3(bg.chars.op.pos);
            op.GetComponent<SpriteRenderer>().sortingOrder = bg.chars.op.layer;
            opbgpos = true;
            print("Set OP position");
        }
    }

    IEnumerator GameSetup()
    {
        if (!pause)
        {
            introScreen.SetActive(true);
            string currentSong = GlobalData.currentSong.songs[0];
            AudioClip intro1 = Resources.Load<AudioClip>("audio/game/intro1");
            AudioClip intro2 = Resources.Load<AudioClip>("audio/game/intro2");
            AudioClip intro3 = Resources.Load<AudioClip>("audio/game/intro3");
            AudioClip introGO = Resources.Load<AudioClip>("audio/game/introGo");
            AudioClip inst = Resources.Load<AudioClip>($"audio/game/{currentSong}/inst");
            AudioClip voiceBF = Resources.Load<AudioClip>($"audio/game/{currentSong}/Voices-bf");
            AudioClip voiceOP = Resources.Load<AudioClip>($"audio/game/{currentSong}/Voices-op");
            string dirPath = Path.Combine(Application.streamingAssetsPath, "songs", currentSong);
            if (Directory.Exists(dirPath))
            {
                foreach (string filePath in Directory.GetFiles(dirPath))
                {
                    string fileName = Path.GetFileName(filePath);
                    print(fileName);
                    if (fileName.EndsWith(".json") && fileName.StartsWith(GlobalData.currentSong.diff))
                    {
                        string json = File.ReadAllText(filePath);
                        currentSongJson = JsonUtility.FromJson<Song>(json);
                        GlobalData.currentSong.bf = currentSongJson.bf;
                        GlobalData.currentSong.op = currentSongJson.op;
                        GlobalData.currentSong.gf = currentSongJson.gf;
                        speedMultiplier = currentSongJson.speedMultiplier;
                        break;
                    }
                }

            }

            if (currentSongJson == null)
            {
                Debug.LogError("Failed to load song JSON for: " + currentSong);
                SceneManager.LoadScene("mainMenu");
                yield break;
            }
            if (GlobalData.currentSong.bf != "none")
            {
                string path = Path.Combine(Application.streamingAssetsPath, "chars", $"{GlobalData.currentSong.bf}.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    Character data = JsonUtility.FromJson<Character>(json);
                    if (data.scale.x > 0 && data.scale.y > 0 && data.scale.z > 0)
                    {
                        bf.transform.localScale = new Vector3(data.scale.x, data.scale.y, data.scale.z);
                    }
                    if ((data.newPos.x != 0f || data.newPos.y != 0f || data.newPos.z != 0f) && !bfbgpos)
                    {
                        bf.transform.localPosition = new Vector3(data.newPos.x, data.newPos.y, data.newPos.z);
                    }

                }
                else
                {
                    Debug.LogError("File not found at " + path);
                }
            }
            if (GlobalData.currentSong.op != "none" && GlobalData.currentSong.op != "gf")
            {
                string path = Path.Combine(Application.streamingAssetsPath, "chars", $"{GlobalData.currentSong.op}.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    Character data = JsonUtility.FromJson<Character>(json);
                    if (data.opFlip == 1)
                    {
                        op.GetComponent<SpriteRenderer>().flipX = true;
                    }
                    if (data.scale.x > 0 && data.scale.y > 0 && data.scale.z > 0)
                    {
                        op.transform.localScale = new Vector3(data.scale.x, data.scale.y, data.scale.z);
                    }
                    if ((data.newPos.x != 0f || data.newPos.y != 0f || data.newPos.z != 0f) && !opbgpos)
                    {
                        op.transform.localPosition = new Vector3(data.newPos.x, data.newPos.y, data.newPos.z);
                    }
                }
                else
                {
                    Debug.LogError("File not found at " + path);
                }
            }
            if (GlobalData.currentSong.gf != "none")
            {
                string path = Path.Combine(Application.streamingAssetsPath, "chars", $"{GlobalData.currentSong.gf}.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    Character data = JsonUtility.FromJson<Character>(json);
                    if (data.scale.x > 0 && data.scale.y > 0 && data.scale.z > 0)
                    {
                        gf.transform.localScale = new Vector3(data.scale.x, data.scale.y, data.scale.z);
                    }
                    if ((data.newPos.x != 0f || data.newPos.y != 0f || data.newPos.z != 0f) && !gfbgpos)
                    {
                        gf.transform.localPosition = new Vector3(data.newPos.x, data.newPos.y, data.newPos.z);
                    }

                }
                else
                {
                    Debug.LogError("File not found at " + path);
                }
            }
            if (GlobalData.currentSong.op == "gf")
            {
                string path = Path.Combine(Application.streamingAssetsPath, "chars", $"{GlobalData.currentSong.op}.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    Character data = JsonUtility.FromJson<Character>(json);
                    if (data.scale.x > 0 && data.scale.y > 0 && data.scale.z > 0)
                    {
                        gf.transform.localScale = new Vector3(data.scale.x, data.scale.y, data.scale.z);
                    }
                    if ((data.newPos.x != 0f || data.newPos.y != 0f || data.newPos.z != 0f) && !opbgpos)
                    {
                        gf.transform.localPosition = new Vector3(data.newPos.x, data.newPos.y, data.newPos.z);
                    }

                }
                else
                {
                    Debug.LogError("File not found at " + path);
                }
            }
            beatTime = 60f / currentSongJson.bpm;
            nextBeatTime = Time.time + beatTime;
            introScreen.transform.Find("ready").gameObject.SetActive(false);
            intro.clip = intro1; intro.Play(); yield return new WaitForSeconds(beatTime);
            introScreen.transform.Find("ready").gameObject.SetActive(true);
            intro.clip = intro2; intro.Play(); yield return new WaitForSeconds(beatTime);
            introScreen.transform.Find("ready").gameObject.SetActive(false);
            introScreen.transform.Find("set").gameObject.SetActive(true);
            intro.clip = intro3; intro.Play(); yield return new WaitForSeconds(beatTime);
            introScreen.transform.Find("set").gameObject.SetActive(false);
            introScreen.transform.Find("go").gameObject.SetActive(true);
            intro.clip = introGO; intro.Play(); yield return new WaitForSeconds(introGO.length);
            introScreen.SetActive(false);
            if (!pause)
            {
                if (inst)
                {
                    instSrc.clip = inst;
                    instSrc.Play();
                }
                if (bfVoice)
                {
                    bfVoice.clip = voiceBF;
                    bfVoice.Play();
                }
                if (opVoice)
                {
                    opVoice.clip = voiceOP;
                    opVoice.Play();
                }
            }
            isGameReady = true;
            StartCoroutine(SpawnBfNotes());
            StartCoroutine(SpawnOpNotes());
        }
    }

    int noteToIndex(string note)
    {
        switch (note)
        {
            case "left": return 0;
            case "down": return 1;
            case "up": return 2;
            case "right": return 3;
            default: return 0;
        }
    }
    string indexToNote(int note)
    {
        switch (note)
        {
            case 0: return "left";
            case 1: return "down";
            case 2: return "up";
            case 3: return "right";
            default: return "err";
        }
    }

    IEnumerator SpawnOpNotes()
    {
        if (!pause)
        {
            int noteIndex = 0;
            float bpm = currentSongJson != null && currentSongJson.bpm > 0 ? currentSongJson.bpm : 100f;
            float beatDuration = 60f / bpm;
            float beatsShownOnScreen = 4f;
            float travelTime = beatDuration * beatsShownOnScreen;
            Transform lane = opNotes.transform.GetChild(0);
            Transform noteLane = lane.childCount > 0 ? lane.GetChild(0) : null;
            Transform lStart = noteLane?.Find("lStart");
            Transform hb = noteLane?.Find("lEnd");

            if (lStart == null)
            {
                Debug.LogError("start not found.");
                yield break;
            }
            if (hb == null)
            {
                Debug.LogError("hb not found.");
                yield break;
            }
            float travelDistance = Vector3.Distance(lStart.position, hb.position);
            float noteSpeed = travelDistance / travelTime;


            while (noteIndex < currentSongJson.opNotes.Count)
            {
                NoteData note = currentSongJson.opNotes[noteIndex];
                float spawnTime = note.time - travelTime + spawnOffset;

                if (instSrc.time >= spawnTime)
                {
                    int laneIndex = noteToIndex(note.note);
                    StartCoroutine(MoveOpNote(laneIndex, note.hold, note.isAlt));
                    noteIndex++;
                }

                yield return null;
            }
        }
    }

    IEnumerator SpawnBfNotes()
    {
        if (!pause)
        {
            int noteIndex = 0;
            float bpm = currentSongJson != null && currentSongJson.bpm > 0 ? currentSongJson.bpm : 100f;
            float beatDuration = 60f / bpm;
            float beatsShownOnScreen = 4f;
            float travelTime = beatDuration * beatsShownOnScreen;
            Transform lane = bfNotes.transform.GetChild(0);
            Transform noteLane = lane.childCount > 1 ? lane.GetChild(1) : null;
            Transform lStart = noteLane?.Find("lStart");
            Transform hb = bfNotes.transform.GetChild(2);

            if (lStart == null || hb == null)
            {
                Debug.LogError("lStart or hb not found.");
                yield break;
            }

            float travelDistance = Vector3.Distance(lStart.position, hb.position);
            float noteSpeed = travelDistance / travelTime;


            while (noteIndex < currentSongJson.bfNotes.Count)
            {
                NoteData note = currentSongJson.bfNotes[noteIndex];
                float spawnTime = note.time - travelTime + spawnOffset;

                if (instSrc.time >= spawnTime)
                {
                    int laneIndex = noteToIndex(note.note);
                    StartCoroutine(MoveBfNote(laneIndex, note.hold, note.isAlt));
                    noteIndex++;
                }

                yield return null;
            }
        }
    }



    IEnumerator MoveBfNote(int noteIndex, float holdDuration, string alt)
    {
        if (bfNotes == null || noteIndex >= bfNotes.transform.childCount)
            yield break;

        Transform nsTF = bfNotes.transform.GetChild(noteIndex);
        Transform nlTF = nsTF.childCount > 1 ? nsTF.GetChild(1) : null;
        if (nlTF == null)
        {
            Debug.LogError($"nlTF is null for noteIndex {noteIndex}");
            yield break;
        }

        Transform nlSTF = nlTF.Find("lStart");
        Transform nlETF = nlTF.Find("lEnd");

        if (nlSTF == null || nlETF == null)
        {
            Debug.LogError($"lStart or lEnd not found for noteIndex {noteIndex}");
            foreach (Transform child in nlTF)
            {
                print($"Child name: {child.name}");
            }
            yield break;
        }

        int bpm = 100;
        if (currentSongJson != null && currentSongJson.bpm > 0)
        {
            bpm = currentSongJson.bpm;
        }

        float beatsShownOnScreen = 4f;
        float beatDuration = 60f / bpm;
        float travelTime = beatDuration * beatsShownOnScreen;

        GameObject nGO = new GameObject($"note{noteIndex}");
        if (alt == "1")
        {
            GameObject isAlt = new GameObject("alt");
            isAlt.transform.parent = nGO.transform;
        }
        SpriteRenderer nGOSR = nGO.AddComponent<SpriteRenderer>();
        nGOSR.sprite = Resources.Load<Sprite>($"sprites/game/notes/{noteIndex}");
        nGOSR.sortingOrder = 10;
        nGO.transform.SetParent(nlTF);
        nGO.transform.position = nlSTF.position;
        nGO.transform.localScale = new Vector3(0.6032144f, 0.07039306f, 0.7744536f);

        BoxCollider2D nBC2D = nGO.AddComponent<BoxCollider2D>();
        nBC2D.size = nGOSR.sprite.bounds.size;
        nBC2D.offset = nGOSR.sprite.bounds.center;
        nBC2D.isTrigger = true;

        Rigidbody2D rb = nGO.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        float spawnTime = Time.time;

        GameObject holdTail = null;
        if (holdDuration > 0.1f)
        {
            holdTail = new GameObject($"holdTail{noteIndex}");
            SpriteRenderer holdTailSR = holdTail.AddComponent<SpriteRenderer>();
            holdTailSR.sprite = Resources.Load<Sprite>($"sprites/game/notes/hold{noteIndex}");
            holdTailSR.sortingOrder = 9;
            holdTail.transform.SetParent(nGO.transform);

            holdTail.transform.localPosition = new Vector3(0f, -1.3f, 0f);
            GameObject holdTime = new GameObject("holdTime");
            holdTime.transform.SetParent(nGO.transform);
            holdTime.transform.localPosition = new Vector3(0f, holdDuration, 0f);
            float stretchAmount = Mathf.Max(holdDuration * (1f / travelTime), 0.5f);
            holdTail.transform.localScale = new Vector3(1f, holdDuration, 1f);
        }

        while (nGO != null)
        {
            if (nGO.transform.position == nlETF.position)
            {
                Destroy(nGO);
                misses++;

                string note = indexToNote(noteIndex);
                Animator bfAnimate = bf.GetComponent<Animator>();
                string clipName = $"{GlobalData.currentSong.bf}miss{note}";
                ratingAnimate.Play("shit", 0, 0f);
                if (score > 0)
                {
                    score -= 150;
                }
                int randomMissAudClipInt = UnityEngine.Random.Range(1, 4);
                AudioClip randomMissAudClip = Resources.Load<AudioClip>($"audio/game/missnote{randomMissAudClipInt}");
                missAud.clip = randomMissAudClip;
                missAud.Play();
                if (bfHealthInt > 1)
                {
                    bfHealthInt -= 25;

                    if (bfHealthInt < 1)
                        bfHealthInt = 0;
                }
                if (opHealthInt < 100)
                {
                    opHealthInt += 25;

                    if (opHealthInt >= 100)
                        bfHealthInt = 100;
                }
                if (StateExists(bfAnimate, clipName))
                {
                    bfAnimate.Play(clipName, 0, 0f);
                    noteHittt = true;
                    StartCoroutine(ResetToBFIdle(bfAnimate, note));
                }
            }
            if (pause || frozenBfNotes.Contains(nGO.transform) && holdTail != null || pause)
            {
                yield return null;
                continue;
            }

            float elapsed = Time.time - spawnTime;
            float percent = Mathf.Clamp01(elapsed / travelTime);

            float adjustedPercent = Mathf.Clamp01(percent * speedMultiplier);
            if (!pause)
            {
                nGO.transform.position = Vector3.Lerp(nlSTF.position, nlETF.position, adjustedPercent);
            }
            if (percent >= 1f)
                break;

            yield return null;
        }


        if (nGO != null)
        {
            Destroy(nGO);
            misses++;

            string note = indexToNote(noteIndex);
            Animator bfAnimate = bf.GetComponent<Animator>();
            string clipName = $"{GlobalData.currentSong.bf}miss{note}";
            ratingAnimate.Play("shit", 0, 0f);
            if (score > 0)
            {
                score -= 150;
            }
            int randomMissAudClipInt = UnityEngine.Random.Range(1, 4);
            AudioClip randomMissAudClip = Resources.Load<AudioClip>($"audio/game/missnote{randomMissAudClipInt}");
            missAud.clip = randomMissAudClip;
            missAud.Play();
            if (bfHealthInt > 1)
            {
                bfHealthInt -= 25;

                if (bfHealthInt < 1)
                    bfHealthInt = 0;
            }
            if (opHealthInt < 100)
            {
                opHealthInt += 25;

                if (opHealthInt >= 100)
                    bfHealthInt = 100;
            }
            if (StateExists(bfAnimate, clipName))
            {
                bfAnimate.Play(clipName, 0, 0f);
                noteHittt = true;
                StartCoroutine(ResetToBFIdle(bfAnimate, note));
            }
        }

        if (holdTail != null)
            Destroy(holdTail);
    }

    IEnumerator MoveOpNote(int noteIndex, float holdDuration, string alt)
    {
        if (opNotes == null || noteIndex >= opNotes.transform.childCount)
            yield break;


        Transform nsTF = opNotes.transform.GetChild(noteIndex);
        Transform nlTF = nsTF.childCount > 0 ? nsTF.GetChild(0) : null;
        if (nlTF == null)
        {
            Debug.LogError($"nlTF is null for noteIndex {noteIndex}");
            yield break;
        }

        Transform nlSTF = nlTF.Find("lStart");
        Transform nlETF = nlTF.Find("lEnd");

        if (nlSTF == null || nlETF == null)
        {
            Debug.LogError($"lStart or lEnd not found for noteIndex {noteIndex}");
            foreach (Transform child in nlTF)
            {
                print($"Child name: {child.name}");
            }
            yield break;
        }

        int bpm = 100;
        if (currentSongJson != null && currentSongJson.bpm > 0)
        {
            bpm = currentSongJson.bpm;
        }

        float beatsShownOnScreen = 4f;
        float beatDuration = 60f / bpm;
        float travelTime = beatDuration * beatsShownOnScreen;

        GameObject nGO = new GameObject($"note{noteIndex}");
        if (alt == "1")
        {
            GameObject isAlt = new GameObject("alt");
            isAlt.transform.parent = nGO.transform;
        }
        SpriteRenderer nGOSR = nGO.AddComponent<SpriteRenderer>();
        nGOSR.sprite = Resources.Load<Sprite>($"sprites/game/notes/{noteIndex}");
        nGOSR.sortingOrder = 10;
        nGO.transform.SetParent(nlTF);
        nGO.transform.position = nlSTF.position;
        nGO.transform.localScale = new Vector3(0.6032144f, 0.07039306f, 0.7744536f);

        BoxCollider2D nBC2D = nGO.AddComponent<BoxCollider2D>();
        nBC2D.size = nGOSR.sprite.bounds.size;
        nBC2D.offset = nGOSR.sprite.bounds.center;
        nBC2D.isTrigger = true;

        Rigidbody2D rb = nGO.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;

        float spawnTime = Time.time;

        GameObject holdTail = null;
        if (holdDuration > 0.1f)
        {
            holdTail = new GameObject($"holdTail{noteIndex}");
            SpriteRenderer holdTailSR = holdTail.AddComponent<SpriteRenderer>();
            holdTailSR.sprite = Resources.Load<Sprite>($"sprites/game/notes/hold{noteIndex}");
            holdTailSR.sortingOrder = 9;
            holdTail.transform.SetParent(nGO.transform);

            holdTail.transform.localPosition = new Vector3(0f, -1.3f, 0f);

            float stretchAmount = Mathf.Max(holdDuration * (1f / travelTime), 0.5f);
            holdTail.transform.localScale = new Vector3(1f, holdDuration, 1f);
        }

        while (nGO != null)
        {
            if (pause || frozenOpNotes.Contains(nGO.transform) && holdTail != null)
            {
                yield return null;
                continue;
            }

            float elapsed = Time.time - spawnTime;
            float percent = Mathf.Clamp01(elapsed / travelTime);

            float adjustedPercent = Mathf.Clamp01(percent * speedMultiplier);
            if (!pause)
            {
                nGO.transform.position = Vector3.Lerp(nlSTF.position, nlETF.position, adjustedPercent);
            }
            if (percent >= 1f)
                break;

            yield return null;
        }

        if (holdTail != null)
            Destroy(holdTail);
    }

    void Update()
    {
        if (!Application.isFocused && !playingCut)
        {
            if (instSrc.clip)
            {
                instSrc.Pause();
            }
            if (bfVoice.clip)
            {
                bfVoice.Pause();
            }
            if (opVoice.clip)
            {
                opVoice.Pause();
            }
            Time.timeScale = 0f;
            pause = true;
        }
        if (!pause)
        {

            if (GlobalData.currentSong.gf == "none")
            {
                gf.SetActive(GlobalData.currentSong.op == "gf");

                if (GlobalData.currentSong.op == "gf" && !noteHitttOP)
                {
                    Animator gfAnimate = gf.GetComponent<Animator>();
                    RuntimeAnimatorController gfController = Resources.Load<RuntimeAnimatorController>($"animations/game/chars/{GlobalData.currentSong.op}/{GlobalData.currentSong.op}");
                    if (gfController == null) return;

                    gfAnimate.runtimeAnimatorController = gfController;
                    AnimationClip gfClip = gfController.animationClips.Length > 0 ? gfController.animationClips[0] : null;
                    if (gfClip == null) return;

                    gfAnimate.speed = beatTime / gfClip.length;
                    gfAnimate.Play("gfidle");
                    op.SetActive(false);
                }
            }
            else
            {
                gf.SetActive(true);

                Animator gfAnimate = gf.GetComponent<Animator>();
                RuntimeAnimatorController gfController = Resources.Load<RuntimeAnimatorController>($"animations/game/chars/{GlobalData.currentSong.gf}/{GlobalData.currentSong.gf}");
                if (gfController == null) return;

                gfAnimate.runtimeAnimatorController = gfController;
                AnimationClip gfClip = gfController.animationClips.Length > 0 ? gfController.animationClips[0] : null;
                if (gfClip == null) return;

                gfAnimate.speed = gfClip.length / beatTime;
                gfAnimate.Play($"{GlobalData.currentSong.gf}idle");
            }


            if (GlobalData.currentSong.bf != "none")
            {
                bf.SetActive(true);
                if (!noteHittt)
                {
                    Animator bfAnimate = bf.GetComponent<Animator>();
                    RuntimeAnimatorController bfController = Resources.Load<RuntimeAnimatorController>($"animations/game/chars/{GlobalData.currentSong.bf}/{GlobalData.currentSong.bf}");
                    if (bfController == null)
                    {
                        Debug.LogError($"Failed to load animator controller for {GlobalData.currentSong.bf}");
                        return;
                    }
                    bfAnimate.runtimeAnimatorController = bfController;

                    if (Time.time >= nextBeatTime)
                    {
                        bfAnimate.Play($"{GlobalData.currentSong.bf}idle", 0, 0f);
                        nextBeatTime += beatTime;
                    }
                }
            }
            else bf.SetActive(false);

            if (GlobalData.currentSong.op != "none" && GlobalData.currentSong.op != "gf")
            {
                op.SetActive(true);
                if (!noteHitttOP)
                {
                    Animator opAnimate = op.GetComponent<Animator>();
                    RuntimeAnimatorController opController = Resources.Load<RuntimeAnimatorController>($"animations/game/chars/{GlobalData.currentSong.op}/{GlobalData.currentSong.op}");
                    if (opController == null)
                    {
                        Debug.LogError($"Failed to load animator controller for {GlobalData.currentSong.op}");
                        return;
                    }
                    opAnimate.runtimeAnimatorController = opController;

                    if (Time.time >= nextBeatTime)
                    {
                        opAnimate.Play($"{GlobalData.currentSong.op}idle", 0, 0f);
                        nextBeatTime += beatTime;
                    }
                }
            }
            else op.SetActive(false);

            CheckNoteHit(left, 0);
            CheckNoteHit(down, 1);
            CheckNoteHit(up, 2);
            CheckNoteHit(right, 3);
            checkKeyUp(left, 0);
            checkKeyUp(down, 1);
            checkKeyUp(up, 2);
            checkKeyUp(right, 3);
            checkKeyDown(left, 0);
            checkKeyDown(down, 1);
            checkKeyDown(up, 2);
            checkKeyDown(right, 3);
            scoreObj.text = $"SCORE: {score}";
            scoreObj.sortingOrder = 15;
            if (!instSrc.isPlaying && !bfVoice.isPlaying && !opVoice.isPlaying && !changing && isGameReady && !pause)
            {
                changing = true;
                string path = Path.Combine(Application.streamingAssetsPath, "data.txt");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    byte[] bytes = Convert.FromBase64String(json);
                    string decodedData = Encoding.UTF8.GetString(bytes);
                    GameData data = JsonUtility.FromJson<GameData>(decodedData);

                    bool found = false;

                    foreach (Songscores songScore in data.scores)
                    {
                        if (songScore.sn == GlobalData.currentSong.songs[0] && songScore.d == GlobalData.currentSong.diff)
                        {
                            if (score > songScore.n)
                            {
                                songScore.n = score;
                            }
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        data.scores.Add(new Songscores
                        {
                            sn = GlobalData.currentSong.songs[0],
                            n = score,
                            d = GlobalData.currentSong.diff
                        });
                    }

                    string newJson = JsonUtility.ToJson(data);
                    byte[] newBytes = Encoding.UTF8.GetBytes(newJson);
                    string encodedData = Convert.ToBase64String(newBytes);
                    File.WriteAllText(path, encodedData);

                    GlobalData.currentSong.songs.Remove(GlobalData.currentSong.songs[0]);
                    SceneManager.LoadScene("game");
                }
            }
            float total = bfHealthInt + opHealthInt;
            if (total <= 0f) total = 1f;

            float bfPercent = bfHealthInt / total;
            float opPercent = opHealthInt / total;

            float bfScaleX = maxBarWidth * bfPercent;
            float opScaleX = maxBarWidth * opPercent;

            bfHealth.transform.localScale = new Vector3(bfScaleX, bfHealth.transform.localScale.y, bfHealth.transform.localScale.z);
            opHealth.transform.localScale = new Vector3(opScaleX, opHealth.transform.localScale.y, opHealth.transform.localScale.z);

            bfHealth.transform.localPosition = new Vector3(0f, bfHealth.transform.localPosition.y, bfHealth.transform.localPosition.z);
            opHealth.transform.localPosition = new Vector3(0f - opScaleX, opHealth.transform.localPosition.y, opHealth.transform.localPosition.z);
            opIcon.transform.localPosition = new Vector3(opHealth.transform.localPosition.x, opHealth.transform.localPosition.y, opHealth.transform.localPosition.z);
            bfIcon.transform.localPosition = new Vector3(bfHealth.transform.localPosition.x, bfHealth.transform.localPosition.y, bfHealth.transform.localPosition.z);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!currentlyPlayingDialog)
            {
                if (!pause)
                {
                    if (instSrc.clip)
                    {
                        instSrc.Pause();
                    }
                    if (bfVoice.clip)
                    {
                        bfVoice.Pause();
                    }
                    if (opVoice.clip)
                    {
                        opVoice.Pause();
                    }
                    Time.timeScale = 0f;
                    pause = true;
                }
                else
                {
                    selectMenuOption();
                }
            }
            else
            {
                print(currentDialog);
                if (currentDialog != currentSongJson.dialog.Count - 1)
                {
                    currentDialog += 1;
                    Destroy(dialog.transform.Find("textBox").gameObject);
                    Destroy(dialog.transform.Find("port").gameObject);
                    RenderDialog();
                }
                else
                {
                    currentDialog = 0;
                    pause = false;
                    currentlyPlayingDialog = false;
                    Destroy(dialog.transform.Find("textBox").gameObject);
                    Destroy(dialog.transform.Find("port").gameObject);
                    dialog.SetActive(false);
                    StartCoroutine(GameSetup());
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pause)
            {
                if (instSrc.clip)
                {
                    instSrc.Pause();
                }
                if (bfVoice.clip)
                {
                    bfVoice.Pause();
                }
                if (opVoice.clip)
                {
                    opVoice.Pause();
                }
                pause = true;
                Time.timeScale = 0f;
            }
            else
            {
                pause = false;
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }
        }
        if (pause)
        {
            menu.SetActive(true);
            sDiffOnMenu.text = GlobalData.currentSong.diff.ToUpper();
            sNameOnMenu.text = GlobalData.currentSong.songs[0].ToUpper();
            sNameOnMenu.sortingOrder = 100;
            sDiffOnMenu.sortingOrder = 100;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentOption--;
            if (currentOption < 0)
                currentOption = 2;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentOption++;
            if (currentOption > 2)
                currentOption = 0;
        }

        if (menu.activeSelf == true)
        {
            foreach (Transform option in menuOptions.transform)
            {
                option.GetComponent<MeshRenderer>().sortingOrder = 15;
            }

            if (currentOption == 0)
            {
                Transform res = menuOptions.transform.Find("res");
                res.localScale = new Vector3(0.2144881f, 0.2144881f, 0.2144881f);
            }
            else
            {
                Transform res = menuOptions.transform.Find("res");
                res.localScale = new Vector3(0.1782499f, 0.1782499f, 0.1782499f);
            }
            if (currentOption == 1)
            {
                Transform rest = menuOptions.transform.Find("rest");
                rest.localScale = new Vector3(0.2144881f, 0.2144881f, 0.2144881f);
            }
            else
            {
                Transform rest = menuOptions.transform.Find("rest");
                rest.localScale = new Vector3(0.1782499f, 0.1782499f, 0.1782499f);
            }
            if (currentOption == 2)
            {
                Transform res = menuOptions.transform.Find("exit");
                res.localScale = new Vector3(0.2144881f, 0.2144881f, 0.2144881f);
            }
            else
            {
                Transform exit = menuOptions.transform.Find("exit");
                exit.localScale = new Vector3(0.1782499f, 0.1782499f, 0.1782499f);
            }
        }
    }
    private void selectMenuOption()
    {
        if (currentOption == 0)
        {
            if (!isGameReady)
            {
                Time.timeScale = 1f;
                pause = false;
                GameSetup();
            }
            else
            {
                menu.SetActive(false);
                if (instSrc.clip)
                {
                    instSrc.UnPause();
                }
                if (bfVoice.clip)
                {
                    bfVoice.UnPause();
                }
                if (opVoice.clip)
                {
                    opVoice.UnPause();
                }
                Time.timeScale = 1f;
                pause = false;
            }
        }
        if (currentOption == 1)
        {
            SceneManager.LoadScene("game");
        }
        if (currentOption == 2)
        {
            pause = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");

        }
    }
    private void CheckNoteHit(KeyCode key, int noteIndex)
    {
        if (Input.GetKeyDown(key))
        {
            bool noteHit = false;

            foreach (Transform noteTransform in bfNotes.transform)
            {
                if (noteToIndex(noteTransform.name) == noteIndex)
                {
                    Transform lane = noteTransform.GetChild(1);
                    Transform hb = noteTransform.Find("hb");
                    Transform note = lane.Find($"note{noteIndex}");
                    if (note != null)
                    {
                        float distance = Vector3.Distance(hb.position, note.position);

                        if (distance <= 0.05549264f)
                        {
                            ratingAnimate.Play("sick", 0, 0f);
                            if (noteTransform.Find("alt") != null)
                            {
                                NoteHitBF(noteTransform.name, "Sick", true);
                            }
                            else
                            {
                                NoteHitBF(noteTransform.name, "Sick", false);
                            }
                            noteHit = true;
                            score += 350;
                            if (bfHealthInt < 100)
                            {
                                bfHealthInt += 15;

                                if (bfHealthInt < 1)
                                    bfHealthInt = 0;
                                if (bfHealthInt >= 100)
                                {
                                    bfHealthInt = 100;

                                }
                            }
                            if (opHealthInt > 0)
                            {
                                opHealthInt -= 15;

                                if (opHealthInt <= 0)
                                    opHealthInt = 0;
                            }
                            Transform holdTail = note.transform.Find($"holdTail{noteIndex}");
                            if (holdTail == null)
                            {
                                Destroy(note.gameObject);
                            }
                            else
                            {
                                frozenBfNotes.Add(note);
                                note.position = hb.position;
                            }
                            break;
                        }
                        else if (distance <= 1.514982f)
                        {

                            ratingAnimate.Play("good", 0, 0f);
                            if (noteTransform.Find("alt") != null)
                            {
                                NoteHitBF(noteTransform.name, "Good", true);
                            }
                            else
                            {
                                NoteHitBF(noteTransform.name, "Good", false);
                            }
                            noteHit = true;
                            score += 200;
                            if (bfHealthInt < 100)
                            {
                                bfHealthInt += 15;

                                if (bfHealthInt < 1)
                                    bfHealthInt = 0;
                                if (bfHealthInt >= 100)
                                {
                                    bfHealthInt = 100;

                                }
                            }
                            if (opHealthInt < 100)
                            {
                                opHealthInt -= 15;

                                if (opHealthInt <= 0)
                                    opHealthInt = 100;
                            }
                            Transform holdTail = note.transform.Find($"holdTail{noteIndex}");
                            if (holdTail == null)
                            {
                                Destroy(note.gameObject);
                            }
                            else
                            {
                                frozenBfNotes.Add(note);
                                note.position = hb.position;
                            }
                            break;
                        }
                        else if (distance <= 3.451254f)
                        {
                            score += 100;
                            ratingAnimate.Play("bad", 0, 0f);
                            if (noteTransform.Find("alt") != null)
                            {
                                NoteHitBF(noteTransform.name, "Bad", true);
                            }
                            else
                            {
                                NoteHitBF(noteTransform.name, "Bad", false);
                            }
                            noteHit = true;
                            if (bfHealthInt > 1)
                            {
                                bfHealthInt -= 5;

                                if (bfHealthInt < 1)
                                    bfHealthInt = 0;
                            }
                            if (opHealthInt < 100)
                            {
                                opHealthInt += 5;

                                if (opHealthInt >= 100)
                                    bfHealthInt = 100;
                            }
                            Transform holdTail = note.transform.Find($"holdTail{noteIndex}");
                            if (holdTail == null)
                            {
                                Destroy(note.gameObject);
                            }
                            else
                            {
                                frozenBfNotes.Add(note);
                                note.position = hb.position;
                            }
                            break;
                        }
                        else if (distance <= 6.3007f)
                        {
                            ratingAnimate.Play("shit", 0, 0f);
                            if (noteTransform.Find("alt") != null)
                            {
                                NoteHitBF(noteTransform.name, "Shit", true);
                            }
                            else
                            {
                                NoteHitBF(noteTransform.name, "Shit", false);
                            }
                            noteHit = true;
                            if (bfHealthInt > 1)
                            {
                                bfHealthInt -= 10;

                                if (bfHealthInt < 1)
                                    bfHealthInt = 0;
                            }
                            if (opHealthInt < 100)
                            {
                                opHealthInt += 10;

                                if (opHealthInt >= 100)
                                    bfHealthInt = 100;
                            }
                            Transform holdTail = note.transform.Find($"holdTail{noteIndex}");
                            if (holdTail == null)
                            {
                                Destroy(note.gameObject);
                            }
                            else
                            {
                                frozenBfNotes.Add(note);
                                note.position = hb.position;
                            }
                            break;
                        }
                    }
                    else
                    {
                        noteHit = false;
                        Animator bfAnimate = bf.GetComponent<Animator>();
                        ratingAnimate.Play("shit", 0, 0f);
                        if (score > 0)
                        {
                            score -= 150;
                        }
                        if (bfHealthInt > 1)
                        {
                            bfHealthInt -= 25;

                            if (bfHealthInt < 1)
                                bfHealthInt = 0;
                        }
                        if (opHealthInt < 100)
                        {
                            opHealthInt += 25;

                            if (opHealthInt >= 100)
                                bfHealthInt = 100;
                        }
                        string clipName = $"{GlobalData.currentSong.bf}miss{noteTransform.name}";

                        if (StateExists(bfAnimate, clipName))
                        {
                            bfAnimate.Play(clipName, 0, 0f);
                            noteHittt = true;
                            StartCoroutine(ResetToBFIdle(bfAnimate, noteTransform.name));
                        }
                    }
                }
            }

            if (!noteHit)
            {
                misses++;
                int randomMissAudClipInt = UnityEngine.Random.Range(1, 4);
                AudioClip randomMissAudClip = Resources.Load<AudioClip>($"audio/game/missnote{randomMissAudClipInt}");
                missAud.clip = randomMissAudClip;
                missAud.Play();
            }
        }
    }

    private void checkKeyDown(KeyCode key, int noteIndex)
    {
        if (Input.GetKeyDown(key))
        {
            Transform noteKey = bfNotes.transform.GetChild(noteIndex);
            SpriteRenderer sr = noteKey.GetComponent<SpriteRenderer>();
            Color c = sr.color; c.a = 1f; sr.color = c;
            foreach (Transform noteTransform in bfNotes.transform)
            {
                if (noteToIndex(noteTransform.name) == noteIndex)
                {
                    Transform lane = noteTransform.GetChild(1);
                    Transform note = lane.Find($"note{noteIndex}");

                    if (note != null)
                    {
                        Transform holdTail = note.Find($"holdTail{noteIndex}");

                        if (holdTail != null)
                        {
                            if (activeHoldNotes.ContainsKey(noteIndex))
                            {
                                StopCoroutine(activeHoldNotes[noteIndex]);
                            }

                            Transform holdDurationObj = note.Find("holdTime");
                            float holdDuration = holdDurationObj != null ? holdDurationObj.localPosition.y : 0f;

                            activeHoldNotes[noteIndex] = StartCoroutine(HoldNoteCoroutine(note, noteIndex, holdDuration));
                        }
                    }
                }
            }
        }
    }


    private void checkKeyUp(KeyCode key, int noteIndex)
    {
        if (Input.GetKeyUp(key))
        {
            Transform noteKey = bfNotes.transform.GetChild(noteIndex);
            SpriteRenderer sr = noteKey.GetComponent<SpriteRenderer>();
            Color c = sr.color; c.a = 0.5f; sr.color = c;
            if (activeHoldNotes.ContainsKey(noteIndex))
            {
                StopCoroutine(activeHoldNotes[noteIndex]);
                activeHoldNotes.Remove(noteIndex);
                misses++;
                int randomMissAudClipInt = UnityEngine.Random.Range(1, 4);
                AudioClip randomMissAudClip = Resources.Load<AudioClip>($"audio/game/missnote{randomMissAudClipInt}");
                missAud.clip = randomMissAudClip;
                missAud.Play();
                foreach (Transform noteTransform in bfNotes.transform)
                {
                    if (noteToIndex(noteTransform.name) == noteIndex)
                    {
                        Transform lane = noteTransform.GetChild(1);
                        Transform note = lane.Find($"note{noteIndex}");

                        if (note != null)
                        {
                            Destroy(note.gameObject);
                            noteHittt = false;
                        }
                    }
                }
            }
        }
    }
    private IEnumerator HoldNoteCoroutine(Transform note, int noteIndex, float holdDuration)
    {
        float elapsedHoldTime = 0f;

        Transform holdTail = note.Find($"holdTail{noteIndex}");
        if (holdTail == null)
        {
            yield break;
        }

        while (true)
        {
            elapsedHoldTime += Time.deltaTime;

            holdTail.localScale = new Vector3(1f, Mathf.Max(holdDuration - elapsedHoldTime, 0f), 1f);
            Animator bfAnimate = bf.GetComponent<Animator>();
            string noteName = indexToNote(noteIndex);
            string clipName = $"{GlobalData.currentSong.bf}{noteName}";
            if (bfAnimate.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
            {
                bfAnimate.Play(clipName, 0, 0f);
            }
            noteHittt = true;
            if (elapsedHoldTime >= holdDuration)
            {
                if (note != null)
                {
                    noteHittt = false;
                    Destroy(note.gameObject);
                }

                if (activeHoldNotes.ContainsKey(noteIndex))
                    activeHoldNotes.Remove(noteIndex);

                yield break;
            }

            yield return null;
        }
    }
}

