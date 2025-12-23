using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
public class protypes_manager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI tipText;

    private const string USED_TIPS_KEY = "used_tip_ids";

    private TipCollection tipCollection;
    private HashSet<int> usedTipIds = new HashSet<int>();

    private void Start()
    {
        LoadUsedTips();
        LoadJson();
        ShowRandomUnusedTip();
    }

    // -------------------------
    // LOAD JSON
    // -------------------------
    private void LoadJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("pro_tips_json");

        if (jsonFile == null)
        {
            Debug.LogError("❌ pro_tips.json not found in Resources!");
            return;
        }

        tipCollection = JsonUtility.FromJson<TipCollection>(jsonFile.text);
    }

    // -------------------------
    // SHOW RANDOM TIP (NO REPEAT)
    // -------------------------
    private void ShowRandomUnusedTip()
    {
        if (tipCollection == null || tipCollection.tipss == null)
            return;

        // ✅ Filter valid (non-empty) tips
        var validTips = tipCollection.tipss
            .Where(t =>
                !string.IsNullOrWhiteSpace(t.text) &&
                t.text != "IT IS BLANK"
            )
            .ToList();

        // Remove already-used tips
        var unusedTips = validTips
            .Where(t => !usedTipIds.Contains(t.id))
            .ToList();

        // If all tips used → reset
        if (unusedTips.Count == 0)
        {
            usedTipIds.Clear();
            PlayerPrefs.DeleteKey(USED_TIPS_KEY);
            unusedTips = validTips;
        }

        // Pick random
        TipData selectedTip = unusedTips[Random.Range(0, unusedTips.Count)];

        // Display
        tipText.text = selectedTip.text;

        // Save used
        usedTipIds.Add(selectedTip.id);
        SaveUsedTips();
    }

    // -------------------------
    // SAVE / LOAD USED IDS
    // -------------------------
    private void SaveUsedTips()
    {
        PlayerPrefs.SetString(USED_TIPS_KEY, string.Join(",", usedTipIds));
        PlayerPrefs.Save();
    }

    private void LoadUsedTips()
    {
        if (!PlayerPrefs.HasKey(USED_TIPS_KEY))
            return;

        string saved = PlayerPrefs.GetString(USED_TIPS_KEY);
        string[] ids = saved.Split(',');

        foreach (string id in ids)
        {
            if (int.TryParse(id, out int parsed))
                usedTipIds.Add(parsed);
        }
    }
}
[System.Serializable]
public class TipData
{
    public int id;
    public string text;
}

[System.Serializable]
public class TipCollection
{
    public List<TipData> tipss;
}