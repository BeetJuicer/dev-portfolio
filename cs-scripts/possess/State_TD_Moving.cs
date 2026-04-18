namespace StateMachineCore
{
using UnityEngine;

public class State_TD_Moving : AnimatedState<TopDownCharacterStateMachine>
{
    private MoveStateData data;
    public State_TD_Moving(string animBool, Animator animator, TopDownCharacterStateMachine stateMachine) : base(animBool, animator, stateMachine)
    {
        data = stateMachine.CharacterData.moveStateData;
    }

    protected override void Enter(State previousState)
    {
        base.Enter(previousState);
    }

    protected override void StateUpdate()
    {
        base.StateUpdate();

        if (stateMachine.Controller.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.idleState);
            return;
        }
        else if(stateMachine.Controller.AttackInput)
        {
            stateMachine.ChangeState(stateMachine.attackState);
            return;
        }
        else if (stateMachine.Controller.RollInput)
        {   
            stateMachine.ChangeState(stateMachine.rollState);
            return;
        }
    }

    protected override void StateFixedUpdate()
    {
        base.StateFixedUpdate();

        Vector2 direction = stateMachine.Controller.MoveInput;
        Vector2 moveAmount = direction * data.speed * Time.fixedDeltaTime;
        stateMachine.Movable.Move(moveAmount);
    }


    protected override void Exit() { 
        base.Exit(); 
    }

}

}