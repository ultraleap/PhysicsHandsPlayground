using Leap.Unity.Interaction.PhysicsHands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParachuteToy : MonoBehaviour
{
    [SerializeField]
    private Rigidbody _character, _parachute;

    private PhysicsProvider _physicProvider;

    [SerializeField]
    private float _yThreshold = -0.2f;

    [SerializeField]
    private float _yTimeIn = 0.25f, _yTimeOut = 1f;
    private float _yTimeInCurrent = 0f, _yTimeOutCurrent = 1f;

    private bool _characterGrabbed = false, _parachuteActive = false;

    [SerializeField, Tooltip("The drag applied to the character when parachuting.")]
    private float _parachutingDrag = 5f, _parachutingAngularDrag = 2.5f;
    private float _originalDrag, _originalAngularDrag;
    private Vector3 _originalParachutePos;

    private ConfigurableJoint _currentJoint;

    private void OnValidate()
    {
        FindElements();
    }

    private void Start()
    {
        FindElements();
        Setup();
    }

    private void FindElements()
    {
        if (_physicProvider == null)
        {
            _physicProvider = FindObjectOfType<PhysicsProvider>(true);
        }
    }

    private void Setup()
    {
        if(_physicProvider != null)
        {
            _physicProvider.SubscribeToStateChanges(_character, OnObjectStateChange);
        }
        _originalDrag = _character.drag;
        _originalAngularDrag = _character.angularDrag;
        _originalParachutePos = _parachute.transform.position - _character.transform.position;
        Collider[] charCols = _character.GetComponentsInChildren<Collider>(true);
        Collider[] paraCols = _parachute.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < charCols.Length; i++)
        {
            for (int j = 0; j < paraCols.Length; j++)
            {
                Physics.IgnoreCollision(charCols[i], paraCols[j]);
            }
        }
    }

    private void OnObjectStateChange(PhysicsGraspHelper helper)
    {
        _characterGrabbed = helper.GraspState == PhysicsGraspHelper.State.Grasp;
    }

    private void FixedUpdate()
    {
        if (_parachuteActive)
        {
            if(_character.velocity.y > -0.1f)
            {
                _yTimeOutCurrent += Time.fixedDeltaTime;
            }
            else
            {
                _yTimeOutCurrent = 0f;
            }

            if(_characterGrabbed || _yTimeOutCurrent > _yTimeOut)
            {
                _yTimeOutCurrent = 0f;
                _parachuteActive = false;
                _parachute.gameObject.SetActive(false);
                if(_currentJoint != null)
                {
                    Destroy(_currentJoint);
                }
                _character.drag = _originalDrag;
                _character.angularDrag = _originalAngularDrag;
            }
        }
        else
        {
            if(_characterGrabbed)
            {
                _yTimeInCurrent = 0f;
                return;
            }
            if(_character.velocity.y < _yThreshold)
            {
                _yTimeInCurrent += Time.fixedDeltaTime;
            }
            else
            {
                _yTimeInCurrent = 0f;
            }
            if(_yTimeInCurrent > _yTimeIn)
            {
                _parachuteActive = true;
                _parachute.gameObject.SetActive(true);
                _parachute.transform.position = _character.position + (_character.rotation * _originalParachutePos);
                _parachute.transform.rotation = _character.rotation;
                _currentJoint = AddJoint(_parachute.gameObject, _character);

                _character.drag = _parachutingDrag;
                _character.angularDrag = _parachutingAngularDrag;
            }
        }
    }

    private ConfigurableJoint AddJoint(GameObject gameObject, Rigidbody connection)
    {
        ConfigurableJoint joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = connection;
        joint.connectedAnchor = _originalParachutePos;
        return joint;
    }
}
