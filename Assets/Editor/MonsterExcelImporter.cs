using UnityEditor;
using System.IO;
using ExcelDataReader;
using UnityEngine;
using System.Data;

public class MonsterExcelImporter
{
    [MenuItem("Tools/Import Monster Table")]
    public static void Import()
    {
        string filePath = Application.dataPath + "/Data/Game.xlsx";

        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                var sheet = result.Tables[0]; // 첫 번째 'Monster' 시트 선택
                var table = ScriptableObject.CreateInstance<MonsterTable>();

                // 7행부터 데이터가 시작되므로 i = 6
                for (int i = 6; i < sheet.Rows.Count; i++)
                {
                    var row = sheet.Rows[i];

                    // ID가 비어있으면 스킵
                    if (row[0] == null || string.IsNullOrWhiteSpace(row[0].ToString())) continue;
                    if (!int.TryParse(row[0].ToString(), out int id)) continue;

                    MonsterData data = new MonsterData();
                    data.monsterId = id;                                 // A열
                    data.name = row[1]?.ToString();                      // B열
                    data.prefabKey = row[2]?.ToString();                  // C열

                    if (int.TryParse(row[3]?.ToString(), out int hp)) data.maxHp = hp;      // D열
                    if (int.TryParse(row[4]?.ToString(), out int atk)) data.atk = atk;     // E열
                    if (int.TryParse(row[5]?.ToString(), out int def)) data.def = def;     // F열
                    if (float.TryParse(row[6]?.ToString(), out float speed)) data.moveSpeed = speed; // G열
                    if (int.TryParse(row[7]?.ToString(), out int exp)) data.expReward = exp; // H열

                    table.monsterList.Add(data);
                }

                AssetDatabase.CreateAsset(table, "Assets/Data/MonsterTable.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("MonsterTable 생성 완료!");
            }
        }
    }
}