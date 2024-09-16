using DOT.Utilities;
using UnityEngine;

/// <summary>
/// The main class responsible of interacting with objects
/// </summary>
public class Manager_Ingame_Interactor : SingletonObserverComponent<Manager_Ingame_Interactor>
{
    /* ------------------------------------------ */

    public enum Enum_NotifyType
    {
        None = 0,

        GotInformation = 10,
        CurrentInteractableChanged = 11
    }

    /* ------------------------------------------ */

    [SerializeField] private LayerMask LayerMask_PickingItem;

    /* ------------------------------------------ */

    private Camera _mainCamera;

    private Ray _ray;

    private RaycastHit _hitInfo;

    private Ability_Interactable _currentInteractable;

    /* ------------------------------------------ */

    private void Start()
    {
        SenderName = nameof(Manager_Ingame_Interactor);
        _mainCamera = Camera.main;

        Factory_Environment.MainCamera = _mainCamera;
    }

    private void Update()
    {
        // Check is there something that we can interact with
        if (Input.GetMouseButtonDown(0))
            Engage();

        // Move/Attack if we have a selected unit.
        if (Input.GetMouseButtonDown(1))
            MoveToMouse();
    }

    /* ------------------------------------------ */

    public void Interact(Ability_Interactable interactable)
    {
        // Check if we can interact with it, if we can, do it and save it as currentInteractable
        if (interactable.Interact())
            ChangeCurrentInteractable(interactable);
        else
        {
            // If it's a building, show the information page.
            if (interactable.TryGetComponent(out Information information))
            {
                // Broadcast the information gathered from the interactable object.
                NotifyObservers(new Observer.Msg<Observer.Msg_Data<ScriptableObject, GameObject>>()
                {
                    Type = (int)Enum_NotifyType.GotInformation,
                    Message = new Observer.Msg_Data<ScriptableObject, GameObject>()
                    {
                        T1 = information.Data,
                        T2 = interactable.gameObject
                    }
                });
            }
        }
    }

    /* ------------------------------------------ */

    private void Engage()
    {
        // If we already have one interactable object selected, clear it.
        if (!ReferenceEquals(_currentInteractable, null))
            Disengage();
        
        _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(_ray, out _hitInfo, 100, LayerMask_PickingItem))
        {
            // Try to interact with the object.
            if (_hitInfo.transform.gameObject.TryGetComponent(out Ability_Interactable interactable))
                Interact(interactable);
        }
    }

    private void Disengage()
    {
        if (ReferenceEquals(_currentInteractable, null))
            return;

        if (_currentInteractable.Disengage())
            ChangeCurrentInteractable(null);
    }

    private void MoveToMouse()
    {
        if (ReferenceEquals(_currentInteractable, null))
            return;

        _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        // If we click on a target, attack. If not, move.
        if (Physics.Raycast(_ray, out _hitInfo, 100, LayerMask_PickingItem))
        {
            if (_hitInfo.transform.gameObject.TryGetComponent(out Ability_Health health))
                _currentInteractable.Interact(health);
            else
                _currentInteractable.Interact(_hitInfo.point);
        }
    }

    private void ChangeCurrentInteractable(Ability_Interactable interactable)
    {
        _currentInteractable = interactable;
        NotifyObservers(new Observer.Msg()
        {
            Type = (int)Enum_NotifyType.CurrentInteractableChanged
        });
    }

    /* ------------------------------------------ */
}