namespace StateMachineCore
{
using UnityEngine;

public class State_TD_Idle : AnimatedState<TopDownCharacterStateMachine>
{
    public State_TD_Idle(string animBool, Animator animator, TopDownCharacterStateMachine stateMachine) : base(animBool, animator, stateMachine)
    {
    }

    protected override void Enter(State previousState)
    {
        base.Enter(previousState);
    }

    protected override void StateUpdate()
    {
        base.StateUpdate();

        if(stateMachine.Controller.MoveInput != Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.moveState);
            return;
        }
        else if (stateMachine.Controller.RollInput)
        {
            stateMachine.ChangeState(stateMachine.rollState);
            return;
        }
        else if (stateMachine.Controller.AttackInput)
        {
            stateMachine.ChangeState(stateMachine.attackState);
            return;
        }
    }

    protected override void Exit() { base.Exit(); }
}

}