using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MaterialItemSlot : MonoBehaviour
{
    public Image icon;
    public MaterialItem item;

    public void initMaterialItemSlot()
    {
        if(GetComponentsInChildren<Image>().Length != 0) icon = GetComponentsInChildren<Image>()[1];
    }

    public void AddIcon(MaterialItem item)
    {
        if (icon == null) initMaterialItemSlot();

        this.item = item;
        icon.sprite = item.itemIcon;
        icon.enabled = true;
        gameObject.SetActive(true);

    }

    public void ClearIcom()
    {
        if (icon == null) initMaterialItemSlot();

        item = null;
        icon.sprite = null;
        icon.enabled = false;
        gameObject.SetActive(false);
    }

    public MaterialItem getItem()
    {
        return item.getDeepCopy();
    }
}
