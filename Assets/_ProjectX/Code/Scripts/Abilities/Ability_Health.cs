using DOT.Utilities;

/// <summary>
/// This class is designed to giving live to units... It also gives taking damage and dying ability too :)
/// </summary>
public class Ability_Health : ObserverMonoBehaviour
{
    /* ------------------------------------------ */

    /// <summary>
    /// We'll have this Enum when we use Observer, this is best way to not hide dependecies when using Observer
    /// </summary>
    public enum Enum_NotifyType
    {
        None = 0,

        GotDamage = 10,
        Dead = 11
    }

    /* ------------------------------------------ */

    public bool IsDead;

    public int Current;
    public int Max;

    /* ------------------------------------------ */

    /// <summary>
    /// We save the high traffic message and don't create it every time we need it, that's good for performance.
    /// </summary>
    private Observer.Msg<Observer.Msg_Data<int, int>> _msgDamage;

    /* ------------------------------------------ */

    private void Start()
    {
        SenderName = nameof(Ability_Health);
        Current = Max;

        _msgDamage = new Observer.Msg<Observer.Msg_Data<int, int>>()
        {
            Type = (int)Enum_NotifyType.GotDamage,
            Message = new Observer.Msg_Data<int, int>()
        };
    }

    /* ------------------------------------------ */

    /// <summary>
    /// Taking damage and check if we died.
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {
        if (damage < 0)
            damage = 0;

        Current -= damage;

        // Update the parameters and broadcast the message
        _msgDamage.Message.T1 = Current;
        _msgDamage.Message.T2 = damage;
        NotifyObservers(_msgDamage);

        if (Current <= 0)
            Die();
    }

    /* ------------------------------------------ */

    private void Die()
    {
        IsDead = true;

        // Let observers know, that we died.
        NotifyObservers(new Observer.Msg()
        {
            Type = (int)Enum_NotifyType.Dead
        });

        Destroy(this.gameObject);
    }

    /* ------------------------------------------ */
}