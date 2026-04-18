// State_SS_RideWind.cs
using UnityEngine;
namespace StateMachineCore
{
    public class State_SS_RideWind : AnimatedState<SidescrollerCharacterStateMachine>
    {
        private float origGravityScale;
        private Wind currentWind;
        private bool isTouchingWind;
        private bool newWindDetected;

        private float windLostTimer;
        private const float WindLostGrace = 0.25f; // tune if needed, ~5-6 fixed frames
        public State_SS_RideWind(string animBool, Animator animator, SidescrollerCharacterStateMachine stateMachine) : base(animBool, animator, stateMachine)
        {
            GameEvents.OnPlayerDied += () => stateMachine.ChangeState(stateMachine.deathState);

        }

        public void SetWind(Wind wind) => currentWind = wind;

        protected override void Enter(State previousState)
        {
            base.Enter(previousState);
            origGravityScale = stateMachine.Movable.GravityScale;
            stateMachine.Movable.SetGravityScale(0f);
        }

        protected override void StateUpdate()
        {
            base.StateUpdate();
            if (InputHandler.Instance.JumpReleased)
            {
                stateMachine.ChangeState(stateMachine.fallState);
                return;
            }
            else if (!isTouchingWind && windLostTimer >= WindLostGrace)
            {
                stateMachine.ChangeState(stateMachine.glideState);
                return;
            }

            if (newWindDetected)
            {
                stateMachine.WindRidable.ExitWind();
                stateMachine.WindRidable.EnterWind(currentWind);
                newWindDetected = false;
            }

        }

        protected override void StateFixedUpdate()
        {
            base.StateFixedUpdate();

            Collider2D hit = Physics2D.OverlapBox(
                stateMachine.transform.position,
                stateMachine.CharacterData.data.windDetectionSize,
                0f,
                stateMachine.CharacterData.data.whatIsWind
            );

            Wind wind = null;
            isTouchingWind = (hit != null && hit.TryGetComponent(out wind));

            if (isTouchingWind)
            {
                windLostTimer = 0f; // reset grace timer while touching
                if (wind != stateMachine.WindRidable.CurrentWind)
                {
                    currentWind = wind;
                    newWindDetected = true;
                }
            }
            else
            {
                windLostTimer += Time.fixedDeltaTime; // only count up when contact is lost
            }
        }

        protected override void Exit()
        {
            base.Exit();
            stateMachine.WindRidable.ExitWind();
            stateMachine.Movable.SetGravityScale(origGravityScale);
        }
    }
}