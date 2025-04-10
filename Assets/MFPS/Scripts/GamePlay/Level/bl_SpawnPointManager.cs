using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFPS.Internal.BaseClass;

public class bl_SpawnPointManager : MonoBehaviour
{
    public SpawnMode spawnMode = SpawnMode.Random;
    [LovattoToogle] public bool drawSpawnPoints = true;
    public float minSpawnDistance = 2.0f; // Minimum distance between players when spawning

    [Header("References")]
    public bl_KillCamBase killCameraInstance;
#if UNITY_EDITOR
    public Mesh SpawnPointPlayerGizmo;
#endif

    private List<bl_SpawnPointBase> spawnPoints = new List<bl_SpawnPointBase>();
    private Dictionary<Team, int> sequentialCounters = new Dictionary<Team, int>();
    private Dictionary<bl_SpawnPointBase, float> lastUsedTimes = new Dictionary<bl_SpawnPointBase, float>();
    private float spawnPointCooldown = 5.0f; // Seconds before a spawn point can be reused

    /// <summary>
    /// Initialize spawn manager
    /// </summary>
    void Awake()
    {
        sequentialCounters[Team.Team1] = -1;
        sequentialCounters[Team.Team2] = -1;
        sequentialCounters[Team.All] = -1;
    }

    /// <summary>
    /// Add a spawn point to the manager
    /// </summary>
    public static void AddSpawnPoint(bl_SpawnPointBase point)
    {
        if (Instance == null) return;

        if (Instance.spawnPoints.Contains(point)) return;
        Instance.spawnPoints.Add(point);
        Instance.lastUsedTimes[point] = -Instance.spawnPointCooldown; // Mark as available immediately
    }

    /// <summary>
    /// Get the position and rotation to instance the player from one of the team spawn points in the scene
    /// </summary>
    public bool GetPlayerSpawnPosition(Team team, out Vector3 position, out Quaternion rotation)
    {
        var point = GetValidSpawnPointForTeam(team);
        if (point == null)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            Debug.LogWarning($"Couldn't find valid spawnpoint for team {team.ToString()} in this scene.");
            return false;
        }

