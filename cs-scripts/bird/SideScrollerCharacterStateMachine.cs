namespace StateMachineCore
{
    using Assets.Projects.StateMachine.SideScroll.SS_States;
    using UnityEngine;
    public class SidescrollerCharacterStateMachine : StateMachine
    {
        public State_SS_Idle idleState { get; private set; }
        public State_SS_Move walkState { get; private set; }
        public State_SS_Jump jumpState { get; private set; }
        public State_SS_Fall fallState { get; private set; }
        public State_SS_Glide glideState { get; private set; }
        public State_SS_RideWind rideWindState { get; private set; }
        public State_SS_Dive diveState { get; private set; }
        public State_SS_Death deathState { get; private set; }

        public IMovable2D Movable { get; private set; }
        public IJumpable Jumpable { get; private set; }
        public IWindRidable WindRidable { get; private set; }
        public Animator Animator { get; private set; }

        [SerializeField] private ISideScrollerController controller;
        [SerializeField] private SO_SS_PlayerData playerData;
        [SerializeField] private SO_SS_AIData aiControllerData;

        public SO_SS_PlayerData CharacterData => playerData;
        public ISideScrollerController Controller => controller;

        private SS_PlayerController playerController;
        //private AIController aiController;

        protected override void Start()
        {
            Animator = GetComponentInChildren<Animator>();
            Movable = GetComponent<IMovable2D>();
            Jumpable = GetComponent<IJumpable>();
            WindRidable = GetComponent<IWindRidable>();

            playerController = new SS_PlayerController();
            //aiController = new AIController(this, aiControllerData.data);

            idleState = new State_SS_Idle("st_idle", Animator, this);
            walkState = new State_SS_Move("st_move", Animator, this);
            jumpState = new State_SS_Jump("st_jump", Animator, this);
            fallState = new State_SS_Fall("st_fall", Animator, this);
            glideState  = new State_SS_Glide("st_glide", Animator, this);
            rideWindState = new State_SS_RideWind("st_rideWind", Animator, this);
            diveState = new State_SS_Dive("st_dive", Animator, this);
            deathState = new State_SS_Death("st_death", Animator, this);

            Movable.SetMaxFallSpeed(playerData.data.maxFallSpeed);

            controller = playerController;
            ChangeState(idleState);
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
            controller.Update();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public void Possess() { controller = playerController; }
        //public void UnPossess() { controller = aiController; }
    }
}