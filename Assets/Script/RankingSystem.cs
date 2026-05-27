using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRanking : MonoBehaviour
{
    [Serializable]
    private class RankData
    {
        public string name;
        public int score;

        public RankData(string name, int score)
        {
            this.name = name;
            this.score = score;
        }
    }

    [Serializable]
    private class RankWrapper
    {
        private const int maxCount = 5;
        public List<RankData> list = new();

        public void Add(RankData data)
        {
            list.Add(data);
            Sort();
        }

        private void Sort()
        {
            list.Sort((a, b) => a.score.CompareTo(b.score));
            list = list.GetRange(0, Mathf.Min(list.Count, maxCount));
        }
    }


    [SerializeField] private RankWrapper _rankingData = new();

    private void Save()
    {
        var json = JsonUtility.ToJson(_rankingData);
        PlayerPrefs.SetString(nameof(RankWrapper), json);
    }

    private void Load()
    {
        var data = PlayerPrefs.GetString(nameof(RankWrapper), "");

        if (string.IsNullOrWhiteSpace(data))
            _rankingData = new();
        else
            _rankingData = JsonUtility.FromJson<RankWrapper>(data);
    }


    void Awake()
    {
        Load();
    }

    /// <summary>이름과 점수를 추가하고 저장</summary>
    public void AddEntry(string playerName, int score)
    {
        _rankingData.Add(new RankData(playerName, score));
        Save();
    }

}