using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

//[CreateAssetMenu(fileName = "ItemInfo", menuName = "Game/Item/ItemInfo")]
public class ItemInfo : ScriptableObject
{
    [SerializeField]
    private Texture2D _texture;

    [SerializeField]
    private string _name;
    [SerializeField]
    private string _description = "d_default_item";

    [SerializeField]
    private Sprite _sprite;
    public Texture2D Texture => _texture;
    public string ItemName => _name;
    public string Description => _description;


    public Sprite Sprite
    {
        get
        {
            if (_sprite == null && _texture != null)
            {
                _sprite = Sprite.Create(
                    _texture,
                    new Rect(0, 0, _texture.width, _texture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
            return _sprite;
        }
    }
}
