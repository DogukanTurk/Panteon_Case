using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace DOT.Utilities
{
    public static class Manager_Addressables
    {
        /* ------------------------------------------ */

        private static Dictionary<AssetReference, int> _assets = new();

        private static AssetReference _tempAssetReference;

        /* ------------------------------------------ */

        public static void RemoveAsset(AssetReference asset)
        {
            if (_assets.ContainsKey(asset))
                _assets[asset]--;

            foreach (var tempAsset in _assets)
            {
                if (tempAsset.Value <= 0)
                    tempAsset.Key.ReleaseAsset();
            }
        }

        public static async UniTask<GameObject> LoadGameObject(AssetReference assetReference)
        {
            AddAsset(assetReference);

            GameObject tempObj = null;
            for (int x = 0; x < _assets.Count; x++)
            {
                _tempAssetReference = _assets.ElementAt(x).Key;
                if (_tempAssetReference.Equals(assetReference))
                {
                    if (_tempAssetReference.Asset == null)
                    {
                        var _handle = Addressables.LoadAssetAsync<GameObject>(_tempAssetReference);
                        await UniTask.WaitUntil(() => { return _handle.IsDone; });
                    }
                    
                    tempObj = await _tempAssetReference.InstantiateAsync();
                    tempObj.GetComponent<Identity_Addressable>().Setup(assetReference);

                    return tempObj;
                }
            }

            return null;
        }

        /* ------------------------------------------ */

        private static void AddAsset(AssetReference asset)
        {
            if (!_assets.ContainsKey(asset))
                _assets.Add(asset, 1);
            else
                _assets[asset]++;
        }

        /* ------------------------------------------ */
    }
}