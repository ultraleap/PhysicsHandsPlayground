using Leap.Unity.Interaction.PhysicsHands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderFloatOnGrab : SimplePhysicsGrab
{
    private List<Renderer> _renderers = new List<Renderer>();

    [SerializeField]
    private string _materialVariable = "";

    [SerializeField]
    private float _valueOnIdle = 0, _valueOnGrab = 1;

    private float _currentValue = 0f, _oldValue = 0f;

    [SerializeField]
    private float _lerpTime = 0.1f;

    private MaterialPropertyBlock _materialPropertyBlock;

    protected override void Awake()
    {
        base.Awake();
        foreach (var rigid in _rigidbodies)
        {
            if(rigid.TryGetComponent<Renderer>(out var temp))
            {
                _renderers.Add(temp);
            }
        }

        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        _currentValue += Time.deltaTime * (1.0f / _lerpTime) * (Grabbed ? 1 : -1);
        _currentValue = Mathf.Clamp(_currentValue, _valueOnIdle, _valueOnGrab);

        if(Mathf.Abs(_currentValue - _oldValue) > 1e-3f)
        {
            UpdateMaterials();
        }
    }

    private void UpdateMaterials()
    {
        _materialPropertyBlock.SetFloat(_materialVariable, _currentValue);
        foreach (var renderer in _renderers)
        {
            renderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}