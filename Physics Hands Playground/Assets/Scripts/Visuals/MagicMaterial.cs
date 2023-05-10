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
    public Color Color { get { return _color; } set { _color = value; ApplyPropertyBlock(); } }

    [SerializeField]
    private bool _sampleGradient = false;
    public bool SampleGradient { get { return _sampleGradient; } set { _sampleGradient = value; ApplyPropertyBlock(); } }

    [SerializeField, Range(0f, 1f)]
    private float _gradientSamplePoint = 0f;
    public float GradientSamplePoint { get { return _gradientSamplePoint; } set { _gradientSamplePoint = value; ApplyPropertyBlock(); } }

    [SerializeField]
    private Gradient _gradient = null;
    public Gradient Gradient { get { return _gradient; } set { _gradient = value; ApplyPropertyBlock(); } }

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
