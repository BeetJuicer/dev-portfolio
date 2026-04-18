namespace StateMachineCore
{
using UnityEngine;

public class Movement_Kinematic : MonoBehaviour, IMovable2D
{
    private float speed;
    [Header("Detection")]
    [SerializeField] private Vector2 boxCastSize;
    [SerializeField] private LayerMask whatIsObstacle;
    [SerializeField] private Transform castPos;
    [SerializeField] private float cornerCorrection = 0.1f;

    private Rigidbody2D rb;
    private Collider2D col;

    public Vector3 MoveDirection => moveDirection;
    private Vector3 moveDirection;

    public Vector3 LastMoveDirection => lastMoveDirection;
    private Vector3 lastMoveDirection;
    public int FacingDirection => facingDirection;

    public Vector3 Velocity => throw new System.NotImplementedException();

        public float GravityScale => throw new System.NotImplementedException();

        public bool IsMoving => throw new System.NotImplementedException();

        private int facingDirection;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        col = GetComponent<Collider2D>();
    }

    public void Move(Vector3 moveAmount)
    {
        if (moveAmount.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveAmount.x), transform.localScale.y, transform.localScale.z);
            facingDirection = (int)Mathf.Sign(moveAmount.x);
            lastMoveDirection = moveAmount.normalized;
        }

        moveDirection = moveAmount.normalized;


        Vector2 adjustedMove = GetAdjustedMovement(moveAmount);
        rb.MovePosition(rb.position + adjustedMove);
    }

    //Returns a move amount where a direction that will hit an obstacle is set to 0
    private Vector2 GetAdjustedMovement(Vector2 moveAmount)
    {
        Vector2 xMove = new Vector2(moveAmount.x, 0);
        Vector2 yMove = new Vector2(0, moveAmount.y);

        Vector2 correction = Vector2.zero;
        bool canMoveX = CanMove(xMove.normalized, Mathf.Abs(moveAmount.x), ref correction);
        bool canMoveY = CanMove(yMove.normalized, Mathf.Abs(moveAmount.y), ref correction);

        // Apply correction as part of the final move, not separately
        return new Vector2(
            canMoveX ? xMove.x : 0,
            canMoveY ? yMove.y : 0
        ) + correction;
    }

    private RaycastHit2D BoxCastIgnoreSelf(Vector2 origin, Vector2 size, float angle, Vector2 dir, float distance)
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, angle, dir, distance, whatIsObstacle);
        foreach (var hit in hits)
        {
            if (hit.collider != col)
                return hit;
        }
        return default;
    }

    private bool CanMove(Vector2 direction, float distance, ref Vector2 correction)
    {
        RaycastHit2D hit = BoxCastIgnoreSelf(castPos.position, boxCastSize, 0f, direction, distance);
        if (hit.collider == null) return true;

        Vector2 perpendicular = new Vector2(direction.y, -direction.x);

        RaycastHit2D hitRight = BoxCastIgnoreSelf(
            (Vector2)castPos.position + perpendicular * cornerCorrection,
            boxCastSize, 0f, direction, distance);
        if (hitRight.collider == null)
        {
            correction += perpendicular * cornerCorrection; // accumulate, don't MovePosition
            return true;
        }

        RaycastHit2D hitLeft = BoxCastIgnoreSelf(
            (Vector2)castPos.position - perpendicular * cornerCorrection,
            boxCastSize, 0f, direction, distance);
        if (hitLeft.collider == null)
        {
            correction -= perpendicular * cornerCorrection;
            return true;
        }

        return false;
    }
}
}