using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class notesBCOP : MonoBehaviour
{
    private gameScript gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<gameScript>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //print($"{parentNoteName} note hit at a {gameObject.name} rating");

        if (gameManager != null)
        {
            bool isAlt;
            if (other.transform.Find("alt") != null)
            {
                isAlt = true;
            }else{
                isAlt = false;
            }
            Destroy(other.gameObject);
            gameManager.NoteHitOp(gameObject.name, isAlt);
        }
        else
        {
            Debug.LogWarning("GameManager not found!");
        }
    }
}
