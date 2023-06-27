using Leap.Unity.Interaction.PhysicsHands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SimplePhysicsGrab : MonoBehaviour
{
    [SerializeField]
    protected List<Rigidbody> _rigidbodies = new List<Rigidbody>();
    private HashSet<Rigidbody> _currentGrabbed = new HashSet<Rigidbody>();

    private bool _grabbed = false;
    public bool Grabbed => _grabbed;

    private PhysicsProvider _physicsProvider;

    protected virtual void Awake()
    {
        _physicsProvider = FindAnyObjectByType<PhysicsProvider>();
        if (_physicsProvider != null)
        {
            foreach (var item in _rigidbodies)
            {
                _physicsProvider.SubscribeToStateChanges(item, OnObjectStateChange);
            }
        }
    }

    private void OnObjectStateChange(PhysicsGraspHelper helper)
    {
        if (helper.GraspState == PhysicsGraspHelper.State.Grasp)
        {
            _currentGrabbed.Add(helper.Rigidbody);
        }
        else
        {
            _currentGrabbed.Remove(helper.Rigidbody);
        }
        _grabbed = _currentGrabbed.Count > 0;
    }
}
