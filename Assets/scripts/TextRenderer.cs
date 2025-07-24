using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextRenderer : MonoBehaviour
{
public string GetStringIS(string str, string font, int isColor = 0, string color = "#FFFFFFFF")
{
    string newString = "";

    foreach (char ch in str)
    {
        if (ch == ' ')
        {
            newString += " ";
        }
        else
        {
            string spriteName = font == "bold" ? char.ToUpper(ch).ToString() : char.ToLower(ch).ToString();

            if (isColor == 1)
            {
                newString += $"<sprite name=\"{spriteName}\" color={color} tint=1>";
            }
            else
            {
                newString += $"<sprite name=\"{spriteName}\">";
            }
        }
    }

    return newString;
}

    void Start()
    {
    }

    void Update()
    {

    }
}
