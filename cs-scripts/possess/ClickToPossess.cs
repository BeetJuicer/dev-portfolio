using StateMachineCore;
using UnityEngine;

public class ClickToPossess : MonoBehaviour
{
    private TopDownCharacterStateMachine sm;
    [SerializeField] private GameObject hoverArrow;   // blue arrow child GO
    [SerializeField] private GameObject selectedArrow; // red arrow child GO

    private void Start()
    {
        sm = GetComponent<TopDownCharacterStateMachine>();
        hoverArrow.SetActive(false);
        selectedArrow.SetActive(false);
    }

    private void OnMouseEnter() => hoverArrow.SetActive(true);
    private void OnMouseExit() => hoverArrow.SetActive(false);

    private void OnMouseDown()
    {
        PossessionManager.Instance.Possess(sm);
    }

    public void SetSelected(bool selected)
    {
        selectedArrow.SetActive(selected);
        if (selected) hoverArrow.SetActive(false); // hide hover when selected
    }
}