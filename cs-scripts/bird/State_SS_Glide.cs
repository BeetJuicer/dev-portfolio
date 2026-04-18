using Assets.Projects.StateMachine;
using System;
using UnityEngine;

namespace StateMachineCore
{
    [System.Serializable]
    public class GlideStateData
    {
        [Header("Lift Settings")]
        [Tooltip("The minimum target upward velocity regardless of entry speed.")]
        public float minGoalY;

        [Tooltip("The maximum cap for target upward velocity when 'swooping'.")]
        public float maxGoalY;

        [Tooltip("The baseline upward acceleration.")]
        public float minLiftAccel;

        [Tooltip("The maximum possible upward acceleration based on entry speed.")]
        public float maxLiftAccel;

        [Header("Initial Y Velocity -> Lift Multipliers")]
        [Tooltip("Multiplies entry fall speed to determine the target upward velocity. Higher = higher 'swoops'.")]
        public float goalYMultiplier;

        [Tooltip("Multiplies entry fall speed to determine upward acceleration. Higher = faster 'snap' to the goal velocity.")]
        public float liftAccelMultiplier;

        [Tooltip("Multiplies entry fall speed to determine horizontal boost.")]
        public float entrySpeedMultiplier;
        public float maxEntryAdditionalSpeed;


        [Header("Speed")]
        [Tooltip("Base horizontal movement speed during the glide.")]
        public float moveSpeed;

        [Tooltip("How quickly the character reaches the glide move speed.")]
        public float acceleration;

        [Tooltip("Speed when turning directions")]
        public float turnAccelMultiplier;

        [Tooltip("Gravity scale applied to the character while in the glide state.")]
        public float gravityScale;
    }

    public class State_SS_Glide : AnimatedState<SidescrollerCharacterStateMachine>
    {
        private GlideStateData glideData;

        // Runtime Tracking
        bool goalYReached;
        float goalY;
        float liftAccel;

        // Cached References
        readonly Vector2 windDetectionSize;
        readonly LayerMask whatIsWind;

        public State_SS_Glide(string animBool, Animator animator, SidescrollerCharacterStateMachine stateMachine)
            : base(animBool, animator, stateMachine)
        {
            glideData = stateMachine.CharacterData.data.glideStateData;

            // Cache deep references from stateMachine.CharacterData.data
            var config = stateMachine.CharacterData.data;
            windDetectionSize = config.windDetectionSize;
            whatIsWind = config.whatIsWind;

            GameEvents.OnPlayerDied += () => stateMachine.ChangeState(stateMachine.deathState);
        }

        protected override void Enter(State previousState)
        {
            base.Enter(previousState);
            goalYReached = false; //only reset on grounding

            float yVel = stateMachine.Movable.Velocity.y;

            if(yVel <= 0)
            {
                // We only allow speed boosts from dive state.
                if(previousState == stateMachine.diveState)
                {
                    float addedSpeed = Mathf.Min(glideData.maxEntryAdditionalSpeed, -yVel * glideData.entrySpeedMultiplier);
                    stateMachine.Movable.AddVelocity(new Vector3(addedSpeed * stateMachine.Movable.FacingDirection, 0, 0));
                }

                //flip the velocity, we want it to have an inverse relationship with the boosts.
                float goalFactor = -yVel * glideData.goalYMultiplier;
                goalY = Mathf.Clamp(glideData.minGoalY + goalFactor, glideData.minGoalY, glideData.maxGoalY);

                float accelFactor = -yVel * glideData.liftAccelMultiplier;
                liftAccel = Mathf.Clamp(glideData.minLiftAccel + accelFactor, glideData.minLiftAccel, glideData.maxLiftAccel);

            }
            else
            {
                liftAccel = glideData.minLiftAccel;
                goalY = glideData.minGoalY;
            }

            stateMachine.Movable.SetGravityScale(glideData.gravityScale);
        }

        protected override void StateUpdate()
        {
            base.StateUpdate();
            if (stateMachine.Jumpable.IsGrounded())
            {
                stateMachine.ChangeState(stateMachine.idleState);
                return;
            }
            //we only want to dive if jump has been released. this allows us to exit dive to glide even if the dive button is held down. feels way better.
            else if (InputHandler.Instance.JumpReleased && InputHandler.Instance.DiveInput.InputActive)
            {
                stateMachine.ChangeState(stateMachine.diveState);
                return;
            }
            else if (InputHandler.Instance.JumpReleased)
            {
                stateMachine.ChangeState(stateMachine.fallState);
                return;
            }

            Collider2D hit = Physics2D.OverlapBox(
                stateMachine.transform.position,
                windDetectionSize,
                0f,
                whatIsWind
            );
            if (hit != null && hit.TryGetComponent(out Wind wind))
            {
                stateMachine.ChangeState(stateMachine.rideWindState);
                stateMachine.WindRidable.EnterWind(wind);
            }
        }

        protected override void StateFixedUpdate()
        {
            base.StateFixedUpdate();

            // keep adding a slight lift to y velocity until we break even, then start falling.
            if (!goalYReached)
            {
                float curVelY = stateMachine.Movable.Velocity.y;
                float newY = Mathf.Min(curVelY + liftAccel * Time.fixedDeltaTime, goalY);
                stateMachine.Movable.SetVelocityY(newY);

                goalYReached = newY >= goalY;
            }

            float moveDir = stateMachine.Controller.MoveInput.x;
            if (Mathf.Abs(moveDir) > 0f)
            {
                bool sameDir = Mathf.Sign(moveDir) == Mathf.Sign(stateMachine.Movable.Velocity.x);
                float turnMult = sameDir ? 1f : glideData.turnAccelMultiplier;

                float addX = moveDir * glideData.moveSpeed;
                float newX = Mathf.MoveTowards(stateMachine.Movable.Velocity.x, stateMachine.Movable.Velocity.x + addX, glideData.acceleration * turnMult * Time.fixedDeltaTime);
                stateMachine.Movable.SetVelocityX(newX);
            }
        }

        protected override void Exit()
        {
            base.Exit();
        }
    }
}