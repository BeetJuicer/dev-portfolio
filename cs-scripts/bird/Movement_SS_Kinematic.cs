using Assets.Projects.StateMachine.SideScroll.SS_States;
using System;
using UnityEngine;

namespace Assets.Projects.StateMachine.SideScroll
{
    public class Movement_SS_Kinematic : MonoBehaviour, IMovable2D, IJumpable, IWindRidable
    {
        #region Serialized Fields
        [Header("Movement")]
        [SerializeField] private float gravityAmount;
        [SerializeField] private float decelerationAmount;

        [Header("Ground Detection")]
        [SerializeField] private Transform groundCheckPos;
        [SerializeField] private Vector2 groundCheckSize;
        [SerializeField] private LayerMask whatIsGround;

        [SerializeField] private Vector2 wallCheckSize;
        #endregion

        #region Private Fields
        private Vector3 currentVelocity = Vector3.zero;
        private float gravityScale = 1f;
        private float decelerationScale;
        private float maxFallSpeed = 10f;
        private Wind currentWind;
        private float preWindGravityScale;
        #endregion

        #region IWindRidable Properties
        public bool IsRidingWind => currentWind != null;
        public Wind CurrentWind => currentWind;
        #endregion

        #region IMovable Properties
        private Vector3 lastMoveDirection;
        private Vector3 moveDirection;

        public int FacingDirection { get; private set; } = 1;
        public Vector3 MoveDirection => moveDirection;
        public Vector3 LastMoveDirection => lastMoveDirection;
        public Vector3 Velocity => currentVelocity;
        public float GravityScale => gravityScale;
        public bool IsMoving => currentVelocity.sqrMagnitude > 0.0001f;

        #endregion

        private Vector2 giz_targetPos;
        private float groundCheckHalfHeight;
        #region Unity Methods
        private void Start()
        {
        }

        //this code doesn't handle slopes yet. not necessary for demo.

        private void FixedUpdate()
        {
            float currentDeceleration = decelerationAmount * decelerationScale * Time.fixedDeltaTime;
            currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, 0f, currentDeceleration);

            Vector2 targetPos = (Vector2)transform.position + (Vector2)currentVelocity * Time.fixedDeltaTime;
            giz_targetPos = targetPos;

            RaycastHit2D hitX = Physics2D.BoxCast(
                transform.position,         
                wallCheckSize,                   
                0f,                         
                Vector2.right * Mathf.Sign(currentVelocity.x), 
                currentVelocity.magnitude * Time.fixedDeltaTime,
                whatIsGround                 
            );

            RaycastHit2D hitY = Physics2D.BoxCast(
                transform.position,         
                wallCheckSize,                   
                0f,                          
                Vector2.up * Mathf.Sign(currentVelocity.y),  
                currentVelocity.magnitude * Time.fixedDeltaTime, 
                whatIsGround                 
            );

            if (hitX)
                currentVelocity.x = 0;

            if (hitY)
            {
                if (currentVelocity.y < 0)
                {
                    // Snap the bottom of the groundCheck onto the detected ground. We do this because the hitY boxCast is affected by magnitude, it'll detect ground earlier than IsGrounded().
                    float newY = hitY.point.y + Vector2.Distance(transform.position, groundCheckPos.position) + groundCheckHalfHeight;
                    transform.position = new Vector2(transform.position.x, newY);
                }

                currentVelocity.y = 0;
            }

            if (!IsGrounded())
            {
                float currentGravity = gravityAmount * gravityScale * Time.fixedDeltaTime;
                currentVelocity.y = Mathf.MoveTowards(currentVelocity.y, -maxFallSpeed, currentGravity);
            }

            Vector2 newPos = transform.position + currentVelocity * Time.fixedDeltaTime;  
           transform.position = newPos;
        }
        #endregion

        #region IMovable
        public void AddVelocity(Vector3 velocity)
        {
            currentVelocity.x += velocity.x;
            currentVelocity.y += velocity.y;

            if (currentVelocity.x != 0f)
                FacingDirection = (int)Mathf.Sign(currentVelocity.x);
        }

        public void Move(Vector3 moveAmount) => throw new NotImplementedException();

        public void SetVelocityX(float velocityX)
        {
            currentVelocity.x = velocityX;
            if (velocityX != 0f)
                FacingDirection = (int)Mathf.Sign(velocityX);
        }

        public void SetVelocityY(float velocityY) => currentVelocity.y = velocityY;
        public void ClampVelocityY(float max) => currentVelocity.y = Mathf.Min(currentVelocity.y, max);
        public void StopMovement() => currentVelocity = Vector3.zero;
        public void SetGravityScale(float scale) => gravityScale = scale;
        public void SetDeceleration(float scale) => decelerationScale = scale;
        public void SetMaxFallSpeed(float speed) => maxFallSpeed = speed;
        #endregion

        #region IJumpable
        public bool IsGrounded() => Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0f, whatIsGround);
        public void Jump(float force) => currentVelocity.y += force;
        #endregion

        #region IWindRidable
        public void SetWind(Wind wind)
        {
            currentWind = wind;
        }

        public void ExitWind()
        {
            currentWind.ExitWind(this);
            currentWind = null;
        }

        public void EnterWind(Wind wind)
        {
            SetWind(wind);
            wind.RideWind(this, this);
        }
        #endregion

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, wallCheckSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        }

        public void SetVelocity(Vector3 velocity)
        {
            currentVelocity = velocity;
        }
    }
}