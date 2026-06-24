using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemInfo", menuName = "Game/Item/Money")]
public class ItemInfoMoney : ItemInfo
{
    [SerializeField]
    private int _count = 1;
    public int Count => _count;
}
