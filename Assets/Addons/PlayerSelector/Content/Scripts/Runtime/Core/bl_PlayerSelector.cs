using MFPS.Addon.PlayerSelector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_PlayerSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ContentUI = null;
    [SerializeField] private GameObject PlayerOptionUI = null;
    [SerializeField] private Transform ListPanel = null;
    public RectTransform CenterReference;

    public static Action<bl_PlayerSelectorInfo> onEquippedPlayerSelected;
    public static Action<bl_PlayerSelectorInfo, Team> onPlayerListedInTeam;
    public bool isChangeOfTeam { get; set; }

    #region Private members
#if PSELECTOR
    private Team SelectTeam = Team.None;
#endif
    private bl_PlayerSelectorInfo Info;
    private bool isSelected = false;
    private List<GameObject> cacheList = new List<GameObject>();
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        PlayerOptionUI.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SelectPlayer(bl_PlayerSelectorInfo info)
    {
        Info = info;
        ContentUI.SetActive(false);
#if PSELECTOR
        SpawnSelectedPlayer(info, SelectTeam);
#endif
        bl_GameManager.Instance.IsLocalPlaying = true;
        isSelected = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerTeam"></param>
    /// <returns></returns>
    public bool TrySpawnSelectedPlayer(Team playerTeam)
    {
        if (bl_PlayerSelectorData.Instance.PlayerSelectorMode == bl_PlayerSelectorData.PSType.InMatch)
        {
            if (IsSelected && !isChangeOfTeam)
            {
                SpawnSelected(playerTeam);
                return true;
            }
            else
            {
                OpenSelection(playerTeam);
                return false;
            }
        }
        else
        {
            if (!bl_PhotonNetwork.OfflineMode)
                SpawnSelectedPlayer(bl_PlayerSelectorData.Instance.GetSelectedPlayerFromTeam(playerTeam), playerTeam);
            else
            {
                bl_GameManager.Instance.SpawnPlayerModel(playerTeam);
            }
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="team"></param>
    public void SpawnSelected(Team team)
    {
        ContentUI.SetActive(false);
#if PSELECTOR
        if (bl_PlayerSelectorData.Instance.PlayerSelectorMode == bl_PlayerSelectorData.PSType.InMatch)
        {
            SpawnSelectedPlayer(Info, SelectTeam);
        }
        else
        {
            bl_PlayerSelectorInfo playerInfo = bl_PlayerSelectorData.Instance.GetSelectedPlayerFromTeam(team);
            SpawnSelectedPlayer(playerInfo, team);
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public static GameObject GetBotForTeam(Team team)
    {
        if (!bl_PlayerSelectorData.Instance.RandomizeBots)
        {
            var bot = bl_GameData.Instance.BotTeam1;
            if (team == Team.Team2) bot = bl_GameData.Instance.BotTeam2;
            return bot.gameObject;
        }

        return bl_PlayerSelectorData.Instance.GetRandomBotForTeam(team);
    }

    public static void SpawnPreSelectedPlayer(Team playerTeam) => SpawnSelectedPlayer(bl_PlayerSelectorData.Instance.GetSelectedPlayerFromTeam(playerTeam), playerTeam);

    /// <summary>
    /// 
    /// </summary>
    public static void SpawnSelectedPlayer(bl_PlayerSelectorInfo info, Team playerTeam)
    {
        if (bl_PhotonNetwork.OfflineMode)
        {
            bl_GameManager.Instance.SpawnPlayerModel(playerTeam);
            return;
        }
        Vector3 pos;
        Quaternion rot;
        bl_SpawnPointManager.Instance.GetPlayerSpawnPosition(playerTeam, out pos, out rot);

        if (!bl_GameManager.Instance.InstancePlayer(info.Prefab, pos, rot, playerTeam)) return;

        bl_GameManager.Instance.AfterSpawnSetup();
        if (!bl_GameManager.Instance.FirstSpawnDone && bl_MatchInformationDisplay.Instance != null) { bl_MatchInformationDisplay.Instance.DisplayInfo(); }
        bl_GameManager.Instance.FirstSpawnDone = true;
        bl_CrosshairBase.Instance.Show(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OpenSelection(Team team)
    {
#if PSELECTOR
        SelectTeam = team;
#endif
        ListPanel.GetComponent<HorizontalLayoutGroup>().enabled = true;
        ListPanel.GetComponent<ContentSizeFitter>().enabled = true;

        if (cacheList != null)
        {
            foreach (var item in cacheList)
            {
                if (item == null) continue;
                Destroy(item);
            }
            cacheList.Clear();
        }

        if (team == Team.Team1)
        {
            for (int i = 0; i < bl_PlayerSelectorData.Instance.Team1Players.Count; i++)
            {

                bl_PlayerSelectorInfo operatorInfo = bl_PlayerSelectorData.Instance.GetPlayer(Team.Team1, i);
                if (operatorInfo.Unlockability.UnlockMethod == MFPS.Internal.Structures.MFPSItemUnlockability.UnlockabilityMethod.Hidden) continue;

                GameObject g = Instantiate(PlayerOptionUI);
                g.SetActive(true);
                g.GetComponent<bl_PlayerSelectorUI>().Set(operatorInfo, this);
                g.transform.SetParent(ListPanel, false);
                cacheList.Add(g);
            }
        }
        else if (team == Team.Team2)
        {
            for (int i = 0; i < bl_PlayerSelectorData.Instance.Team2Players.Count; i++)
            {
                bl_PlayerSelectorInfo operatorInfo = bl_PlayerSelectorData.Instance.GetPlayer(Team.Team2, i);
                if (operatorInfo.Unlockability.UnlockMethod == MFPS.Internal.Structures.MFPSItemUnlockability.UnlockabilityMethod.Hidden) continue;

                GameObject g = Instantiate(PlayerOptionUI);
                g.SetActive(true);
                g.GetComponent<bl_PlayerSelectorUI>().Set(operatorInfo, this);
                g.transform.SetParent(ListPanel, false);
                cacheList.Add(g);
            }
        }
        else
        {
            for (int i = 0; i < bl_PlayerSelectorData.Instance.FFAPlayers.Count; i++)
            {
                bl_PlayerSelectorInfo operatorInfo = bl_PlayerSelectorData.Instance.GetPlayer(Team.All, i);
                if (operatorInfo.Unlockability.UnlockMethod == MFPS.Internal.Structures.MFPSItemUnlockability.UnlockabilityMethod.Hidden) continue;

                GameObject g = Instantiate(PlayerOptionUI);
                g.SetActive(true);
                g.GetComponent<bl_PlayerSelectorUI>().Set(operatorInfo, this);
                g.transform.SetParent(ListPanel, false);
                cacheList.Add(g);
            }
        }
        isChangeOfTeam = false;
        ContentUI.SetActive(true);
        bl_UtilityHelper.LockCursor(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    public void DeleteAllBut(GameObject obj)
    {
        for (int i = 0; i < cacheList.Count; i++)
        {
            if (cacheList[i] != obj)
            {
                Destroy(cacheList[i]);
            }
        }
        cacheList.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GameObject GetPlayerModel()
    {
        if (Info == null)
        {
            return Info.Prefab;
        }
        else
        {
            Debug.Log("Any team selected yet");
            return null;
        }
    }

    public bool IsSelected { get { return isSelected; } }
    public static bool InMatch => bl_PlayerSelectorData.Instance.PlayerSelectorMode == bl_PlayerSelectorData.PSType.InMatch;
    public static bl_PlayerSelectorData Data => bl_PlayerSelectorData.Instance;

    private static bl_PlayerSelector _ps;
    public static bl_PlayerSelector Instance
    {
        get
        {
            if (_ps == null) { _ps = FindObjectOfType<bl_PlayerSelector>(); }

            if (_ps == null && bl_GameManager.Instance != null && InMatch)
            {
                _ps = bl_PlayerSelectorData.InitInMatchUI();
            }
            return _ps;
        }
    }
}