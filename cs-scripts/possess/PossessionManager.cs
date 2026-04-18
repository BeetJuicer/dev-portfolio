using UnityEngine;
namespace StateMachineCore
{
    public class PossessionManager : MonoBehaviour
    {
        public static PossessionManager Instance { get; private set; }

        private TopDownCharacterStateMachine currentPossessed;
        [SerializeField] TopDownCharacterStateMachine defaultChar;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            Possess(defaultChar);
        }

        public void Possess(TopDownCharacterStateMachine actor)
        {
            if (currentPossessed != null)
            {
                currentPossessed.UnPossess();   
                currentPossessed.GetComponent<ClickToPossess>().SetSelected(false);
            }

            currentPossessed = actor;
            currentPossessed.Possess();
            currentPossessed.GetComponent<ClickToPossess>().SetSelected(true);
        }
    }
}