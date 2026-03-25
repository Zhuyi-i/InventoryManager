using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MapNavigation : MonoBehaviour
{
    public static MapNavigation Instance;

    [Header("References — safe to assign, never goes null")]
    [SerializeField] private MapLibrary library;

    [Header("Runtime names/tags (found fresh each scene load)")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string mapParentName = "MapParent";
    [SerializeField] private string faderName = "TransitionFader";

    [Header("Starting location")]
    [SerializeField] private int startMapID = 0;
    [SerializeField] private int startDoorID = 0;


    private Transform _player;
    private Transform _mapParent;
    private TransitionFader _fader;

    private Dictionary<int, MapData> _mapDict = new Dictionary<int, MapData>();
    private Dictionary<int, RoomConnection> _doors = new Dictionary<int, RoomConnection>();
    private GameObject _currentMapGO;
    private bool _transitioning = false;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        BuildMapDictionary();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSceneReferences();

        if (_mapParent == null) return;

        int resumeMap = startMapID;
        int resumeDoor = startDoorID;

        var save = GameSaveManager.Instance?.GetCurrentSave();
        if (save != null && !save.isNewGame)
        {
            resumeMap = save.currentMapID;
            resumeDoor = save.spawnDoorID;
        }

        LoadMapImmediate(resumeMap, resumeDoor);
    }

    private void RefreshSceneReferences()
    {
        var playerGO = GameObject.FindGameObjectWithTag(playerTag);
        _player = playerGO != null ? playerGO.transform : null;

        var parentGO = GameObject.Find(mapParentName);
        _mapParent = parentGO != null ? parentGO.transform : null;

        var faderGO = GameObject.Find(faderName);
        _fader = faderGO != null ? faderGO.GetComponent<TransitionFader>() : null;

        if (_player == null)
            Debug.Log($"MapNavigation: '{playerTag}' not found in '{SceneManager.GetActiveScene().name}' — OK if main menu.");
        if (_mapParent == null)
            Debug.Log($"MapNavigation: '{mapParentName}' not found in '{SceneManager.GetActiveScene().name}' — OK if main menu.");
        if (_fader == null)
            Debug.Log($"MapNavigation: '{faderName}' not found in '{SceneManager.GetActiveScene().name}' — OK if main menu.");
    }

    public void ResetMap()
    {
        _doors.Clear();
        if (_currentMapGO != null)
        {
            Destroy(_currentMapGO);
            _currentMapGO = null;
        }
        _transitioning = false;
    }


    public void RegisterDoor(RoomConnection door) => _doors[door.doorID] = door;

    public void UnregisterDoor(RoomConnection door)
    {
        if (_doors.ContainsKey(door.doorID)) _doors.Remove(door.doorID);
    }


    public void GoToMap(int mapID, int doorID)
    {
        if (_transitioning) return;
        if (!_mapDict.ContainsKey(mapID))
        {
            Debug.LogError($"MapNavigation: mapID {mapID} not in library.");
            return;
        }
        StartCoroutine(TransitionToMap(mapID, doorID));
    }


    private IEnumerator TransitionToMap(int mapID, int doorID)
    {
        _transitioning = true;

        if (_fader != null)
        {
            bool done = false;
            _fader.FadeOut(() => done = true);
            yield return new WaitUntil(() => done);
        }

        SwapMap(mapID);
        yield return new WaitForEndOfFrame();
        Vector2 spawnPos = PlacePlayerAtDoor(doorID, false, mapID);

        GameSaveManager.Instance?.SaveOnMapEnter(
            mapID,
            SceneManager.GetActiveScene().name,
            doorID,
            spawnPos);


        if (_fader != null)
        {
            bool done = false;
            _fader.FadeIn(() => done = true);
            yield return new WaitUntil(() => done);
        }

        _transitioning = false;
    }


    private void LoadMapImmediate(int mapID, int doorID)
    {
        SwapMap(mapID);
        StartCoroutine(PlacePlayerNextFrame(doorID, mapID));
    }

    private IEnumerator PlacePlayerNextFrame(int doorID, int mapID)
    {
        yield return new WaitForEndOfFrame();
        PlacePlayerAtDoor(doorID, true, mapID);
    }


    private void SwapMap(int mapID)
    {
        _doors.Clear();
        if (_currentMapGO != null) Destroy(_currentMapGO);
        _currentMapGO = Instantiate(_mapDict[mapID].prefab, _mapParent);
    }

    private Vector2 PlacePlayerAtDoor(int doorID, bool usesSavedPosition, int mapID)
    {
        if (_player == null)
        {
            Debug.LogWarning("MapNavigation: Player reference is null — can't place player.");
            return Vector2.zero;
        }

        var rb = _player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Only use saved position on initial load AND if it was saved on this exact map
        if (usesSavedPosition)
        {
            var save = GameSaveManager.Instance?.GetCurrentSave();
            if (save != null && save.hasPositionSave && !save.isNewGame
                && save.currentMapID == mapID)
            {
                _player.position = new Vector2(save.playerX, save.playerY);
                Debug.Log($"MapNavigation: Restored player to ({save.playerX}, {save.playerY}) on map {mapID}");
                return _player.position;
            }
        }

        if (_doors.TryGetValue(doorID, out RoomConnection door))
        {
            _player.position = door.transform.position;
            return _player.position;
        }

        Debug.LogWarning($"MapNavigation: doorID {doorID} not found — player not moved.");
        return _player.position;
    }

    private void BuildMapDictionary()
    {
        _mapDict.Clear();
        if (library == null) { Debug.LogError("MapNavigation: MapLibrary not assigned!"); return; }
        foreach (GameMap m in library.mapLibrary)
            _mapDict[m.mapID] = new MapData(m);
    }
}

public class MapData
{
    public GameObject prefab;
    public string mapName;
    public int mapID;
    public Dictionary<int, MapEntryPoint> entryPoints = new Dictionary<int, MapEntryPoint>();

    public MapData(GameMap config)
    {
        prefab = config.prefab;
        mapID = config.mapID;
        mapName = config.mapName;
        foreach (var ep in config.entryPoints)
            entryPoints[ep.entryPointID] = ep;
    }
}