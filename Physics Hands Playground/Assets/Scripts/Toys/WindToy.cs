using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindToy : MonoBehaviour
{
    [SerializeField]
    private SimpleFloatOutput _inputValue;

    [SerializeField]
    private float _forceScale = 0.1f, _lengthScale = 0.1f;

    [SerializeField]
    private float _radius = 0.1f;

    [SerializeField]
    private List<Rigidbody> _bodiesToIgnore = new List<Rigidbody>();

    private List<Rigidbody> _currentEffects = new List<Rigidbody>();
    private Vector3 _currentForce;

    RaycastHit[] _rayCache = new RaycastHit[64];

    private void FixedUpdate()
    {
        _currentEffects.Clear();
        if (_inputValue.Output == 0)
            return;

        int count = Physics.SphereCastNonAlloc(transform.position, _radius, transform.up, _rayCache,_inputValue.Output * _lengthScale);
        for (int i = 0; i < count; i++)
        {
            if(_rayCache[i].collider.attachedRigidbody != null && !_bodiesToIgnore.Contains(_rayCache[i].collider.attachedRigidbody))
            {
                _currentForce = transform.up * _inputValue.Output * _forceScale;
                _currentEffects.Add(_rayCache[i].collider.attachedRigidbody);
                _rayCache[i].collider.attachedRigidbody.AddForce(transform.up * _inputValue.Output * _forceScale);
                Debug.DrawLine(transform.position, transform.position + (transform.up * _inputValue.Output * _lengthScale), Color.red, Time.fixedDeltaTime);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position, _radius);
    }

}
