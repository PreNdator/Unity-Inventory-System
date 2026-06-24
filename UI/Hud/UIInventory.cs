using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIInventory : MonoBehaviour
{
    private RectTransform _itemsCenter;

    [SerializeField]
    private GameObject _staffPrefab;
    [SerializeField]
    private UIItemIcon _itemCellPrefab;
    [SerializeField]
    private UIItemIcon _spellCellPrefab;
    [SerializeField]
    private UIItemText _moneyCellPrefab;
    [SerializeField]
    private float _spacing = 70f;
    [SerializeField]
    private float _itemSize = 70f;
    [SerializeField]
    private float _selectedScale = 1.1f;
    [SerializeField]
    private float _animationDuration = 0.3f;
    [SerializeField]
    private Ease UpscaleEase;
    [SerializeField]
    private Ease DownscaleEase;
    [SerializeField]
    private float _fadeDuration = 1f;
    [SerializeField]
    private Ease _fadeEase = Ease.Linear;
    private int _currSlot = 0;

    [Header("Staff Bobbing")]
    [SerializeField] private Ease _staffBobEaseForward = Ease.InOutSine;
    [SerializeField] private float _staffBobOffsetY = 8f;
    [SerializeField] private float _bobAnimationDuration = 1f;
    [SerializeField] private float _scaleMultiplyer = 1.2f;

    private Tween _staffBobTween;
    private Tween _staffBiggerTween;

    private PlayerInventory _inventory;
    private List<UIItemIcon> _spellCells = new List<UIItemIcon>();
    private List<UIItemIcon> _itemCells = new List<UIItemIcon>();
    private GameObject _staff;
    private UIItemText _moneyText;

    private TMP_Text _changeItemText;
    private LocalizationManager _localization;
    private float _startPositionX;
    private float _staffInitY;
    private float _baseScale;


    [Inject]
    private void Construct(HudHolder ui, LocalizationManager localization)
    {
        
        _itemsCenter = ui.ItemsCenter;
        _changeItemText = ui.ItemsText;
        _localization = localization;
    }

    public void PlayStaffBobOnce()
    {
        if (_staff == null) return;

        _staffBobTween?.Kill();

        float baseY = _staffInitY;
        float targetY = baseY + _staffBobOffsetY;

        _staffBobTween = _staff.transform
            .DOLocalMoveY(targetY, _bobAnimationDuration * 0.5f)
            .SetEase(_staffBobEaseForward)
            .SetLink(_staff.gameObject, LinkBehaviour.KillOnDestroy)
            .OnComplete(() =>
            {
                _staffBobTween = _staff.transform
                    .DOLocalMoveY(baseY, _bobAnimationDuration * 0.5f)
                    .SetEase(_staffBobEaseForward)
                    .SetLink(_staff.gameObject, LinkBehaviour.KillOnDestroy); ;
            });
    }

    public void PlayStaffBigger()
    {
        if (_staff == null) return;

        _staffBiggerTween?.Kill();

        float baseScale = _baseScale;
        float targetScale = _baseScale*_scaleMultiplyer;

        

        _staffBiggerTween = _staff.transform
            .DOScale(targetScale, _bobAnimationDuration * 0.5f)
            .SetEase(_staffBobEaseForward)
            .SetLink(_staff.gameObject, LinkBehaviour.KillOnDestroy)
            .OnComplete(() =>
            {
                _staffBiggerTween = _staff.transform
                    .DOScale(baseScale, _bobAnimationDuration * 0.5f)
                    .SetEase(_staffBobEaseForward)
                    .SetLink(_staff.gameObject, LinkBehaviour.KillOnDestroy);
            });
    }

    public void SetInventory(PlayerInventory inventory)
    {
        if (_inventory != null)
        {
            _inventory.OnSpellChanged -= UpdateSpellSlot;
            _inventory.OnItemChanged -= UpdateItemSlot;
            _inventory.OnMoneyChange -= UpdateMoney;
            _inventory.OnCurrSlotChanged -= SlotChanged;
        }

        _inventory = inventory;
        _inventory.OnSpellChanged += UpdateSpellSlot;
        _inventory.OnItemChanged += UpdateItemSlot;
        _inventory.OnMoneyChange += UpdateMoney;
        _inventory.OnCurrSlotChanged += SlotChanged;

        InitializeUI();
    }


    private void OnDestroy()
    {
        if (_inventory != null)
        {
            _inventory.OnSpellChanged -= UpdateSpellSlot;
            _inventory.OnItemChanged -= UpdateItemSlot;
            _inventory.OnMoneyChange -= UpdateMoney;
            _inventory.OnCurrSlotChanged -= SlotChanged;
        }

    }

    private void SlotChanged(int slot)
    {
        UpdateText(_inventory.GetText(slot));
        _currSlot = slot;


        _staff.transform.DOLocalMoveX(_startPositionX + slot * _spacing, _animationDuration).SetEase(Ease.OutBack);

        foreach (var itemCell in _itemCells)
        {
            itemCell.transform.DOScale(Vector3.one, _animationDuration).SetEase(DownscaleEase);
        }
        foreach (var spellCell in _spellCells)
        {
            spellCell.transform.DOScale(Vector3.one, _animationDuration).SetEase(DownscaleEase);
        }

        _moneyText.transform.DOScale(Vector3.one, _animationDuration).SetEase(DownscaleEase);

        if (slot == 0)
        {
            _moneyText.transform.DOScale(Vector3.one * _selectedScale, _animationDuration).SetEase(UpscaleEase);
        }
        if (slot > 0 && slot <= _itemCells.Count)
        {
            _itemCells[slot-1].transform.DOScale(Vector3.one * _selectedScale, _animationDuration).SetEase(UpscaleEase);
        }
        else if (slot > _itemCells.Count && slot <= _itemCells.Count + _spellCells.Count)
        {
            int spellSlotIndex = slot - _itemCells.Count-1;
            _spellCells[spellSlotIndex].transform.DOScale(Vector3.one * _selectedScale, _animationDuration).SetEase(UpscaleEase); 
        }
    }

    private void InitializeUI()
    {
        foreach (var cell in _spellCells) Destroy(cell.gameObject);
        foreach (var cell in _itemCells) Destroy(cell.gameObject);
        _spellCells.Clear();
        _itemCells.Clear();

        if (_staff != null)
        {
            Destroy(_staff);
            _staff = null;
        }
        

        _startPositionX = -((_inventory.Items + _inventory.Spells) / 2f) * _spacing;



        _staff = Instantiate(_staffPrefab, _itemsCenter);
        _staffInitY = 0;
        _staff.transform.localPosition = new Vector3(_startPositionX, 0, 0);
        _baseScale = _staff.transform.localScale.x;

        for (int i = 0; i < _inventory.Items; i++)
        {
            UIItemIcon itemCell = Instantiate(_itemCellPrefab, _itemsCenter);
            itemCell.transform.localPosition = new Vector3(_startPositionX + (i + 1) * _spacing, 0, 0);
            _itemCells.Add(itemCell);
        }

        for (int i = 0; i < _inventory.Spells; i++)
        {
            UIItemIcon spellCell = Instantiate(_spellCellPrefab, _itemsCenter);
            spellCell.transform.localPosition = new Vector3(
                _startPositionX + (1 + _inventory.Items + i) * _spacing,
                0,
                0
            );
            _spellCells.Add(spellCell);
        }


        if (_moneyText == null)
        {
            _moneyText = Instantiate(_moneyCellPrefab, _itemsCenter);
            _moneyText.transform.localPosition = new Vector3(_startPositionX, 0, 0);
            UpdateMoney(_inventory.Coins);
        }

    }

    private void UpdateText(string text)
    {
        if (_changeItemText != null)
        {
            _changeItemText.DOKill();
            _changeItemText.text = text;
            _changeItemText.alpha = 1;
            _changeItemText.DOFade(0, _fadeDuration).SetEase(_fadeEase);
        }
    }

    private void UpdateItemSlot(int slot, ItemInfoSellable itemInfo)
    {
        
        if (slot >= 0 && slot < _itemCells.Count)
        {
            Image iconImage = _itemCells[slot].Image;
            if (iconImage != null)
            {
                iconImage.sprite = itemInfo != null ? itemInfo.Sprite : null;
                iconImage.enabled = itemInfo != null;
                if (itemInfo != null)
                {
                    iconImage.rectTransform.sizeDelta = new Vector2(_itemSize, _itemSize);
                }
            }
        }
        int slotGlobal = slot + 1;
        if (slotGlobal == _currSlot)
        {
            UpdateText(_inventory.GetText(slotGlobal));
        }
    }

    private void UpdateSpellSlot(int slot, ItemInfoSpellOnly spellInfo)
    {
        if (slot >= 0 && slot < _spellCells.Count)
        {
            Image iconImage = _spellCells[slot].Image;
            
            if (iconImage != null)
            {
                iconImage.sprite = spellInfo != null ? spellInfo.Sprite : null;
                iconImage.enabled = spellInfo != null;
                if (spellInfo != null)
                {
                    iconImage.rectTransform.sizeDelta = new Vector2(_itemSize, _itemSize);
                }
            }
        }
        int slotGlobal = slot + _itemCells.Count + 1;
        if (slotGlobal == _currSlot)
        {
            UpdateText(_inventory.GetText(slotGlobal));
        }
    }

    private void UpdateMoney(int newAmount)
    {
        if (_moneyText != null)
        {
            _moneyText.Text.text = newAmount.ToString();
            if (0 == _currSlot)
            {
                UpdateText(_inventory.GetText(0));
            }
        }
    }
}
