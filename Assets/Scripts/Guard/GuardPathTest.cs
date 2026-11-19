using UnityEngine;
using Pathfinding;

public class GuardPathTest : MonoBehaviour
{
    [SerializeField] private Transform target;

    private AIPath aiPath;

    private void Awake()
    {
        aiPath = GetComponent<AIPath>();
    }

    private void Start()
    {
        if (aiPath == null)
        {
            Debug.LogError("GuardPathTest: AIPath not found!");
            return;
        }

        if (target == null)
        {
            Debug.LogError("GuardPathTest: Target not assigned!");
            return;
        }

        aiPath.destination = target.position;
        aiPath.canMove = true;

        Debug.Log("GuardPathTest: Destination assigned: " + target.position);
    }
}