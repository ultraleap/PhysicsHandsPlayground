using Leap.PhysicalHands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SimplePhysicsGrab : MonoBehaviour
{
    [SerializeField]
    protected List<PhysicalHandEvents> _eventObjects = new List<PhysicalHandEvents>();
    private HashSet<PhysicalHandEvents> _currentGrabbed = new HashSet<PhysicalHandEvents>();

    private bool _grabbed = false;
    public bool Grabbed => _grabbed;

    protected virtual void Awake()
    {
        foreach (var item in _eventObjects)
        {
            item.onGrabEnter.AddListener((hand) => { OnObjectGrabChange(item, true); });
            item.onGrabExit.AddListener((hand) => { OnObjectGrabChange(item, false); });
        }
    }

    private void OnObjectGrabChange(PhysicalHandEvents events, bool grabbed)
    {
        if (grabbed)
        {
            _currentGrabbed.Add(events);
        }
        else
        {
            _currentGrabbed.Remove(events);
        }
        _grabbed = _currentGrabbed.Count > 0;
    }
}
