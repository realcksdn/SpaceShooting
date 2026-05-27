using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RankingScene에 추가. SimpleRanking과 동일한 PlayerPrefs 저장 방식 사용.
/// Inspector 연결:
///   - rowTexts : Name/Score Text 쌍 (10줄이면 10개)
///   - currentScoreText : "Current Score" 텍스트
///   - nameInput : 이름 InputField
///   - submitButton : 등록 버튼
/// </summary>
public class RankingManager : MonoBehaviour
{
    [Header("랭킹 목록 (Name Text, Score Text 쌍으로 연결)")]
    public Text[] nameTexts;   // 위에서부터 순서대로 연결
    public Text[] scoreTexts;

    [Header("현재 점수 표시")]
    public Text currentScoreText;

    [Header("이름 입력 + 등록 버튼")]
    public InputField nameInput;
    public Button submitButton;

    // SimpleRanking과 동일한 내부 구조
    [Serializable] private class RankData { public string name; public int score; }
    [Serializable] private class RankWrapper { public List<RankData> list = new(); }

    private const string PREF_KEY = "RankWrapper";
    private int currentScore;

    void Start()
    {
        currentScore = GameManager.Instance != null ? GameManager.Instance.score : 0;

        if (currentScoreText != null)
            currentScoreText.text = "Current Score : " + currentScore;

        RefreshList();
    }

    /// <summary>등록 버튼 OnClick에 연결</summary>
    public void OnClickSubmit()
    {
        string playerName = nameInput != null ? nameInput.text.Trim() : "Player";
        if (string.IsNullOrEmpty(playerName)) playerName = "Player";

        // 기존 SimpleRanking과 동일한 방식으로 저장
        var wrapper = LoadWrapper();
        wrapper.list.Add(new RankData { name = playerName, score = currentScore });
        wrapper.list.Sort((a, b) => b.score.CompareTo(a.score)); // 높은 점수 먼저
        if (wrapper.list.Count > 10) wrapper.list.RemoveRange(10, wrapper.list.Count - 10);
        SaveWrapper(wrapper);

        RefreshList();

        // 등록 후 메뉴로
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMenu();
    }

    void RefreshList()
    {
        var wrapper = LoadWrapper();

        for (int i = 0; i < nameTexts.Length; i++)
        {
            if (i < wrapper.list.Count)
            {
                if (nameTexts[i]  != null) nameTexts[i].text  = wrapper.list[i].name;
                if (scoreTexts[i] != null) scoreTexts[i].text = wrapper.list[i].score.ToString();
            }
            else
            {
                if (nameTexts[i]  != null) nameTexts[i].text  = "-";
                if (scoreTexts[i] != null) scoreTexts[i].text = "-";
            }
        }
    }

    RankWrapper LoadWrapper()
    {
        var json = PlayerPrefs.GetString(PREF_KEY, "");
        return string.IsNullOrWhiteSpace(json) ? new RankWrapper() : JsonUtility.FromJson<RankWrapper>(json);
    }

    void SaveWrapper(RankWrapper wrapper)
    {
        PlayerPrefs.SetString(PREF_KEY, JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }
}
