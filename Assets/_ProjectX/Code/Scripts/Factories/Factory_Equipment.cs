public abstract class Factory_Weapon
{
    /* ------------------------------------------ */

    public abstract Weapon Create();
    
    /* ------------------------------------------ */

    public class Rifle : Factory_Weapon
    {
        /* ------------------------------------------ */

        public static Rifle instance
        {
            get
            {
                if (ReferenceEquals(_instance, null))
                    _instance = new Rifle();

                return _instance;
            }
        }

        /* ------------------------------------------ */
        
        private static Rifle _instance;
        
        /* ------------------------------------------ */

        public override Weapon Create()
        {
            return new Weapon.Rifle();
        }
        
        /* ------------------------------------------ */
    }
    
    /* ------------------------------------------ */
}

