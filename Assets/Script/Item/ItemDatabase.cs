using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemRow
{
    public int id;
    public string name;
    public string description;
    public float effectAmount;
    public float duration;    
}

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance; 
    private Dictionary<int, ItemRow> itemDict = new Dictionary<int, ItemRow>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadCSV();
    }

    void LoadCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("ItemData");
        if (csvFile == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다! Resources 폴더를 확인하세요.");
            return;
        }

       
        string[] lines = csvFile.text.Trim().Split('\n');

        for (int i = 2; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] row = line.Split(',');

            if (row.Length < 4) continue;

            ItemRow item = new ItemRow();
            item.id = int.Parse(row[0]);
            item.name = row[1];
            item.description = row[2];
            item.effectAmount = float.Parse(row[3]);

            if (!itemDict.ContainsKey(item.id))
                itemDict.Add(item.id, item);
        }
        Debug.Log($"아이템 로드 완료: {itemDict.Count}개");
    }

    public ItemRow GetItem(int id) => itemDict.ContainsKey(id) ? itemDict[id] : null;
}