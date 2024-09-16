using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// This class is designed to giving attacking ability to units.
/// This class is uses Weapon to attack any Ability_Health.
/// </summary>
public class Ability_Attack : MonoBehaviour
{
    /* ------------------------------------------ */

    public Weapon Weapon;

    /// <summary>
    /// We need this in order to move to the target if it's not in range.
    /// </summary>
    [Header("Other Component Dependencies")] [SerializeField]
    private Ability_Pathfinding Pathfinding;

    /* ------------------------------------------ */

    /// <summary>
    /// The current target, if we have any.
    /// </summary>
    private Ability_Health _validTarget;

    /* ------------------------------------------ */

    private void Update()
    {
        // Make sure we have a valid target.
        if (_validTarget != null)
        {
            // Check distance, if it's far get close before trying to attack.
            if (math.distancesq(_validTarget.transform.position, this.transform.position) > Weapon.RangeSq)
                Pathfinding.SetDestination(_validTarget.transform.position);
            else
            {
                // We can stop the Unit here if it needed.

                // Cycling the attack function here, weapon will check the fire rate itself.
                Weapon.Attack(_validTarget);
                
                Pathfinding.LookAt(_validTarget.transform.position);
            }
        }
    }

    /// <summary>
    /// Make sure we don't have missing references
    /// </summary>
    private void OnValidate()
    {
        // We don't want to do null check in update, so we make sure the component is disabled till it's ready to use.
        enabled = false;

        Pathfinding = GetComponent<Ability_Pathfinding>();
    }

    /* ------------------------------------------ */

    /// <summary>
    /// We need to run this method in order to run this component proparly.
    /// Factory should run this method.
    /// </summary>
    /// <param name="weapon"></param>
    public void Setup(Weapon weapon)
    {
        Weapon = weapon;

        enabled = true;
    }

    /// <summary>
    /// The method responsible to update Valid Target, so we can attack.
    /// We could do it with Observer pattern in better way but, it's enough for the prototype
    /// </summary>
    /// <param name="validTarget"></param>
    public void SetupValidTarget(Ability_Health validTarget)
    {
        _validTarget = validTarget;
    }

    /* ------------------------------------------ */
}