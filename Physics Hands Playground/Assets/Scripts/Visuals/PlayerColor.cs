using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColor : MonoBehaviour
{

    [SerializeField]
    private bool _useSpecificColor = false;

    [SerializeField]
    private int _specificColorIndex = 0;

    [SerializeField]
    private List<Color> _colors = new List<Color>();

    private void Start()
    {
        ApplyColours();
    }

    private void OnValidate()
    {
        _specificColorIndex = Mathf.Clamp(_specificColorIndex, 0, _colors.Count);
    }

    private void ApplyColours()
    {
        MagicMaterial[] magicMaterials = GetComponentsInChildren<MagicMaterial>(true);
        Color color = _colors[_useSpecificColor ? _specificColorIndex : Random.Range(0, _colors.Count)];
        foreach (MagicMaterial material in magicMaterials)
        {
            material.Color = color;
        }
    }
}
