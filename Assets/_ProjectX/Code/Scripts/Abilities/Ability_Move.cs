using System;
using UnityEngine;

public class Ability_Move : Ability_Interactable
{
    /* ------------------------------------------ */

    [Header("Other Components Dependencies")] 
    [SerializeField] private Ability_Health Health;
    [SerializeField] private Ability_Attack Attack;
    [SerializeField] private Ability_Pathfinding Pathfinding;
    
    /* ------------------------------------------ */

    private void OnValidate()
    {
        Health = GetComponent<Ability_Health>();
        Attack = GetComponent<Ability_Attack>();
        Pathfinding = GetComponent<Ability_Pathfinding>();
    }

    /* ------------------------------------------ */

    public override bool Interact(Vector3 point)
    {
        if (!Health.IsDead)
        {
            Pathfinding.SetDestination(point);
            return true;
        }
        
        return false;
    }

    public override bool Interact(Ability_Health target)
    {
        if (!Health.IsDead)
        {
            // We could do it with Observer pattern in better way but, it's enough for the prototype
            Attack.SetupValidTarget(target);
            return true;
        }
        
        return false;
    }

    public override bool Disengage()
    {
        return true;
    }
    
    /* ------------------------------------------ */

}