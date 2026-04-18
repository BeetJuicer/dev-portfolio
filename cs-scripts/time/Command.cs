using UnityEngine;

public abstract class Command
{
    public abstract void Execute();
    public abstract void Undo();
}

public class MoveCommand : Command
{
    public Vector3 newPos { get; }
    public Vector3 oldPos { get; }
    public Quaternion newRot { get; }
    public Quaternion oldRot { get; }
    public GameObject owner { get; }
    public float speed;
    public float currentT = 1;

    public MoveCommand(Vector3 oldPos, Vector3 newPos, Quaternion oldRot, Quaternion newRot, GameObject owner)
    {
        this.oldPos = oldPos;
        this.newPos = newPos;
        this.oldRot = oldRot;
        this.newRot = newRot;
        this.owner = owner;
    }

    public override void Execute()
    {
        owner.transform.position = newPos;
        owner.transform.rotation = newRot;
        currentT = 1;
    }

    public void LerpTowardsNew(float t)
    {
        if (currentT < 1f)
        {
            currentT = Mathf.Min(currentT + t, 1f);
            owner.transform.position = Vector3.Lerp(oldPos, newPos, currentT);
            owner.transform.rotation = Quaternion.Slerp(oldRot, newRot, currentT);
        }
        else
        {
            currentT = 1;
            owner.transform.position = newPos;
            owner.transform.rotation = newRot;
        }
    }

    public void LerpTowardsOld(float t)
    {
        if (currentT > 0.001f)
        {
            currentT = Mathf.Max(currentT - t, 0f);
            owner.transform.position = Vector3.Lerp(oldPos, newPos, currentT);
            owner.transform.rotation = Quaternion.Slerp(oldRot, newRot, currentT);
        }
        else
        {
            currentT = 0;
            owner.transform.position = oldPos;
            owner.transform.rotation = oldRot;
        }
    }

    public override void Undo()
    {
        owner.transform.position = oldPos;
        owner.transform.rotation = oldRot;
        currentT = 0;
    }
}