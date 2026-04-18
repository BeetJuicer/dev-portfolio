namespace StateMachineCore
{
using UnityEngine;

public class PlayerController : ITopDownController
{
    public Vector2 MoveInput => InputHandler.Instance.ActiveMoveInput;

    public bool RollInput => InputHandler.Instance.RollInput.InputActive;

    public bool AttackInput => InputHandler.Instance.AttackInput.InputActive;

    public void Update()
    {
    }
}

}