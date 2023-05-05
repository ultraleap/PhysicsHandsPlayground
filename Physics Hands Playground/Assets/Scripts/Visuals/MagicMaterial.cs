using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMaterial : MonoBehaviour
{
    private Renderer _renderer;

    [SerializeField]
    private string _materialColorVar = "_BaseColor";

    [SerializeField]
    private Color _color = Color.black;

    [SerializeField]
    private bool _sampleGradient = false;

    [SerializeField, Range(0f, 1f)]
    private float _gradientSamplePoint = 0f;

    [SerializeField]
    private Gradient _gradient = null;

    private void Awake()
    {
        ApplyPropertyBlock();
    }

    private void OnValidate()
    {
        ApplyPropertyBlock();
    }

    private void ApplyPropertyBlock()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }
        if (_renderer != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor(_materialColorVar, _sampleGradient ? _gradient.Evaluate(_gradientSamplePoint) : _color);
            _renderer.SetPropertyBlock(block);
        }
    }
}
