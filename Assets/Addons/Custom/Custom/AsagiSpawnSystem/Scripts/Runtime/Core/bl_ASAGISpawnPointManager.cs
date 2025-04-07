using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_ASAGISpawnPointManager : MonoBehaviour
{
    [Tooltip("The min distance of an enemy to the spawn point to consider them safe")]
    public float safeDistance = 10f;
    public TDMSpawnMode tDMSpawnMode = TDMSpawnMode.SafeTeammate;
    [SerializeField] private bl_ASAGITeamBaseArea[] teamBases = null;

    private bool initialSpawnDone = false;
    private int networkSpawnPointIndex = 0;
    private int networkSpawnPoint2Index = 0;
    private const byte NETWORK_CODE = 127;
    private bool firstShuffle = false;
    private readonly float checkDistance = 2f; // Distance to check from the teammate
    private readonly float groundCheckDistance = 1.5f; // Distance to check for ground
    private static System.Random rng = new System.Random();
    private List<bl_AIShooter> pendingSpawnBots = new();
    private bool hasRecervedPoint = false;
    private int reservedPointIndex, reservedPointIndex2 = 0;

    public enum TDMSpawnMode
    {
        SafeTeammate,
        SafeBase
    }

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        initialSpawnDone = false;
        bl_GameManager.Instance.overrideSpawnPlayerModel = SpawnPlayerModel;
        bl_GameManager.Instance.overrideRepawnLocalAfter = RespawnLocalAfter;
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPlayerEnter;
        InvokeRepeating(nameof(CheckPendingSpawns), 1, 1);
    }

    private void OnEnable()
    {
        bl_PhotonNetwork.AddNetworkCallback(NETWORK_CODE, OnNetworkMessage);
        bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
        bl_EventHandler.onLocalPlayerDeath += OnLocalDeath;
        bl_EventHandler.onRemotePlayerDeath += OnRemotePlayerDeath;
    }

    private void OnDisable()
    {
        bl_PhotonNetwork.RemoveNetworkCallback(OnNetworkMessage);
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
        bl_EventHandler.onLocalPlayerDeath -= OnLocalDeath;
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPlayerEnter;
        bl_EventHandler.onRemotePlayerDeath -= OnRemotePlayerDeath;
    }

    void OnNetworkMessage(ExitGames.Client.Photon.Hashtable data)
    {
        byte evCode = (byte)data[0];

        if (evCode == 0) // sync spawn point index
        {
            networkSpawnPointIndex = (int)data[1];
        }
        else if (evCode == 1) // sync spawn point 2 index
        {
            networkSpawnPoint2Index = (int)data[1];
        }
        else if (evCode == 2) // sync spawn point index for new player
        {
            networkSpawnPointIndex = (int)data[1];
            networkSpawnPoint2Index = (int)data[2];
        }
        else if (evCode == 3) // sync spawn point index after a player death
        {
            networkSpawnPointIndex = (int)data[1];
            networkSpawnPoint2Index = (int)data[2];

            if (bl_MFPS.LocalPlayer.MFPSActor.Name == (string)data[3])
            {
                hasRecervedPoint = true;
                reservedPointIndex = networkSpawnPointIndex;
                reservedPointIndex2 = networkSpawnPoint2Index;
                // Debug.Log($"Spawn point reserved after death: {reservedPointIndex}:{reservedPointIndex2}");
            }
        }
    }

    private void Update()
    {
        //VisualLog.Log("Pending", pendingSpawnBots.Count);
        /* if (Input.GetKeyDown(KeyCode.K))
         {

             var all = bl_GameManager.Instance.OthersActorsInScene;
             foreach (var a in all)
             {
                 if (!a.isRealPlayer) return;

                 var pr = a.ActorView.GetComponent<bl_PlayerReferences>();
                 var damege = new DamageData()
                 {
                     Damage = 100,
                     From = bl_MFPS.LocalPlayer.FullNickName(),
                     Cause = DamageCause.Player,
                     GunID = 0,
                     ActorViewID = bl_MFPS.LocalPlayerReferences.photonView.ViewID
                 };
                 pr.playerHealthManager.DoDamage(damege);
             }
             bl_MFPS.LocalPlayer.Suicide(false);
         }*/
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool SpawnPlayerModel(GameObject playerPefab, Team playerTeam)
    {
        GameMode currentMode = bl_RoomSettings.Instance.GetGameMode;
        if (currentMode == GameMode.FFA)
        {
            var spawnPoint = GetFFASpawnPoint();
            spawnPoint.GetSpawnPosition(out Vector3 pos, out Quaternion rot);
            SpawnPlayer(Team.All, pos, rot);
        }
        else if (currentMode == GameMode.TDM)
        {
            if (GetTDMSpawnPoint(playerTeam, out Vector3 pos, out Quaternion rot, false))
            {
                SpawnPlayer(playerTeam, pos, rot);
            }
            else
            {
                // if not suitable spawn point is found, we wait 1 second and try again
                this.InvokeAfter(1f, () => { SpawnPlayerModel(playerPefab, playerTeam); });
                return false;
            }
        }
        else
        {
            // default spawn logic
            bl_SpawnPointManager.Instance.GetPlayerSpawnPosition(playerTeam, out Vector3 pos, out Quaternion rot);

            SpawnPlayer(playerTeam, pos, rot, playerPefab);
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    private void SpawnPlayerModel(Team playerTeam)
    {
        GameMode currentMode = bl_RoomSettings.Instance.GetGameMode;
        if (currentMode == GameMode.FFA)
        {
            var spawnPoint = GetFFASpawnPoint();
            spawnPoint.GetSpawnPosition(out Vector3 pos, out Quaternion rot);
            SpawnPlayer(Team.All, pos, rot);
        }
        else if (currentMode == GameMode.TDM)
        {
            if (GetTDMSpawnPoint(playerTeam, out Vector3 pos, out Quaternion rot, false))
            {
                SpawnPlayer(playerTeam, pos, rot);
                SetActiveText(false);
            }
            else
            {
                // if not suitable spawn point is found, we wait 1 second and try again
                this.InvokeAfter(1f, () => { SpawnPlayerModel(playerTeam); });
                SetActiveText(true, "Waiting for a safe spawn point...");
            }
        }
        else
        {
            // default spawn logic
            bl_GameManager.Instance.overrideSpawnPlayerModel = null;
            bl_GameManager.Instance.SpawnPlayerModel(playerTeam);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void RespawnLocalAfter(float respawnTime, bool doFadeIn)
    {
        GameMode currentMode = bl_RoomSettings.Instance.GetGameMode;
        if (currentMode != GameMode.FFA && currentMode != GameMode.TDM)
        {
            // of other game modes, use the default respawn logic
            bl_GameManager.Instance.overrideRepawnLocalAfter = null;
            bl_GameManager.Instance.RespawnLocalPlayerAfter(respawnTime, doFadeIn);
            return;
        }

        if (respawnTime < 0) respawnTime = bl_GameData.Instance.PlayerRespawnTime;

        StartCoroutine(DoWait());
        IEnumerator DoWait()
        {
            respawnTime += Random.Range(-0.3f, 0.7f); // add a random time to the respawn time (to avoid players spawning at the same time)
            // wait for the respawn time
            yield return new WaitForSeconds(respawnTime);

            // check if the player can spawn based on the EV spawn system logic
            if (CanSpawnPlayer(bl_MFPS.LocalPlayer.Team))
            {
                // if the player can spawn, we hide the kill cam and spawn the player
                bl_KillCamBase.Instance.SetActive(false);
                // Debug.Log("Player can spawn, using EV system to spawning player.");
            }
            else
            {
                SetActiveText(true, "Waiting for a safe spawn point...");
                // if the player can't spawn, we wait 1 second and try again (repeat until the player can spawn)
                while (true)
                {
                    yield return new WaitForSeconds(1);
                    if (CanSpawnPlayer(bl_MFPS.LocalPlayer.Team))
                    {
                        bl_KillCamBase.Instance.SetActive(false);
                        break;
                    }
                }
                bl_NamePlateBase.BlockDraw = false;
            }

            bl_GameManager.Instance.SpawnPlayerModel(bl_MFPS.LocalPlayer.Team);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerTeam"></param>
    /// <returns></returns>
    public bool CanSpawnPlayer(Team playerTeam)
    {
        GameMode currentMode = bl_RoomSettings.Instance.GetGameMode;
        if (currentMode == GameMode.FFA)
        {
            return true;
        }
        else if (currentMode == GameMode.TDM)
        {
            return GetTDMSpawnPoint(playerTeam, out _, out _, false);
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Returns a spawn point for a Free For All game mode
    /// Integration: Have to be called from bl_AIMananger.Instance.SpawnBot
    /// </summary>
    /// <returns></returns>
    public bl_SpawnPointBase GetFFASpawnPoint()
    {
        var spawnPoints = bl_SpawnPointManager.Instance.GetListOfPointsForTeam(Team.All);
        bl_SpawnPointBase spawnPoint;

        if (!initialSpawnDone)
        {
            // make sure every player spawn in a different spawn point
            if (bl_PhotonNetwork.IsMasterClient)
            {
                // since the master client will be the first one to spawn, we shuffle the spawn points so the first player not always spawn in the same spot
                if (!firstShuffle && networkSpawnPointIndex == 0)// if not 0 it means someone already spawn
                {
                    networkSpawnPointIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
                    firstShuffle = true;
                }
            }
            networkSpawnPointIndex = (networkSpawnPointIndex + 1) % spawnPoints.Count;

            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add(0, (byte)0);
            data.Add(1, networkSpawnPointIndex);
            bl_PhotonNetwork.Instance.SendDataOverNetwork(NETWORK_CODE, data);

            spawnPoint = spawnPoints[networkSpawnPointIndex];
        }
        else
        {
            // if the initial spawn is done, the spawn logic changes, now we need to make sure
            // the player spawn in a safe spawn point

            var allPlayers = new List<MFPSPlayer>(bl_GameManager.Instance.OthersActorsInScene)
            {
                bl_GameManager.Instance.LocalActor
            };
            spawnPoint = GetSafeSpawnPoint(spawnPoints, allPlayers);
        }

        return spawnPoint;
    }

    public bool GetTDMSpawnPoint(Team playerTeam, out Vector3 position, out Quaternion rotation, bool forBot)
    {
        var spawnPoints = bl_SpawnPointManager.Instance.GetListOfPointsForTeam(playerTeam);
        bl_SpawnPointBase spawnPoint;

        if (!initialSpawnDone)
        {
            // make sure every player spawn in a different spawn point at the start of the match
            int spawnIndex = reservedPointIndex;
            int spawnIndex2 = reservedPointIndex2;
            if (!hasRecervedPoint)
            {
                IncreaseSpawnPointIndex(playerTeam, spawnPoints);
                spawnIndex = networkSpawnPointIndex;
                spawnIndex2 = networkSpawnPoint2Index;
            }

            spawnPoint = spawnPoints[playerTeam != Team.Team2 ? spawnIndex : spawnIndex2];
            spawnPoint.GetSpawnPosition(out position, out rotation);
        }
        else
        {
            // if the initial spawn is done, the spawn logic changes, now we need to make sure
            // the player spawn in a safe spawn point
            if (tDMSpawnMode == TDMSpawnMode.SafeTeammate)
            {
                return SafeTeamMateSpawnPoint(playerTeam, spawnPoints, out position, out rotation, forBot);
            }
            else
            {
                return SafeBaseSpawnPoint(playerTeam, spawnPoints, out position, out rotation, forBot);
            }
        }

        return true;
    }

    private bool SafeTeamMateSpawnPoint(Team playerTeam, List<bl_SpawnPointBase> spawnPoints, out Vector3 position, out Quaternion rotation, bool forBot)
    {
        // first, we look for a teammate to spawn near
        Transform teammate = SearchSafeTeamMate(bl_GameManager.Instance.GetMFPSPlayerInTeam(playerTeam), bl_GameManager.Instance.GetMFPSPlayerInTeam(playerTeam.OppsositeTeam()), out bool isBot);
        if (teammate == null)
        {
            // if not safe teammate found, we spawn in the base
            // but first we check if the base is safe

            int teamId = ((int)playerTeam) - 1;
            bl_ASAGITeamBaseArea teamBase = teamBases[teamId];
            // check if the base is safe
            if (teamBase.IsSafeForTeam(playerTeam))
            {
                int spawnIndex = reservedPointIndex;
                int spawnIndex2 = reservedPointIndex2;
                if (!hasRecervedPoint)
                {
                    IncreaseSpawnPointIndex(playerTeam, spawnPoints);
                    spawnIndex = networkSpawnPointIndex;
                    spawnIndex2 = networkSpawnPoint2Index;
                }
                else
                {
                    //  Debug.Log($"Spawned in reserved spawn point {spawnIndex}:{spawnIndex2}-{spawnPoints[spawnIndex]}");
                }
                var spawnPoint = spawnPoints[playerTeam != Team.Team2 ? spawnIndex : spawnIndex2];

                spawnPoint.GetSpawnPosition(out position, out rotation);
                Debug.Log("No friends is safe to spawn near, spawning in base.");
                return true;
            }
            else
            {

                // In this mode the players do not switch bases.

                // the last check will be for an very unique case where the player is a solo team player
                // and there are no teammates and no safe bases the player will spawn in the safest spawn point
                var teammates = bl_GameManager.Instance.GetMFPSPlayerInTeam(playerTeam);
                if (teammates.Length == 1)
                {
                    var allPlayers = new List<MFPSPlayer>(bl_GameManager.Instance.OthersActorsInScene);
                    spawnPoints = bl_SpawnPointManager.Instance.GetListOfPointsForTeam(Team.All);
                    var spawnPoint = GetSafeSpawnPoint(spawnPoints, allPlayers);
                    spawnPoint.GetSpawnPosition(out position, out rotation);
                    Debug.Log("Player is solo team and doesnt have safe base to spawn, spawning in the safest spawn point available.");
                    return true;
                }

                // if the team base is not safe either
                // we wait 1 second and try again

                if (!forBot) Debug.Log("No safe spawn point found, waiting 1 second and trying again.");

                position = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }
        }
        else
        {
            if (!TryGetSafeSpawnPointNearTeammate(teammate, out position, out rotation, isBot, forBot))
            {
                Debug.LogWarning("Couldnt find position near teammate!");
                return false;
            }
        }

        return true;
    }

    private bool SafeBaseSpawnPoint(Team playerTeam, List<bl_SpawnPointBase> spawnPoints, out Vector3 position, out Quaternion rotation, bool isBot)
    {
        bl_ASAGITeamBaseArea teamBase = teamBases[((int)playerTeam) - 1];
        if (teamBase.IsSafeForTeam(playerTeam))
        {
            int spawnIndex = reservedPointIndex;
            int spawnIndex2 = reservedPointIndex2;
            if (!hasRecervedPoint)
            {
                IncreaseSpawnPointIndex(playerTeam, spawnPoints);
                spawnIndex = networkSpawnPointIndex;
                spawnIndex2 = networkSpawnPoint2Index;
            }

            var spawnPoint = spawnPoints[playerTeam != Team.Team2 ? spawnIndex : spawnIndex2];
            spawnPoint.GetSpawnPosition(out position, out rotation);
            Debug.Log("Local player spawn in team base.");
            return true;
        }
        else
        {
            // if the team default base is not safe, check the enemy base
            Team oppositeTeam = playerTeam.OppsositeTeam();
            teamBase = teamBases[((int)oppositeTeam) - 1];
            if (teamBase.IsSafeForTeam(playerTeam))
            {
                spawnPoints = bl_SpawnPointManager.Instance.GetListOfPointsForTeam(oppositeTeam);
                int spawnIndex = reservedPointIndex;
                int spawnIndex2 = reservedPointIndex2;
                if (!hasRecervedPoint)
                {
                    IncreaseSpawnPointIndex(playerTeam, spawnPoints);
                    spawnIndex = networkSpawnPointIndex;
                    spawnIndex2 = networkSpawnPoint2Index;
                }

                var spawnPoint = spawnPoints[oppositeTeam != Team.Team2 ? spawnIndex : spawnIndex2];
                spawnPoint.GetSpawnPosition(out position, out rotation);
                Debug.Log("Local player can't spawn in team base, spawning in enemy base.");
                return true;
            }
        }

        // if no safe spawn point is found, try find a safe teammate spawn point
        Transform teammate = SearchSafeTeamMate(bl_GameManager.Instance.GetMFPSPlayerInTeam(playerTeam), bl_GameManager.Instance.GetMFPSPlayerInTeam(playerTeam.OppsositeTeam()), out bool isTeammateBot);
        if (teammate != null)
        {
            TryGetSafeSpawnPointNearTeammate(teammate, out position, out rotation, isTeammateBot, isBot);
            Debug.Log("No safe base to spawn, spawning near teammate.");
            return true;
        }

        // the last check will be for an very unique case where the player is a solo team player
        // and there are no teammates and no safe bases the player will spawn in the safest spawn point
        var teammates = bl_GameManager.Instance.GetMFPSPlayerInTeam(playerTeam);
        if (teammates.Length == 1)
        {
            var allPlayers = new List<MFPSPlayer>(bl_GameManager.Instance.OthersActorsInScene)
            {
                bl_GameManager.Instance.LocalActor
            };
            spawnPoints = bl_SpawnPointManager.Instance.GetListOfPointsForTeam(Team.All);
            var spawnPoint = GetSafeSpawnPoint(spawnPoints, allPlayers);
            spawnPoint.GetSpawnPosition(out position, out rotation);
            Debug.Log("Player is solo team and doesnt have safe base to spawn, spawning in the safest spawn point available.");
            return true;
        }

        Debug.Log("No safe spawn point found, waiting 1 second and trying again.");

        position = Vector3.zero;
        rotation = Quaternion.identity;
        return false;
    }

    public void SpawnPlayer(Team playerTeam, Vector3 pos, Quaternion rot, GameObject playerPrefab = null)
    {
        if (playerPrefab == null)
        {
            playerPrefab = bl_GameData.Instance.Player1.gameObject;
            playerPrefab = bl_GameData.Instance.Player2.gameObject;
        }

        if (!bl_GameManager.Instance.InstancePlayer(playerPrefab, pos, rot, playerTeam))
        {
            // if the player was not instanced
            return;
        }

        bl_GameManager.Instance.FirstSpawnDone = true;
        bl_GameManager.Instance.AfterSpawnSetup();
    }

    private void ReserveSpawnPoint()
    {
        hasRecervedPoint = true;
    }

    public bl_SpawnPointBase GetSafeSpawnPoint(List<bl_SpawnPointBase> spawnPoints, List<MFPSPlayer> players)
    {
        bl_SpawnPointBase safestSpawnPoint = null;
        float maxDistanceToClosestEnemy = 0f;

        foreach (bl_SpawnPointBase spawnPoint in spawnPoints)
        {
            float closestEnemyDistance = float.MaxValue;

            // Calculate distance to the closest enemy
            foreach (var enemy in players)
            {
                if (enemy.Actor == null || !enemy.isAlive) continue;

                float distance = Vector3.Distance(spawnPoint.transform.position, enemy.Actor.position);
                if (distance < closestEnemyDistance)
                {
                    closestEnemyDistance = distance;
                }
            }

            // Check if this spawn point is safe
            if (closestEnemyDistance >= safeDistance)
            {
                return spawnPoint;  // Found a completely safe spawn point
            }

            // If not safe, track the safest one
            if (closestEnemyDistance > maxDistanceToClosestEnemy)
            {
                maxDistanceToClosestEnemy = closestEnemyDistance;
                safestSpawnPoint = spawnPoint;
            }
        }

        // Return the safest spawn point if no completely safe one is found
        return safestSpawnPoint;
    }

    private Transform SearchSafeTeamMate(MFPSPlayer[] teammates, MFPSPlayer[] enemies, out bool isBot)
    {
        // suffle the teammates array to avoid always spawning in the same spot
        Shuffle(teammates);

        // probably some other filters to avoid too many players spawning in the same teammate would be good

        foreach (var teammate in teammates)
        {
            if (teammate.Actor == null || !teammate.isAlive) continue;

            // check if there are enemies near the teammate
            bool isSafe = true;
            foreach (var enemy in enemies)
            {
                if (enemy.Actor == null || !enemy.isAlive) continue;

                float distance = Vector3.Distance(teammate.Actor.position, enemy.Actor.position);
                if (distance < safeDistance)
                {
                    isSafe = false;
                    break;
                }
            }

            if (isSafe)
            {
                isBot = !teammate.isRealPlayer;
                return teammate.Actor;
            }
        }

        isBot = false;
        return null;
    }

    public bool TryGetSafeSpawnPointNearTeammate(Transform teammate, out Vector3 spawnPoint, out Quaternion rot, bool isTeammateBot, bool isForABot)
    {
        // Directions to check relative to the teammate's forward direction
        Vector3[] directions = new Vector3[]
        {
            -teammate.forward, // Behind
			teammate.right,    // Right
			teammate.forward,  // In front
			-teammate.right    // Left
		};

        Vector3 offset = Vector3.zero;
        if (isTeammateBot && !isForABot)
        {
            offset = new Vector3(0, -1.21f, 0);
        }
        else if (!isTeammateBot && isForABot)
        {
            offset = new Vector3(0, 1.21f, 0);
        }

        foreach (Vector3 direction in directions)
        {
            // Calculate the potential spawn position
            Vector3 potentialSpawnPoint = teammate.position + offset + (direction * checkDistance);

            // Check if the position is feasible (no obstacles and ground exists)
            if (IsFeasibleSpawnPoint(teammate.position, potentialSpawnPoint))
            {
                spawnPoint = potentialSpawnPoint;
                rot = teammate.rotation;
                return true;
            }
        }

        // If no feasible point is found, return false
        spawnPoint = teammate.position;
        rot = teammate.rotation;
        return false;
    }

    private bool IsFeasibleSpawnPoint(Vector3 teammatePosition, Vector3 potentialSpawnPoint)
    {
        // Check if there is ground beneath the potential spawn point
        if (!Physics.Raycast(potentialSpawnPoint, Vector3.down, out RaycastHit hit, groundCheckDistance, bl_GameData.TagsAndLayerSettings.EnvironmentOnly))
        {
            return false; // No ground detected
        }

        // Check if there is an obstacle between the teammate and the potential spawn point
        if (Physics.Linecast(teammatePosition, potentialSpawnPoint, bl_GameData.TagsAndLayerSettings.EnvironmentOnly))
        {
            return false; // Obstacle detected
        }

        // If no obstacles and ground is found, the point is feasible
        return true;
    }

    private int IncreaseSpawnPointIndex(Team team, List<bl_SpawnPointBase> spawnPoints)
    {
        var data = bl_UtilityHelper.CreatePhotonHashTable();

        if (team != Team.Team2)
        {
            networkSpawnPointIndex = (networkSpawnPointIndex + 1) % spawnPoints.Count;
            data.Add(0, (byte)0);
            data.Add(1, networkSpawnPointIndex);
        }
        else
        {
            networkSpawnPoint2Index = (networkSpawnPoint2Index + 1) % spawnPoints.Count;
            data.Add(0, (byte)1);
            data.Add(1, networkSpawnPoint2Index);
        }

        bl_PhotonNetwork.Instance.SendDataOverNetwork(NETWORK_CODE, data);
        return team != Team.Team2 ? networkSpawnPointIndex : networkSpawnPoint2Index;
    }

    private void SetActiveText(bool active, string text = "")
    {
        bl_UIReferences.Instance.SetWaitingPlayersText(text, active);
    }

    public bool EnqueueBotSpawn(bl_AIShooter agent, Team botTeam)
    {
        if (agent == null)
        {
            Debug.LogWarning("A waiting bot was destroyed");
            return false;
        }

        if (!pendingSpawnBots.Contains(agent))
        {
            pendingSpawnBots.Add(agent);
            Debug.Log($"Added {agent.AIName} to pending spawn bots.");
        }
        Debug.Log($"Not safe spawn point for {agent.AIName}, trying again in 1 sec");

        return true;
    }

    private void CheckPendingSpawns()
    {
        for (int i = pendingSpawnBots.Count - 1; i >= 0; i--)
        {
            var bot = pendingSpawnBots[i];
            if (pendingSpawnBots[i] == null)
            {
                pendingSpawnBots.RemoveAt(i);
                continue;
            }

            if (bl_AIMananger.Instance.SpawnBot(bot) != null)
            {
                Debug.Log($"{bot.AIName}, spawned after waiting.");

                pendingSpawnBots.RemoveAt(i);  // Remove bot safely by index
                bl_AIMananger.Instance.RemovePendingSpawnBot(bot);
            }
            else
            {
                Debug.Log($"{bot.AIName}, still waiting for a safe spawn point.");
            }
        }
    }

    void OnPlayerEnter(Player player)
    {
        if (bl_PhotonNetwork.IsMasterClient)
        {
            // sync the spawn point index for the new player
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add(0, (byte)2);
            data.Add(1, networkSpawnPointIndex);
            data.Add(2, networkSpawnPoint2Index);

            bl_PhotonNetwork.Instance.SendDataOverNetworkToPlayer(NETWORK_CODE, data, player);
        }
    }

    void OnLocalSpawn()
    {
        initialSpawnDone = true;
        bl_CrosshairBase.Instance.Block = false;
        bl_CrosshairBase.Instance.Show(true);
        hasRecervedPoint = false;
    }

    void OnLocalDeath()
    {
        if (bl_PhotonNetwork.IsMasterClient)
        {
            OnRemotePlayerDeath(bl_MFPS.LocalPlayer.MFPSActor);
        }
    }

    void OnRemotePlayerDeath(MFPSPlayer player)
    {
        if (bl_PhotonNetwork.IsMasterClient)
        {
            // to fix the issue with players killing each other at the same time and then spawning in the same spawn point
            // the Master Client increase the spawn point index for the players that died and then reserve that spawn point for them

            IncreaseSpawnPointIndex(Team.Team1, bl_SpawnPointManager.Instance.GetListOfPointsForTeam(Team.Team1));
            IncreaseSpawnPointIndex(Team.Team2, bl_SpawnPointManager.Instance.GetListOfPointsForTeam(Team.Team2));
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add(0, (byte)3);
            data.Add(1, networkSpawnPointIndex);
            data.Add(2, networkSpawnPoint2Index);
            data.Add(3, player.Name);

            bl_PhotonNetwork.Instance.SendDataOverNetwork(NETWORK_CODE, data);

            // Debug.Log($"Master Client reserved spawn point for: {player.Name}-{networkSpawnPointIndex}:{networkSpawnPoint2Index} - {bl_SpawnPointManager.Instance.GetListOfPointsForTeam(Team.Team1)[networkSpawnPointIndex].name}");
        }
    }

    public static void Shuffle<T>(T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }

    private static bl_ASAGISpawnPointManager _instance;
    public static bl_ASAGISpawnPointManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_ASAGISpawnPointManager>(); }
            return _instance;
        }
    }
}