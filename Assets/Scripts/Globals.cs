using System;
using UnityEngine;

public static class Globals
{
    public static int Rows = PlayerPrefs.GetInt("Rows", 4);
    public static int Columns = PlayerPrefs.GetInt("Columns", 4);
    public static float AnimationDuration =  PlayerPrefs.GetFloat("AnimDuration", 0.05f);

    public static void Apply()
    {
        Rows = PlayerPrefs.GetInt("Rows", 4);
        Columns = PlayerPrefs.GetInt("Columns", 4);
        AnimationDuration =  PlayerPrefs.GetFloat("AnimDuration", 0.05f);
    }
}