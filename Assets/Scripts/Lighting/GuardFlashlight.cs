using UnityEngine;

public class GuardFlashlight : MonoBehaviour
{
    [SerializeField] private float turnSpeed = 540f;

    private GuardAI guard;

    public void Init(GuardAI owner)
    {
        guard = owner;
        transform.rotation = Quaternion.Euler(0f, 0f, TargetAngle());
    }

    private void LateUpdate()
    {
        if (guard == null) return;

        float current = transform.eulerAngles.z;
        float angle = Mathf.MoveTowardsAngle(current, TargetAngle(), turnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private float TargetAngle()
    {
        Vector2 facing = guard.FacingDirection;
        if (facing.sqrMagnitude < 0.001f) facing = Vector2.right;
        return Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f;
    }
}
