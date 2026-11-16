using UnityEngine;

public class LockOnTarget : MonoBehaviour
{
    [SerializeField] private Transform lockOnPoint;

    public Transform AimPoint =>
        lockOnPoint != null ? lockOnPoint : transform;

    public bool IsTargetable { get; private set; } = true;

    public void SetTargetable(bool value)
    {
        IsTargetable = value;
    }
}
