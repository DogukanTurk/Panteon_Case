using UnityEngine;

public class Ability_Interactable : MonoBehaviour
{
    /* ------------------------------------------ */

    public virtual bool Interact()
    {
        return true;
    }
    
    public virtual bool Interact(Vector3 point)
    {
        return true;
    }

    public virtual bool Interact(Ability_Health target)
    {
        return true;
    }

    public virtual bool Disengage()
    {
        return true;
    }
    
    /* ------------------------------------------ */
}