using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
public class NewBehaviourScript : MonoBehaviour
{
    public GameObject loading;
    public GameObject main;
    public TextMeshPro TitleLoadText;
    public TextRenderer textRender;
    public Animator enterAni;
    public GameObject enterAud;
    public GameObject pulse;
    private float timer = 0f;
    private bool switched = false;


    void Start()
    {
        TitleLoadText.fontSharedMaterial = new Material(TitleLoadText.fontSharedMaterial);
        enterAud.SetActive(false);
        loading.SetActive(true);
        main.SetActive(false);
    }
    public void EnterHit()
    {
        enterAud.SetActive(true);
        enterAni.Play("enterHit");
        pulse.SetActive(false);
        pulse.SetActive(true);
        StartCoroutine(DelayedSceneLoad());
    }

    private IEnumerator DelayedSceneLoad()
    {
        yield return new WaitForSeconds(enterAni.GetCurrentAnimatorStateInfo(0).length);
        SceneManager.LoadScene("MainMenu");
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            if (!switched && timer <= 9.4f)
            {
                loading.SetActive(false);
                main.SetActive(true);
                switched = true;
                pulse.SetActive(false);
                pulse.SetActive(true);
            }
            else
            {
                EnterHit();
            }
        }
        timer += Time.deltaTime;

        if (!switched && timer >= 9.0f)
        {
            loading.SetActive(false);
            main.SetActive(true);
            pulse.SetActive(false);
            pulse.SetActive(true);
            switched = true;
        }
        else if (!switched && timer >= 1.0f && timer < 2.0f)
        {
            string newRender = textRender.GetStringIS("Assets By", "bold");
            TitleLoadText.text = newRender;
        }
        else if (!switched && timer >= 3.0f && timer < 4.0f)
        {
            string newRender = textRender.GetStringIS("The Funkin Crew", "bold");
            TitleLoadText.text = newRender;
        }
        else if (!switched && timer >= 5.0f && timer < 6.0f)
        {
            string newRender = textRender.GetStringIS("SOUNDGOD Presents", "bold");
            TitleLoadText.text = newRender;
        }
        else if (!switched && timer >= 7.0f && timer < 7.5f)
        {
            string newRender = textRender.GetStringIS("Friday Night Funkin", "bold");
            TitleLoadText.text = newRender;
        }
        else if (!switched && timer >= 7.6f && timer < 8.0f)
        {
            string newRender = textRender.GetStringIS("Unity", "bold");
            TitleLoadText.text = newRender;
        }
        else if (!switched && timer >= 8.0f && timer < 8.3f)
        {
            string newRender = textRender.GetStringIS("SOUND", "bold");
            TitleLoadText.text = newRender;
        }
        else if (!switched && timer >= 8.3f && timer < 8.6f)
        {
            string newRender = textRender.GetStringIS("ENGINE", "bold");
            TitleLoadText.text = newRender;
        }
    }
}