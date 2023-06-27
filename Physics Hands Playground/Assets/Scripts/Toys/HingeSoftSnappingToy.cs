using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class HingeSoftSnappingToy : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private ConfigurableJoint _joint;

    private Quaternion _localRotationStart;
    private Vector3 _originalUp;

    [SerializeField]
    private List<float> _rotationsToSnapTo = new List<float>();

    private float _currentRotaton;

    [SerializeField]
    private float _rotationThreshold = 5f;

    private bool _hasBroken = false;

    private void OnValidate()
    {
        if (_joint == null)
        {
            _joint = GetComponent<ConfigurableJoint>();
        }
    }

    private void Awake()
    {
        _localRotationStart = transform.localRotation;
        _currentRotaton = _rotationsToSnapTo[0];
        _originalUp = transform.parent.InverseTransformDirection(transform.up);
        _joint.SetTargetRotationLocal(Quaternion.Euler(_currentRotaton, 0, 0), _localRotationStart);
    }

    private void FixedUpdate()
    {
        if (_joint == null)
        {
            if(!_hasBroken)
            {
                Rigidbody r = GetComponent<Rigidbody>();
                if(r != null)
                {
                    r.angularDrag = 0.05f;
                }
            }
            return;
        }

        float angle = Vector3.Angle(_originalUp, transform.parent.InverseTransformDirection(transform.up));
        if (Vector3.Cross(_originalUp, transform.parent.InverseTransformDirection(transform.up)).x < 0)
        {
            angle = -angle;
        }
        float closest = ClosestTo(_rotationsToSnapTo, angle);
        if (Mathf.Abs(closest - angle) < _rotationThreshold && _currentRotaton != closest)
        {
            _joint.SetTargetRotationLocal(Quaternion.Euler(closest, 0, 0), _localRotationStart);
            _currentRotaton = closest;
        }
    }

    private float ClosestTo(IEnumerable<float> collection, float target)
    {
        return collection.Aggregate((x, y) => Mathf.Abs(x - target) < Mathf.Abs(y - target) ? x : y);
    }

}
