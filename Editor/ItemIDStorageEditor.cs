using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemIDStorage))]
public class ItemIDStorageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ItemIDStorage storage = (ItemIDStorage)target;

        if (GUILayout.Button("Populate ItemInfo List"))
        {
            storage.PopulateItemInfoList();

            EditorUtility.SetDirty(storage);
        }
    }
}
