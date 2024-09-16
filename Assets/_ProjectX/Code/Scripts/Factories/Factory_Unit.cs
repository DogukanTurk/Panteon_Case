using Cysharp.Threading.Tasks;
using DOT.Utilities;
using UnityEngine;

/// <summary>
/// This is the main Unit factory class. 
/// </summary>
public abstract class Factory_Unit
{
    /* ------------------------------------------ */

    public static Camera MainCamera;

    /* ------------------------------------------ */

    public abstract UniTask<GameObject> Create(ScriptableObject data, Vector3 position);

    /* ------------------------------------------ */

    public class Soldier : Factory_Unit
    {
        /* ------------------------------------------ */

        public static Soldier instance
        {
            get
            {
                if (ReferenceEquals(_instance, null))
                    _instance = new Soldier();

                return _instance;
            }
        }

        /* ------------------------------------------ */

        private static Soldier _instance;

        /* ------------------------------------------ */

        /// <summary>
        /// We are creating an instance and doing the setup here and returning the object
        /// after the initialization.
        /// </summary>
        /// <param name="tempData"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public override async UniTask<GameObject> Create(ScriptableObject tempData, Vector3 position)
        {
            SO_Unit data = (SO_Unit)tempData;

            var tempSoldier = await Manager_Addressables.LoadGameObject(data.Prefab);

            // Updating the position
            tempSoldier.transform.position = position;

            // Updating the components here
            if (tempSoldier.TryGetComponent(out Visuals visuals))
            {
                for (int x = 0; x < visuals.SpriteRenderers.Length; x++)
                    visuals.SpriteRenderers[x].sprite = data.Spr_Icon;
            }

            if (tempSoldier.TryGetComponent(out Ability_Health health))
                health.Max = data.MaxHealth;

            if (tempSoldier.TryGetComponent(out Ability_Pathfinding pathfinding))
                pathfinding.Setup(data.MovementSpeed);

            if (tempSoldier.TryGetComponent(out Ability_Attack attack))
            {
                var weapon = Factory_Weapon.Rifle.instance.Create();
                weapon.Setup(data.Weapon.FireRate, data.Weapon.Damage, data.Weapon.Range);

                attack.Setup(weapon);
            }

            return tempSoldier;
        }

        /* ------------------------------------------ */
    }

    /* ------------------------------------------ */
}