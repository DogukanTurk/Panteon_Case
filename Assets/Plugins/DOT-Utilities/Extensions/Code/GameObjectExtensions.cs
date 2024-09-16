using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class GameObjectExtensions
{
    /* ------------------------------------------ */

    public static void SetLayer(this GameObject origin, int layer, bool includeChildreen = false)
    {
        if (includeChildreen)
        {
            var children = origin.transform.root.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
                child.gameObject.layer = layer;
        }

        origin.layer = layer;
    }

    /* ------------------------------------------ */
}