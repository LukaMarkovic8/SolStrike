using UnityEngine;

public class bl_ASAGITeamBaseArea : MonoBehaviour
{
    public Team TeamBase = Team.Team1;
    public float safeDistance = 12f;

    public bool IsSafeForTeam(Team team)
    {
        var enemyPlayers = bl_GameManager.Instance.GetMFPSPlayerInTeam(team.OppsositeTeam());
        foreach (var player in enemyPlayers)
        {
            if (player == null || player.Actor == null || !player.isAlive) continue;

            if (!IsAreaSafe(player.Actor, safeDistance))
            {
                //  Debug.Log($"Player {player.Name} is near {TeamBase} base area, is not safe.");
                return false;
            }
        }

        return true;
    }

    public float DistanceToBaseArea(Transform enemyTransform)
    {
        // Get the enemy's position in world space
        Vector3 enemyPosition = enemyTransform.position;

        // Calculate the distance from the enemy to the center of the base area
        float distanceToCenter = Vector3.Distance(enemyPosition, transform.position);

        // Get the radius of the sphere (using the x scale as the diameter, assuming uniform scale for sphere)
        float radius = transform.localScale.x / 2f;

        // Return the distance from the enemy to the closest point on the sphere's surface
        return Mathf.Max(0, distanceToCenter - radius);
    }

    // Function to determine if the area is safe based on a given safe distance
    public bool IsAreaSafe(Transform enemyTransform, float safeDistance)
    {
        float distance = DistanceToBaseArea(enemyTransform);
        return distance > safeDistance;
    }

    void OnDrawGizmos()
    {
        // Save the current Gizmo color
        Color previousColor = Gizmos.color;

        // Set Gizmo color
        Color gizmoColor = TeamBase.GetTeamColor();
        Gizmos.color = gizmoColor;

        // Draw a wire sphere representing the base area
        Gizmos.DrawWireSphere(transform.position, transform.localScale.x / 2f);
        gizmoColor.a = 0.2f;
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, transform.localScale.x / 2f);

        // Restore the previous Gizmo color
        Gizmos.color = previousColor;

#if UNITY_EDITOR
        // Draw a label with the team name at the transform position
        UnityEditor.Handles.Label(transform.position, $"{TeamBase} Base");
#endif
    }
}
