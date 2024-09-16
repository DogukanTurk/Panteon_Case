using Cysharp.Threading.Tasks;
using DOT.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Ingame_Prefab_Building : MonoBehaviour
{
    /* ------------------------------------------ */

    public TextMeshProUGUI Txt_Title;

    public Image Img_Icon;

    public Button Btn;

    public SO_Building SO_Building;

    /* ------------------------------------------ */

    public void Setup(SO_Building data)
    {
        SO_Building = data;

        Txt_Title.text = SO_Building.Name;
        Img_Icon.sprite = SO_Building.Spr_Icon_UI;

        Btn.onClick.RemoveAllListeners();
        Btn.onClick.AddListener(FunButtonClicked);
    }

    /* ------------------------------------------ */

    private void FunButtonClicked()
    {
        Factory_Environment.Building.instance.Create(SO_Building).Forget();
    }

    /* ------------------------------------------ */
}