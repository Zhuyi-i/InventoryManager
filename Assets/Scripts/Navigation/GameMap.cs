using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameMap", menuName = "Navigation/GameMap")]
public class GameMap : ScriptableObject
{
    public GameObject prefab;
    public string mapName;
    public int mapID;
    public List<MapEntryPoint> entryPoints;
}

[Serializable]
public class MapEntryPoint
{
    public int entryPointID;
}
