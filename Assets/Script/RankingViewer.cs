using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingViewer : MonoBehaviour
{
    [Header("순위 텍스트 (1~5위 순서대로 연결)")]
    public Text rank1Text;
    public Text rank2Text;
    public Text rank3Text;
    public Text rank4Text;
    public Text rank5Text;

    [Serializable]
    private class RankData
    {
        public string name;
        public int score;
    }

    [Serializable]
    private class RankWrapper
    {
        public List<RankData> list = new List<RankData>();
    }

    void OnEnable() => Refresh();

    public void Refresh()
    {
        var texts = new Text[] { rank1Text, rank2Text, rank3Text, rank4Text, rank5Text };

        var json = PlayerPrefs.GetString("RankWrapper", "");
        var entries = new List<RankData>();

        if (!string.IsNullOrEmpty(json))
        {
            var data = JsonUtility.FromJson<RankWrapper>(json);
            entries = data.list;
            entries.Sort((a, b) => b.score.CompareTo(a.score));
        }

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] == null) continue;
            if (i < entries.Count)
                texts[i].text = $"{i + 1}위  {entries[i].name}  {entries[i].score}점";
            else
                texts[i].text = $"{i + 1}위  --";
        }
    }
}
