using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
public class mainMenu : MonoBehaviour
{
    public GameObject menu;
    public GameObject startMenu;
    public Animator story;
    public Animator fp;
    public Animator options;
    public GameObject selectAud;
    public GameObject pulse;
    public GameObject enterHitAud;
    public Animator enterHit;
    private int switched = 0;
    private int currentMenu = 0;
    private int currentlySelected = 0;
    public Animator menuBG;

    private bool isPlayingSelected = false;

    void Start()
    {
        PlayIfNotAlready("menuIdle");
    }

    private void PlayIfNotAlready(string stateName)
    {
        if (!menuBG.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            menuBG.Play(stateName);
        }
    }

    private IEnumerator DelayedSceneLoad()
    {
        enterHitAud.SetActive(false);
        enterHitAud.SetActive(true);
        enterHit.Play("enterHit");
        PlayIfNotAlready("menuIdle");
        pulse.SetActive(false);
        pulse.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        PlayIfNotAlready("menuIdle");
        menu.SetActive(true);
        startMenu.SetActive(false);
        switched = 0;
    }

    private IEnumerator PlayGameMode(int gameMode)
    {
        isPlayingSelected = true;

        enterHitAud.SetActive(false);
        enterHitAud.SetActive(true);
        PlayIfNotAlready("selectedOption");
        if (gameMode == 0)
        {
            story.Play("storyClicked");
        }
        else if (gameMode == 1)
        {
            fp.Play("fpClicked");
        }
        else if (gameMode == 2)
        {
            options.Play("optionsClicked");
        }

        yield return new WaitForSeconds(0.9f);
        PlayIfNotAlready("menuIdle");

        if (gameMode == 0)
        {
            SceneManager.LoadScene("storyMode");
        }
        else if (gameMode == 1)
        {
            SceneManager.LoadScene("freeplay");
        }
        else if (gameMode == 2)
        {
            SceneManager.LoadScene("options");
        }

        isPlayingSelected = false;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && switched == 0)
        {
            menu.SetActive(false);
            pulse.SetActive(false);
            pulse.SetActive(true);
            startMenu.SetActive(true);
            switched = 1;
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            if (switched == 1)
            {
                StartCoroutine(DelayedSceneLoad());
            }
            else if (switched == 0 && currentMenu == 1)
            {
                StartCoroutine(PlayGameMode(currentlySelected));
            }
        }

        if (Input.GetKeyUp(KeyCode.UpArrow) && switched == 0)
        {
            selectAud.SetActive(false);
            selectAud.SetActive(true);
            currentlySelected = (currentlySelected == 0) ? 2 : currentlySelected - 1;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) && switched == 0)
        {
            selectAud.SetActive(false);
            selectAud.SetActive(true);
            currentlySelected = (currentlySelected == 2) ? 0 : currentlySelected + 1;
        }

        if (!isPlayingSelected)
        {
            if (currentlySelected == 0)
            {
                story.Play("storyHover");
                fp.Play("fpIdle");
                options.Play("optionsIdle");
            }
            else if (currentlySelected == 1)
            {
                story.Play("storyIdle");
                fp.Play("fpHover");
                options.Play("optionsIdle");
            }
            else if (currentlySelected == 2)
            {
                story.Play("storyIdle");
                fp.Play("fpIdle");
                options.Play("optionsHover");
            }
        }
        if (startMenu.activeSelf == true)
        {
            currentMenu = 0;
        }
        else if (menu.activeSelf == true)
        {
            currentMenu = 1;
        }
    }
}
