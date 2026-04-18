using StateMachineCore;
using System.Collections;
using UnityEngine;

namespace Assets.Projects.StateMachine.SideScroll.SS_States
{
    public class State_SS_Dive : AnimatedState<SidescrollerCharacterStateMachine>
    {
        public State_SS_Dive(string animBool, Animator animator, SidescrollerCharacterStateMachine stateMachine)
            : base(animBool, animator, stateMachine) {
            GameEvents.OnPlayerDied += () => stateMachine.ChangeState(stateMachine.deathState); //TODO: inherit from an aliveState, move the death transition there.
        }
        protected override void Enter(State previousState)
        {
            base.Enter(previousState);
            stateMachine.Movable.SetGravityScale(stateMachine.CharacterData.data.fallGravityScale);
            stateMachine.Movable.SetVelocityY(-stateMachine.CharacterData.data.diveSpeed);

            GameEvents.OnPlayerDied += () => stateMachine.ChangeState(stateMachine.deathState);

        }
        protected override void StateUpdate()
        {
            base.StateUpdate();
            if (stateMachine.Jumpable.IsGrounded())
            {
                Debug.Log("Dive to idle");
                stateMachine.ChangeState(stateMachine.idleState);
                return;
            }
            else if(InputHandler.Instance.DiveReleased)
            {
                stateMachine.ChangeState(stateMachine.fallState);
                return;
            }
            else if(InputHandler.Instance.JumpHeld)
            {
                stateMachine.ChangeState(stateMachine.glideState);
                return;
            }
        }
        protected override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            float moveDir = stateMachine.Controller.MoveInput.x;
            float targetX = moveDir * stateMachine.CharacterData.data.diveAirControlSpeed;
            float newX = Mathf.MoveTowards(stateMachine.Movable.Velocity.x, targetX, stateMachine.CharacterData.data.diveAirControlSpeed * Time.fixedDeltaTime);
            stateMachine.Movable.SetVelocityX(newX);
        }
        protected override void Exit()
        {
            base.Exit();
        }
    }
}