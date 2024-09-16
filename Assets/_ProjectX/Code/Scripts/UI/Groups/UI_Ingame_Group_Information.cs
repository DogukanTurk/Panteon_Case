using DOT.Utilities;
using TMPro;
using UIS;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UI_Ingame_Group_Information : ObserverMonoBehaviour
{
    /* ------------------------------------------ */

    [SerializeField] private Scroller Scroller;

    [SerializeField] private SO_Unit[] SO_Units;

    [Header("Building Related")] 
    public Transform Group_Information;
    public Image Img_Icon_Building;
    public TextMeshProUGUI Txt_Title_Building;

    /* ------------------------------------------ */
    
    private GameObject _selectedBuilding;

    private SO_Building _SO_Building;
    
    /* ------------------------------------------ */

    private void Start()
    {
        Scroller.OnFill += OnFillItem;
        Scroller.OnHeight += OnHeightItem;
        
        Manager_Ingame_Interactor.instance.AddObserver(this);
    }

    private void OnValidate()
    {
        FindSOs();
    }

    /* ------------------------------------------ */

    public void Setup(GameObject building, SO_Building SO_Building)
    {
        _selectedBuilding = building;
        _SO_Building = SO_Building;

        Img_Icon_Building.sprite = _SO_Building.Spr_Icon;
        Txt_Title_Building.text = _SO_Building.Name;
        Group_Information.gameObject.SetActive(true);

        Scroller.RecycleAll();
        if (_SO_Building.Mask_Production.IsAnythingSelected())
            Scroller.InitData(_SO_Building.Mask_Production.GetSelectedValues().Length);
    }
    
    /* ------------------------------------------ */

    [ContextMenu("FindSOs")]
    private void FindSOs()
    {
        SO_Units = Resources.LoadAll<SO_Unit>("SO/Units");
    }

    private void OnFillItem(int index, GameObject item)
    {
        SO_Unit tempSOData = null;

        if (SO_Units.Length <= index)
            index = 1;

        tempSOData = SO_Units[index];

        var tempPrefab = item.GetComponent<UI_Ingame_Prefab_Information>();
        tempPrefab.Setup(tempSOData, _selectedBuilding);
    }

    private int OnHeightItem(int index)
    {
        return 182;
    }

    /* ------------------------------------------ */
    
    public override void OnNotify<T>(Observer.Msg<T> message)
    {
        if (message.Sender.Equals("Manager_Ingame_Interactor"))
        {
            // We do use int casting here bc we don't want to hide dependency and make it easy to trackable
            if (message.Type == (int)Manager_Ingame_Interactor.Enum_NotifyType.GotInformation)
            {
                if (message.Message is Observer.Msg_Data<ScriptableObject, GameObject> data)
                    Setup(data.T2, (SO_Building)data.T1);
            }
        }
    }

    public override void OnNotify(Observer.Msg message)
    {
        if (message.Sender.Equals("Manager_Ingame_Interactor"))
        {
            if (message.Type == (int)Manager_Ingame_Interactor.Enum_NotifyType.CurrentInteractableChanged)
            {
                Group_Information.gameObject.SetActive(false);
                Scroller.RecycleAll();
            }
        }
    }

    /* ------------------------------------------ */

}