using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

[RequireComponent(typeof(Rigidbody))]
public class CustomRigidbodySetup : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private Rigidbody _rigidbody;

    [SerializeField]
    private bool _adjustInertiaTensor = false;
    [SerializeField]
    private Vector3 _inertiaTensor = Vector3.one;
    [SerializeField, HideInInspector]
    private Vector3 _originalInertiaTensor = Vector3.zero;

    [SerializeField]
    private bool _adjustCenterOfMass = false;
    [SerializeField]
    private Vector3 _centerOfMassOffset = Vector3.zero;
    [SerializeField, HideInInspector]
    private Vector3 _originalCenterOfMass = Vector3.zero;


    private void Awake()
    {
        ApplySettings();
    }

    private void OnValidate()
    {
        ApplySettings();
    }

    private void ApplySettings()
    {
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
        if (_rigidbody != null)
        {
            if (_adjustCenterOfMass)
            {
                _rigidbody.centerOfMass = _centerOfMassOffset;
            }
            else
            {
                _rigidbody.ResetCenterOfMass();
                _originalCenterOfMass = _rigidbody.centerOfMass;
            }
            if (_adjustInertiaTensor)
            {
                _rigidbody.inertiaTensor = _inertiaTensor;
            }
            else
            {
                _rigidbody.ResetInertiaTensor();
                _originalInertiaTensor = _rigidbody.inertiaTensor;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.DrawDottedLine(transform.position, transform.position + (transform.rotation * (_adjustCenterOfMass ? _centerOfMassOffset : _originalCenterOfMass)), 5f);
        UnityEditor.Handles.color = new Color(Color.green.r, Color.green.g, Color.green.b, .25f);
        Vector3 inertia = _adjustInertiaTensor ? _inertiaTensor : _originalInertiaTensor;
        UnityEditor.Handles.DrawSolidDisc(transform.position + (transform.rotation * _centerOfMassOffset), transform.up, (inertia.x + inertia.y + inertia.z) / 3f);
#endif
    }
}
