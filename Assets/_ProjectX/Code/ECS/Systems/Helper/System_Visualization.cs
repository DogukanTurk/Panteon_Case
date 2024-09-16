#if UNITY_EDITOR

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// This system is made for debug purposes only.
/// </summary>
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class System_Visualization : SystemBase
{
    /* ------------------------------------------ */

    private SO_Settings_Grid _grid;

    /* ------------------------------------------ */

    protected override void OnCreate()
    {
        // If we need to debug the grid system, we'll enable it via the System tab.
        Enabled = false;
    }

    protected override void OnUpdate()
    {
        if (ReferenceEquals(_grid, null))
        {
            if (!ReferenceEquals(Manager_Ingame_Settings.instance, null))
                _grid = Manager_Ingame_Settings.instance.Grid;
            else
                return;
        }

        float gridSize = _grid.Size;
        Entities.ForEach((Entity entity, ref Data_Node node) =>
        {
            int2 nodePosition = node.Position;
            float3 worldPosition = new float3(nodePosition.x, 0, nodePosition.y);

            float3 topLeft = worldPosition + new float3(-gridSize / 2, 0, gridSize / 2);
            float3 topRight = worldPosition + new float3(gridSize / 2, 0, gridSize / 2);
            float3 bottomLeft = worldPosition + new float3(-gridSize / 2, 0, -gridSize / 2);
            float3 bottomRight = worldPosition + new float3(gridSize / 2, 0, -gridSize / 2);

            Debug.DrawLine(topLeft, topRight, Color.green);
            Debug.DrawLine(topRight, bottomRight, Color.green);
            Debug.DrawLine(bottomRight, bottomLeft, Color.green);
            Debug.DrawLine(bottomLeft, topLeft, Color.green);
        }).WithoutBurst().Run();
    }

    /* ------------------------------------------ */
}

#endif