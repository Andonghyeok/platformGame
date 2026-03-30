using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "SpawnerTable", menuName = "Data/SpawnerTable")]
public class SpawnerTable : ScriptableObject
{
    public List<SpawnerData> spawnList = new List<SpawnerData>();
}