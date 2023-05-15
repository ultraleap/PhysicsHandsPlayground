using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RotationVelocity : SimpleFloatOutput
{
    [System.Serializable]
    public enum RotationAxis
    {
        X,Y,Z
    }

    [SerializeField, HideInInspector]
    private Rigidbody _rigidbody;

    [SerializeField]
    private RotationAxis _rotationAxis = RotationAxis.X;
    public RotationAxis CurrentRotationAxis => _rotationAxis;

    private Vector3 _localAngularVelocity;

    [SerializeField]
    private float _currentRotationVelocity;

    private void OnValidate()
    {
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
    }

    public void FixedUpdate()
    {
        if (_rigidbody == null)
            return;

        if (_rigidbody.IsSleeping())
        {
            _currentRotationVelocity = 0f;
            Output = _currentRotationVelocity;
            return;
        }

        _localAngularVelocity = transform.InverseTransformDirection(_rigidbody.angularVelocity);

        switch (_rotationAxis)
        {
            case RotationAxis.X:
                _currentRotationVelocity = _localAngularVelocity.x;
                break;
            case RotationAxis.Y:
                _currentRotationVelocity = _localAngularVelocity.y;
                break;
            case RotationAxis.Z:
                _currentRotationVelocity = _localAngularVelocity.z;
                break;
        }

        if(Mathf.Abs(_currentRotationVelocity) < Physics.sleepThreshold)
        {
            _currentRotationVelocity = 0f;
        }
        Output = _currentRotationVelocity * Time.fixedDeltaTime;
    }

}
