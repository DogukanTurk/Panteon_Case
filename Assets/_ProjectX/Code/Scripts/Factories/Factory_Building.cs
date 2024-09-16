using Cysharp.Threading.Tasks;
using DOT.Utilities;
using UnityEngine;

/// <summary>
/// This is the main Environment factory class. 
/// </summary>
public abstract class Factory_Environment
{
    /* ------------------------------------------ */

    public static Camera MainCamera;

    /* ------------------------------------------ */

    public abstract UniTask<GameObject> Create(ScriptableObject data);

    /* ------------------------------------------ */

    public class Building : Factory_Environment
    {
        /* ------------------------------------------ */

        public static Building instance
        {
            get
            {
                if (ReferenceEquals(_instance, null))
                    _instance = new Building();

                return _instance;
            }
        }

        /// <summary>
        /// We need to store this value somewhere to block duplication
        /// </summary>
        public static GameObject CurrentPreviewingObject;

        /* ------------------------------------------ */
        
        private static Building _instance;
        
        /* ------------------------------------------ */

        /// <summary>
        /// We are creating an instance and doing the setup here and returning the object
        /// after the initialization.
        /// </summary>
        /// <param name="tempData"></param>
        /// <returns></returns>
        public override async UniTask<GameObject> Create(ScriptableObject tempData)
        {
            SO_Building data = (SO_Building)tempData;

            Ability_Placeable tempPlaceable = null;
            if (CurrentPreviewingObject != null)
            {
                // If there any preview object, destroy it.
                if (CurrentPreviewingObject.TryGetComponent(out tempPlaceable))
                {
                    if (tempPlaceable.CurrentState != Ability_Placeable.State.Built)
                    {
                        GameObject.Destroy(CurrentPreviewingObject);
                        CurrentPreviewingObject = null;
                    }
                }
            }

            CurrentPreviewingObject = await Manager_Addressables.LoadGameObject(data.Prefab);

            // Position and scale calculations
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f,
                Mathf.Abs(MainCamera.transform.position.z));
            Vector3 worldPosition = MainCamera.ScreenToWorldPoint(screenCenter);
            worldPosition.y = 0;

            CurrentPreviewingObject.transform.position = worldPosition;
            CurrentPreviewingObject.transform.localScale =
                new Vector3(data.Dimensions.x, 1, data.Dimensions.y);

            // Updating the components here
            if (CurrentPreviewingObject.TryGetComponent(out tempPlaceable))
                tempPlaceable.SwitchState(Ability_Placeable.State.Moving);

            if (CurrentPreviewingObject.TryGetComponent(out Visuals visuals))
            {
                for (int x = 0; x < visuals.SpriteRenderers.Length; x++)
                    visuals.SpriteRenderers[x].sprite = data.Spr_Icon;
            }
            
            if (CurrentPreviewingObject.TryGetComponent(out Ability_Health health))
                health.Max = data.MaxHealth;
            
            if (CurrentPreviewingObject.TryGetComponent(out Information tempInformation))
                tempInformation.Data = data;
            
            return CurrentPreviewingObject;
        }

        /* ------------------------------------------ */
    }

    /* ------------------------------------------ */
}