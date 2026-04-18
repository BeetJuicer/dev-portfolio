namespace StateMachineCore
{
using System.Collections.Generic;
using UnityEngine;

public class AIController : ITopDownController
{
    TopDownCharacterStateMachine sm;
    public AIController(TopDownCharacterStateMachine sm, AIControllerData data)
    {
        this.sm = sm;
        this.data = data;
    }

    public Vector2 MoveInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool RollInput => false;

    private float idleEndTime;
    private float patrolEndTime;
    private Vector2 patrolDirection;
    private AIControllerData data;
    public void Update()
    {
        if (Time.time < idleEndTime)
        {
            MoveInput = Vector2.zero;
            AttackInput = false;
        }
        else if (Time.time < patrolEndTime)
        {
            MoveInput = patrolDirection;
        }
        else
        {
            // decide next action
            StartIdle();
        }
    }

    private void StartIdle()
    {
        MoveInput = Vector2.zero;
        idleEndTime = Time.time + Random.Range(data.minIdleTime, data.maxIdleTime);
        patrolEndTime = idleEndTime + Random.Range(data.minPatrolTime, data.maxPatrolTime);
        patrolDirection = GetRandomDirectionExcluding(sm.Movable.LastMoveDirection);
    }

    private Vector2 GetRandomDirectionExcluding(Vector2 dirToExclude)
    {
        Vector2 topRight = new Vector2(1, 1);
        Vector2 topLeft = new Vector2(-1, 1);
        Vector2 bottomRight = new Vector2(1, -1);
        Vector2 bottomLeft = new Vector2(-1, -1);

        List<Vector2> directions = new()
        {
            Vector2.right, Vector2.left, Vector2.up, Vector2.down,
            topLeft, topRight, bottomLeft, bottomRight
        };

        directions.Remove(dirToExclude);
        return directions[Random.Range(0, directions.Count)];
    }
}

[System.Serializable]
public struct AIControllerData
{
    public float minIdleTime;
    public float maxIdleTime;
    public float minPatrolTime;
    public float maxPatrolTime;
}
}