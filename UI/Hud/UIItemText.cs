using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class UIItemText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text;
    public TMP_Text Text => _text;
}
