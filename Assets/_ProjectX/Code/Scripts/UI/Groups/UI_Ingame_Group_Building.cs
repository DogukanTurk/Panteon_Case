using UIS;
using UnityEngine;

public class UI_Ingame_Group_Building : MonoBehaviour
{
    /* ------------------------------------------ */

    [SerializeField] private Scroller Scroller;

    [SerializeField] private SO_Building[] SO_Buildings;

    /* ------------------------------------------ */

    private void Start()
    {
        Scroller.OnFill += OnFillItem;
        Scroller.OnHeight += OnHeightItem;
        
        Scroller.InitData(20);
    }

    private void OnValidate()
    {
        FindSOs();
    }

    /* ------------------------------------------ */

    [ContextMenu("FindSOs")]
    private void FindSOs()
    {
        SO_Buildings = Resources.LoadAll<SO_Building>("SO/Buildings");
    }

    private void OnFillItem(int index, GameObject item)
    {
        SO_Building tempSOData = null;

        if (SO_Buildings.Length <= index)
            index = 1;
        
        tempSOData = SO_Buildings[index];

        var tempPrefab = item.GetComponent<UI_Ingame_Prefab_Building>();
        tempPrefab.Setup(tempSOData);
    }

    private int OnHeightItem(int index)
    {
        return 182;
    }

    /* ------------------------------------------ */
}