        point.GetSpawnPosition(out position, out rotation);
        lastUsedTimes[point] = Time.time; // Mark this spawn point as used
        return true;
    }

    /// <summary>
    /// Get a valid spawn point with safety checks
    /// </summary>
    private bl_SpawnPointBase GetValidSpawnPointForTeam(Team team)
    {
        // First try the preferred spawn mode
        var point = GetSpawnPointForTeam(team, spawnMode);

        // If the point is invalid or recently used, try alternatives
        if (!IsSpawnPointValid(point))
        {
            // Try other spawn modes as fallback
            var fallbackMode = spawnMode == SpawnMode.Random ? SpawnMode.Sequential : SpawnMode.Random;
            point = GetSpawnPointForTeam(team, fallbackMode);

            // If still invalid, try any available point
            if (!IsSpawnPointValid(point))
            {
                point = GetAnyAvailableSpawnPoint(team);
            }
        }

        return point;
    }

    /// <summary>
    /// Check if a spawn point is valid (not recently used and safe location)
    /// </summary>
    private bool IsSpawnPointValid(bl_SpawnPointBase point)
    {
        if (point == null) return false;

        // Check if spawn point is on cooldown
        if (lastUsedTimes.TryGetValue(point, out float lastUsed))
        {
            if (Time.time - lastUsed < spawnPointCooldown) return false;
        }

        // Check if spawn area is clear of other players
        Vector3 position;
        Quaternion rotation;
        point.GetSpawnPosition(out position, out rotation);

        return IsSpawnAreaClear(position);
    }

    /// <summary>
    /// Check if spawn area is clear of other players
    /// </summary>
    private bool IsSpawnAreaClear(Vector3 position)
    {
        if (bl_GameManager.Instance == null) return true;

        foreach (var player in bl_GameManager.Instance.OthersActorsInScene)
        {
            if (player.isAlive && player.Actor != null)
            {
                float distance = Vector3.Distance(position, player.Actor.position);
                if (distance < minSpawnDistance)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Get any available spawn point for the team (last resort)
    /// </summary>
    private bl_SpawnPointBase GetAnyAvailableSpawnPoint(Team team)
    {
        var teamPoints = GetListOfPointsForTeam(team);
        if (teamPoints == null || teamPoints.Count <= 0) return null;

        // Try to find the least recently used spawn point
        bl_SpawnPointBase bestPoint = null;
        float oldestTime = float.MaxValue;

        foreach (var point in teamPoints)
        {
            if (lastUsedTimes.TryGetValue(point, out float lastUsed))
            {
                if (lastUsed < oldestTime)
                {
                    oldestTime = lastUsed;
                    bestPoint = point;
                }
            }
            else
            {
                // Never used point is ideal
                return point;
            }
        }

        return bestPoint;
    }

    /// <summary>
    /// Get the spawnpoint from all the registered points based in the default selector mode. 
    /// </summary>
    public bl_SpawnPointBase GetSpawnPointForTeam(Team team) => GetSpawnPointForTeam(team, spawnMode);

    /// <summary>
    /// Get the spawnpoint from all the registered points based in the given selector mode. 
    /// </summary>
    public bl_SpawnPointBase GetSpawnPointForTeam(Team team, SpawnMode m_spawnMode)
    {
        if (spawnPoints.Count <= 0)
        {
            var all = FindObjectsOfType<bl_SpawnPointBase>();
            if (all.Length > 0)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    all[i].Initialize();
                }
            }
            else
            {
                Debug.LogWarning("There's not spawnpoints in this scene.");
                return null;
            }
        }

        switch (m_spawnMode)
        {
            case SpawnMode.Random:
            default:
                return GetRandomSpawnPoint(team);
            case SpawnMode.Sequential:
                return GetSequentialSpawnPoint(team);
        }
    }

    /// <summary>
    /// Get a random spawn point for the team
    /// </summary>
    public bl_SpawnPointBase GetRandomSpawnPoint(Team team)
    {
        var teamPoints = GetListOfPointsForTeam(team);
        if (teamPoints == null || teamPoints.Count <= 0) return null;

        // Try up to 10 times to find a valid random point
        for (int i = 0; i < 10; i++)
        {
            var point = teamPoints[Random.Range(0, teamPoints.Count)];
            if (IsSpawnPointValid(point)) return point;
        }

        // If no valid point found after tries, return any point
        return teamPoints[Random.Range(0, teamPoints.Count)];
    }

    /// <summary>
    /// Get the next spawn point in sequence for the team
    /// </summary>
    public bl_SpawnPointBase GetSequentialSpawnPoint(Team team)
    {
        var teamPoints = GetListOfPointsForTeam(team);
        if (teamPoints == null || teamPoints.Count <= 0) return null;

        // Find next valid sequential point
        for (int i = 0; i < teamPoints.Count; i++)
        {
            sequentialCounters[team] = (sequentialCounters[team] + 1) % teamPoints.Count;
            var point = teamPoints[sequentialCounters[team]];
            if (IsSpawnPointValid(point)) return point;
        }

        // If no valid point found in sequence, return next in sequence anyway
        sequentialCounters[team] = (sequentialCounters[team] + 1) % teamPoints.Count;
        return teamPoints[sequentialCounters[team]];
       
    }
    
    /// <summary>
    /// Get the list of all the spawnpoints available for the given team
    /// </summary>
    public List<bl_SpawnPointBase> GetListOfPointsForTeam(Team team)
    {
        if (team == Team.None) team = Team.All;

        var teamPoints = spawnPoints.FindAll(x => x.team == team);
        if (teamPoints.Count <= 0)
        {
            Debug.LogWarning("There's not spawnpoints for the team: " + team.GetTeamName());
            return null;
        }
        return teamPoints;
    }

    /// <summary>
    /// Get a completely random spawn point from any team
    /// </summary>
    public bl_SpawnPointBase GetSingleRandom() => spawnPoints[Random.Range(0, spawnPoints.Count)];

    [System.Serializable]
    public enum SpawnMode
    {
        Random = 1,
        Sequential = 2,
    }

    private static bl_SpawnPointManager _instance;
    public static bl_SpawnPointManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_SpawnPointManager>();
            }
            return _instance;
        }
    }
}