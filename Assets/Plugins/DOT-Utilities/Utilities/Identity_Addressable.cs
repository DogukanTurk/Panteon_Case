using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DOT.Utilities
{
    public class Identity_Addressable : MonoBehaviour
    {
        /* ------------------------------------------ */

        public AssetReference MyReference;
        
        /* ------------------------------------------ */

        private void OnDestroy()
        {
            Manager_Addressables.RemoveAsset(MyReference);
        }

        /* ------------------------------------------ */

        public void Setup(AssetReference reference)
        {
            MyReference = reference;
        }
        
        /* ------------------------------------------ */

    }
}