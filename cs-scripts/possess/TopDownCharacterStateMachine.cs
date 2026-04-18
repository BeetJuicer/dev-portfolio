namespace StateMachineCore
{
using UnityEngine;

public class TopDownCharacterStateMachine : StateMachine
{
    public State_TD_Idle idleState { get; private set; }
    public State_TD_Moving moveState { get; private set; }
    public State_TD_Attack attackState { get; private set; }
    public State_TD_Roll rollState { get; private set; }
    public State_AI_Patrol patrolState { get; private set; }
    public IMovable2D Movable { get; private set; }
    public IAttacker Attacker { get; private set; }
    public Animator Animator { get; private set; }

    [SerializeField] private bool possessOnStart;
    [SerializeField] private ITopDownController controller;
    [SerializeField] private SO_AttackData attackData; // change when moving to multiple attacks
    [SerializeField] private SO_TD_PlayerData playerData;
    [SerializeField] private SO_TD_AIData aiControllerData;
    public SO_TD_PlayerData CharacterData => playerData;
    public ITopDownController Controller => controller;
    private PlayerController playerController;
    private AIController aiController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        Attacker = GetComponentInChildren<IAttacker>();

        Movable = GetComponent<IMovable2D>();

        playerController = new PlayerController();
        aiController = new AIController(this, aiControllerData.data);

        idleState = new State_TD_Idle("st_idle", Animator, this);
        moveState = new State_TD_Moving("st_move", Animator, this);
        attackState = new State_TD_Attack("st_attack", attackData.data, Animator, this);
        rollState = new State_TD_Roll("st_roll", Animator, this);
        patrolState = new State_AI_Patrol("st_patrol", Animator, this);

        controller = aiController;

        ChangeState(idleState);
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        controller.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public void Possess()
    {
        controller = playerController;
    }

    public void UnPossess()
    {
        controller = aiController;
    }
}

}