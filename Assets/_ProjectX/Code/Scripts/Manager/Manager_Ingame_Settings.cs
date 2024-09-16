using System;
using DOT.Utilities;
using Unity.Entities;
using UnityEngine;

public class Manager_Ingame_Settings : SingletonComponent<Manager_Ingame_Settings>
{
    /* ------------------------------------------ */

    public SO_Settings_Grid Grid;

    /* ------------------------------------------ */

    private void Start()
    {
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<System_Grid_Obstacles>().Enabled = true;
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<System_Grid_Obstacles>().Setup(Grid);
    }

    private void OnValidate()
    {
        Grid = Resources.LoadAll<SO_Settings_Grid>("SO/")[0];
    }

    /* ------------------------------------------ */
}