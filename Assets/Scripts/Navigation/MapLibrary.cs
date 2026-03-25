using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapLibrary", menuName = "Navigation/MapLibrary")]
public class MapLibrary : ScriptableObject
{
    public List<GameMap> mapLibrary;
}
