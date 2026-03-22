using UnityEditor;
using System.IO;
using ExcelDataReader;
using UnityEngine;
using System.Data;

public class SpawnerExcelImporter
{
    [MenuItem("Tools/Import Spawner Table")]
    public static void Import()
    {
        // 1. 경로 설정 (Assets/Data 폴더 안에 Game.xlsx가 있어야 함)
        string filePath = Application.dataPath + "/Data/Game.xlsx";

        if (!File.Exists(filePath))
        {
            Debug.LogError("엑셀 파일을 찾을 수 없습니다: " + filePath);
            return;
        }

        // 2. 파일 읽기 시작
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();

                // [체크] 시트가 최소 2개 이상인지 확인 (0: Monster, 1: Spawner)
                if (result.Tables.Count < 2)
                {
                    Debug.LogError("엑셀에 두 번째 시트('Spawner')가 없습니다. 시트 순서를 확인하세요!");
                    return;
                }

                // 두 번째 시트(Spawner)를 가져옵니다.
                var sheet = result.Tables[1];
                var table = ScriptableObject.CreateInstance<SpawnerTable>();

                // 3. 데이터 파싱 (5행부터 데이터가 있으므로 i=4부터 시작)
                for (int i = 4; i < sheet.Rows.Count; i++)
                {
                    var row = sheet.Rows[i];

                    // [핵심 수정] 첫 번째 칸(SpawnerId)이 비어있으면 리스트에 넣지 않고 건너뜁니다.
                    if (row[0] == null || string.IsNullOrWhiteSpace(row[0].ToString())) continue;

                    // 숫자로 변환 시도 - 변환에 실패하면(제목줄 등) 리스트에 추가하지 않습니다.
                    if (!int.TryParse(row[0].ToString(), out int sId)) continue;

                    SpawnerData data = new SpawnerData();
                    data.spawnerId = sId; // 이미 위에서 성공했으므로 할당

                    // 나머지 데이터들도 안전하게 TryParse로 가져옵니다.
                    if (row[1] != null && int.TryParse(row[1].ToString(), out int mId))
                        data.monsterId = mId;

                    if (row[2] != null && float.TryParse(row[2].ToString(), out float sTime))
                        data.spawnStartTime = sTime;

                    if (row[3] != null && float.TryParse(row[3].ToString(), out float delay))
                        data.respawnDelay = delay;

                    if (row[4] != null && float.TryParse(row[4].ToString(), out float range))
                        data.range = range;

                    // 진짜 데이터만 리스트에 추가합니다.
                    table.spawnList.Add(data);
                }

                // 4. 에셋 파일 생성 및 저장
                if (!Directory.Exists(Application.dataPath + "/Data"))
                    Directory.CreateDirectory(Application.dataPath + "/Data");

                // 파일이 덮어씌워지도록 경로 설정
                string assetPath = "Assets/Data/SpawnerTable.asset";
                AssetDatabase.CreateAsset(table, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"임포트 완료! 총 {table.spawnList.Count}개의 데이터를 가져왔습니다.");
            }
        }
    }
}