using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ApplyForceToRigidbody : MonoBehaviour
{
    [SerializeField]
    private SimpleFloatOutput _inputFloat;

    [SerializeField, HideInInspector]
    private Rigidbody _rigidbody;

    [SerializeField]
    private ForceMode _forceMode = ForceMode.Force;

    [SerializeField]
    private bool _applyVelocity = false;
    [SerializeField]
    private Vector3 _velocityAxis = Vector3.zero;

    [SerializeField]
    private bool _applyAngularVelocity = false;
    [SerializeField]
    private Vector3 _angularVelocityAxis = Vector3.zero;

    private void OnValidate()
    {
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        if (_applyVelocity)
        {
            _rigidbody.AddRelativeForce(Vector3.Scale(Vector3.one * _inputFloat.Output, _velocityAxis), _forceMode);
        }
        if (_applyAngularVelocity)
        {
            _rigidbody.AddRelativeTorque(Vector3.Scale(Vector3.one * _inputFloat.Output, _angularVelocityAxis), _forceMode);
        }
    }
}
