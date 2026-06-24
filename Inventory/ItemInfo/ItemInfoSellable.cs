
using UnityEngine;


[CreateAssetMenu(fileName = "ItemInfo", menuName = "Game/Item/ItemInfoSellable")]
public class ItemInfoSellable : ItemInfoWithSpell
{
    [SerializeField]
    private int _price = 0;
    [SerializeField]
    private int _sellPrice = 3;
    public int Price => _price;
    public int SellPrice => _sellPrice;
}
