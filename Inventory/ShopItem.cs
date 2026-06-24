using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ShopItem : Item
{
    LocalizationManager _localization;

    [Inject]
    private void Construct(LocalizationManager localization)
    {
        _localization = localization;
    }

    protected override void TryPickup(PlayerController player)
    {
        if (Info is ItemInfoSellable sellable)
        {
            player.Inventory.CmdTryPickup(this, sellable.Price);
        }
        else
        {
            player.Inventory.CmdTryPickup(this, 0);
        }
    }

    protected override void Start()
    {
        base.Start();
        if (Info is ItemInfoSellable sellable) {
            _interactable.ChangeText($"{_localization.GetText(sellable.ItemName)}\n" +
                $"{_localization.GetText("costs")} {sellable.Price}");
        }
    }
}
