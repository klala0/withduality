using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Leaderboard
{
    public static List<float> scores = new List<float>();
    public static int maxScores = 10;

    public static void Save(float score)
    {
        scores.Add(score);
        scores = scores.OrderByDescending(p => p).Reverse().ToList();
        scores = scores.Take(maxScores).ToList();
        for (int i = 0; i < scores.Count; i++)
        {
            float s = scores[i];
            PlayerPrefs.SetFloat("score["+i+"]", s);
        }
    }

    public static void Load()
    {
        for (int i = 0; i < maxScores; i++)
        {
            float score = PlayerPrefs.GetFloat("score[" + i + "]");
            if(score > 0) scores.Add(score);
        }

        scores = scores.OrderByDescending(p => p).Reverse().ToList();
    }
}
