using UnityEngine;

/// <summary>
/// This is the base Weapon class which used for attacking
/// </summary>
public abstract class Weapon
{
    /* ------------------------------------------ */

    #region Definitions

    /// <summary>
    /// The rifle gun inhereted from Weapon, nothing special here but maybe we can later on.
    /// </summary>
    public class Rifle : Weapon
    {
        /* ------------------------------------------ */

        public override void Attack(Ability_Health target)
        {
            base.Attack(target);
        }

        /* ------------------------------------------ */
    }

    #endregion

    /* ------------------------------------------ */

    public float FireRate { get; internal set; }
    public int Damage { get; internal set; }

    public int Range { get; internal set; }

    public int RangeSq { get; internal set; }

    /* ------------------------------------------ */

    private float _lastFireTime;

    /* ------------------------------------------ */

    public virtual void Setup(float fireRate, int damage, int range)
    {
        FireRate = fireRate;
        Damage = damage;
        Range = range;

        RangeSq = range * range;
    }

    /// <summary>
    /// We calculate the cooldown and doing damage here, nothing special
    /// </summary>
    /// <param name="target"></param>
    public virtual void Attack(Ability_Health target)
    {
        if (Time.time >= _lastFireTime + FireRate)
        {
            _lastFireTime = Time.time;

            target.TakeDamage(Damage);
            Debug.Log("Attacked");
        }
    }

    /* ------------------------------------------ */
}