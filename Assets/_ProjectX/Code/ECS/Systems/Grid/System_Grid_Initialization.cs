using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// This system is responsible of creating grids at the start of the game with the data from SO_Settings_Grid
/// </summary>
[BurstCompile]
public partial class System_Grid_Initialization : SystemBase
{
    /* ------------------------------------------ */
    
    [BurstDiscard]
    protected override void OnUpdate()
    {
        if (Manager_Ingame_Settings.instance == null)
            return;
        
        // Getting the data from GO world.
        var tempGridSettings = Manager_Ingame_Settings.instance.Grid;

        // Calculating the size of the grid
        var gridSize = tempGridSettings.Layout * tempGridSettings.Size;
        for (int x = 0; x < gridSize.x; x += tempGridSettings.Size)
        {
            for (int y = 0; y < gridSize.y; y += tempGridSettings.Size)
            {
                // Creating the grids here.
                var entity = EntityManager.CreateEntity();
                var nodeComponent = new Data_Node()
                {
                    Identity = new Identity()
                    {
                        Entity = entity
                    },
                    Position = new int2(x,y),
                    IsWalkable = true,
                    Size = tempGridSettings.Size
                };
                
                // Updating the data here. If we don't, data will be gone (bc it's a struct).
                EntityManager.AddComponentData(entity, nodeComponent);
            }
        }

        Enabled = false;
    }

    /* ------------------------------------------ */

}