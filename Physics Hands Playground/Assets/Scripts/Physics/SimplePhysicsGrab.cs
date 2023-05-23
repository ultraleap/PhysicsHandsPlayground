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
            _physicsProvider.OnObjectStateChange += OnObjectStateChange;
        }
    }

    private void OnObjectStateChange(Rigidbody rigid, PhysicsGraspHelper helper)
    {
        if (_rigidbodies.Contains(rigid))
        {
            if (helper.GraspState == PhysicsGraspHelper.State.Grasp)
            {
                _currentGrabbed.Add(rigid);
            }
            else
            {
                _currentGrabbed.Remove(rigid);
            }
            _grabbed = _currentGrabbed.Count > 0;
        }
    }
}
