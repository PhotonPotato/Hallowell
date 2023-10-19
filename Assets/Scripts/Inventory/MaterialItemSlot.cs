using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MaterialItemSlot : MonoBehaviour
{
    public Image icon;
    public MaterialItem item;

    //Effectively sets the icon to reference the correct image
    public void initMaterialItemSlot()
    {
        if(GetComponentsInChildren<Image>().Length != 0) icon = GetComponentsInChildren<Image>()[1];
    }

    //Essentially updates the item slot (icon and item data) then shows it (activates)
    public void AddIcon(MaterialItem item, bool showSlot = true)
    {
        if (icon == null) initMaterialItemSlot();

        this.item = item.getDeepCopy();
        icon.sprite = this.item.itemIcon;
        icon.enabled = true;
        gameObject.SetActive(showSlot);

    }

    //Updates 'icon' reference to Imager renderer and sets image and data to null
    public void ClearSlot(bool hideSlot = true)
    {
        if (icon == null) initMaterialItemSlot();

        item = null;
        icon.sprite = null;
        icon.enabled = false;
        gameObject.SetActive(!hideSlot);
    }

    //Returns copy of the Item
    public MaterialItem getItem()
    {
        return item.getDeepCopy();
    }

    //Return a deep copy that has no pointers (hopefully)
    public MaterialItemSlot getDeepCopy()
    {
        /*MaterialItemSlot slot = new MaterialItemSlot();

        slot.item = item.getDeepCopy();
        slot.icon = icon;*/

        return this;
    }
}
