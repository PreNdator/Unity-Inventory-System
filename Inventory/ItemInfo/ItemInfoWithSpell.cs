using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInfoWithSpell : ItemInfo
{
    [SerializeField]
    private Spell _spell = null;
    public Spell Spell => _spell;
}
