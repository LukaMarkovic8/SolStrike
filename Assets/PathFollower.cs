using UnityEngine;
using System.Collections.Generic; // Required for using Lists

public class PathFollower : MonoBehaviour
{
    [Header("Path Settings")]
    [Tooltip("List of Transforms representing the waypoints in the path.")]
    public List<Transform> waypoints = new List<Transform>(); // List to hold the waypoints

    [Tooltip("Speed at which the object moves along the path.")]
    public float moveSpeed = 5f;

    [Tooltip("Speed at which the object rotates to face the next waypoint.")]
    public float rotationSpeed = 5f;

    [Tooltip("How close the object needs to be to a waypoint to consider it reached.")]
    public float proximityThreshold = 0.1f;

    [Header("Internal State (Read Only)")]
    [SerializeField] // Show private variable in inspector (optional)
    private int currentWaypointIndex = 0; // Index of the *next* waypoint to move towards

    void Start()
    {
        // Basic validation
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("PathFollower: Waypoints list is not assigned or empty!", this);
            enabled = false; // Disable the script if no waypoints are set
            return;
        }

        // Optional: Snap to the first waypoint on start
        // if (waypoints.Count > 0 && waypoints[0] != null)
        // {
        //     transform.position = waypoints[0].position;
        // }
    }

    void Update()
    {
        // Exit if waypoints list is invalid
        if (waypoints == null || waypoints.Count == 0) return;

        // Ensure the current index is valid (might happen if waypoints are removed during runtime)
        if (currentWaypointIndex < 0 || currentWaypointIndex >= waypoints.Count || waypoints[currentWaypointIndex] == null)
        {
            Debug.LogWarning("PathFollower: Current waypoint index is invalid or waypoint is null. Resetting.", this);
            currentWaypointIndex = 0; // Reset to start
            if (waypoints.Count == 0 || waypoints[currentWaypointIndex] == null)
            {
                Debug.LogError("PathFollower: Cannot find a valid starting waypoint.", this);
                enabled = false; // Disable if still invalid
                return;
            }
        }

        // --- Movement ---
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 targetPosition = targetWaypoint.position;

        // Move towards the target waypoint
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // --- Rotation (Smooth Turning) ---
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        // Only rotate if we are actually moving (direction is not zero)
        if (directionToTarget != Vector3.zero)
        {
            // Calculate the rotation needed to look at the target
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // --- Waypoint Check ---
        // Check if we've reached the current target waypoint
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget < proximityThreshold)
        {
            // Move to the next waypoint index
            currentWaypointIndex++;

            // Loop back to the beginning if we've gone past the last waypoint
            if (currentWaypointIndex >= waypoints.Count)
            {
                currentWaypointIndex = 0;
            }
        }
    }

    // Optional: Draw gizmos in the editor to visualize the path
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Transform current = waypoints[i];
            Transform next = waypoints[(i + 1) % waypoints.Count]; // Use modulo for looping connection

            if (current != null && next != null)
            {
                Gizmos.DrawSphere(current.position, 0.3f); // Draw sphere at waypoint
                Gizmos.DrawLine(current.position, next.position); // Draw line to next waypoint
            }
        }
    }
}