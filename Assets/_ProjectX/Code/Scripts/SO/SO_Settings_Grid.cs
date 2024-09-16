using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Grid", menuName = "Project X/Settings/Grid", order = 1)]
public class SO_Settings_Grid : ScriptableObject
{
    /* ------------------------------------------ */

    public int Size;

    public int2 Layout;

    /* ------------------------------------------ */
}