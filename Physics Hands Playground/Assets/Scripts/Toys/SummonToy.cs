using Leap;
using Leap.Interaction.Internal.InteractionEngineUtility;
using Leap.Unity;
using UnityEngine;
using Leap.Unity.Interaction.PhysicsHands;
using System.Collections.Generic;

public class SummonToy : MonoBehaviour
{

    private PhysicsProvider _physicsProvider = null;

    private Camera _camera = null;

    [SerializeField]
    private bool _useCustomPosition = false;
    [SerializeField]
    private Transform _customPosition = null;

    [SerializeField]
    private bool _useLeftHand = true, _useRightHand = true;

    [SerializeField]
    private LeapProvider _inputProvider;

    [SerializeField]
    private bool _useVelocity = true;
    [SerializeField]
    private float _velocityThreshold = 0.4f;

    [SerializeField]
    private bool _usePinch = true, _useGrab = false;
    [SerializeField]
    private float _pinchDistance = 0.02f;
    [SerializeField]
    private float _grabThreshold = 0.8f;

    [SerializeField, Tooltip("Minimum distance to be considered for summoning.")]
    private float _minimumDistance = 0.2f;
    private float _rayRadius = 0.02f;

    [SerializeField]
    private float _endingFloatTime = 0.8f, _cooldownTime = 0.5f;
    private float _currentLerp = 0f;

    private bool _isSummoning = false;
    private Rigidbody _currentTarget = null, _summoningItem = null;
    private RaycastHit[] _rayCache = new RaycastHit[32];
    private Collider[] _colliderCache = new Collider[32];
    private Rigidbody[] _rigidCache = new Rigidbody[32];
    private Vector3[] _points = new Vector3[32];
    private int _rigidCount = 0;
    private Vector3 _startPosition, _targetPosition, _summonPosition;
    private float _speed, _timeOfFlight;

    private bool _origKinematic = false, _origGrav = false;

    private Vector3[] _targetPoints = new Vector3[10];

    private Vector3 _debugPoint;
    [SerializeField]
    private Transform _debug;

    private bool _releasePoseL, _releasePoseR, _releaseVelL, _releaseVelR;

    private List<SummonData> _summons = new List<SummonData>();

    private class SummonData
    {
        public Rigidbody rigid;
        public Vector3 start, end;
        public float currentLerp;
        public float time, cooldownTime;
        public float cooldown;
        public int state = 0;
        public bool origKinematic, origGrav;

        private PhysicsProvider _provider;
        private PhysicsGraspHelper.State _state = PhysicsGraspHelper.State.Hover;
        public bool physicsAffected = false;
        private bool _hovering = false;

        public SummonData(Rigidbody rigid, Vector3 end, float cooldown, PhysicsProvider physicsProvider)
        {
            this.rigid = rigid;
            start = rigid.position;
            this.end = end;
            currentLerp = 0f;
            time = Vector3.Distance(start, end);
            this.cooldown = cooldown;
            cooldownTime = 0f;
            origKinematic = rigid.isKinematic;
            origGrav = rigid.useGravity;
            state = 0;

            _provider = physicsProvider;
            if (_provider != null)
            {
                _provider.SubscribeToStateChanges(rigid, GraspState);
            }
        }

        public void Remove()
        {
            if (_provider != null)
            {
                _provider.UnsubscribeFromStateChanges(rigid, GraspState);
            }
            rigid.isKinematic = origKinematic;
            rigid.useGravity = origGrav;
        }

        public void Update()
        {
            switch (state)
            {
                case 0:
                    if (currentLerp >= 1f)
                    {
                        currentLerp = 1f;
                    }
                    PhysicsMovement(SampleParabola(start, end, time * .2f, currentLerp.EaseOut()), rigid);
                    if (currentLerp < 1f)
                    {
                        currentLerp += Time.fixedDeltaTime * (1.0f / (_hovering ? time * 2.5f : time));
                    }
                    else
                    {
                        state = 1;
                    }
                    break;
                case 1:
                    PhysicsMovement(end, rigid);
                    if (cooldownTime < 1f)
                    {
                        cooldownTime += Time.fixedDeltaTime * (1.0f / cooldown);
                    }
                    else
                    {
                        state = 3;
                    }
                    break;
            }
        }

        public void Reset(Vector3 target)
        {
            start = rigid.position;
            end = target;
            currentLerp = 0f;
            cooldownTime = 0f;
            state = 0;
            time = Vector3.Distance(start, end);
        }

        public void GraspState(PhysicsGraspHelper helper)
        {
            if (helper == null)
            {
                physicsAffected = false;
                _hovering = false;
                return;
            }
            _hovering = true;

            if (helper.GraspState != PhysicsGraspHelper.State.Grasp && _state == PhysicsGraspHelper.State.Grasp)
            {
                rigid.isKinematic = origKinematic;
                rigid.useGravity = origGrav;
            }
            _state = helper.GraspState;
            if (_state == PhysicsGraspHelper.State.Grasp)
            {
                physicsAffected = true;
                state = 3;
            }
            else
            {
                physicsAffected = false;
            }
        }

        private static Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t)
        {
            float parabolicT = t * 2 - 1;
            if (Mathf.Abs(start.y - end.y) < 0.05f)
            {
                //start and end are roughly level, pretend they are - simpler solution with less steps
                Vector3 travelDirection = end - start;
                Vector3 result = start + t * travelDirection;
                result.y += (-parabolicT * parabolicT + 1) * height;
                return result;
            }
            else
            {
                //start and end are not level, gets more complicated
                Vector3 travelDirection = end - start;
                Vector3 levelDirecteion = end - new Vector3(start.x, end.y, start.z);
                Vector3 right = Vector3.Cross(travelDirection, levelDirecteion);
                Vector3 up = Vector3.Cross(right, travelDirection);
                if (end.y > start.y) up = -up;
                Vector3 result = start + t * travelDirection;
                result += ((-parabolicT * parabolicT + 1) * height) * up.normalized;
                return result;
            }
        }

        private static void PhysicsMovement(Vector3 solvedPosition,
                               Rigidbody intObj)
        {
            Vector3 solvedCenterOfMass = intObj.rotation * intObj.centerOfMass + solvedPosition;
            Vector3 currCenterOfMass = intObj.rotation * intObj.centerOfMass + intObj.position;

            Vector3 targetVelocity = PhysicsUtility.ToLinearVelocity(currCenterOfMass, solvedCenterOfMass, Time.fixedDeltaTime);
            intObj.velocity = targetVelocity;
        }
    }

    private void Awake()
    {
        _camera = Camera.main;
        _physicsProvider = FindObjectOfType<PhysicsProvider>(true);
    }

    // Update is called once per frame
    void Update()
    {
        AcquireTarget();
        if (_currentTarget != null && AreHandStatesGood())
        {
            Summon();
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < _summons.Count; i++)
        {
            if (_summons[i].state == 3 && !_summons[i].physicsAffected)
            {
                _summons[i].Remove();
                _summons.RemoveAt(i);
                i--;
                continue;
            }
            _summons[i].Update();
        }
    }

    private void AcquireTarget()
    {
        if (_customPosition != null)
        {
            int hits = Physics.OverlapSphereNonAlloc(_customPosition.position, _rayRadius, _colliderCache);
            _rigidCount = 0;
            for (int i = 0; i < hits; i++)
            {
                if (_colliderCache[i].attachedRigidbody != null)
                {
                    _rigidCache[_rigidCount] = _colliderCache[i].attachedRigidbody;
                    _points[_rigidCount] = _colliderCache[i].ClosestPoint(_customPosition.position);
                    _rigidCount++;
                }
            }
        }
        else
        {

            int hits = Physics.SphereCastNonAlloc(_camera.transform.position + (_camera.transform.forward * _minimumDistance), _rayRadius, _camera.transform.forward, _rayCache, 15f);
            _rigidCount = 0;
            for (int i = 0; i < hits; i++)
            {
                if (_rayCache[i].rigidbody != null)
                {
                    _rigidCache[_rigidCount] = _rayCache[i].rigidbody;
                    _points[_rigidCount] = _rayCache[i].point;
                    _rigidCount++;
                }
            }
        }

        _currentTarget = null;
        float dist = float.MaxValue, temp;
        for (int i = 0; i < _rigidCount; i++)
        {
            temp = Vector3.Distance(_rigidCache[i].transform.position, _useCustomPosition ? _customPosition.position : _camera.transform.position);
            if (temp < dist)
            {
                dist = temp;
                _debugPoint = _points[i];
                _currentTarget = _rigidCache[i];
            }
        }
        _debug.gameObject.SetActive(_currentTarget != null);
        _debug.position = _debugPoint;
    }

    private bool AreHandStatesGood()
    {
        if (_useLeftHand && IsHandGood(_inputProvider.GetHand(Chirality.Left)))
        {
            Hand hand = _inputProvider.GetHand(Chirality.Left);
            _targetPosition = hand.PalmPosition + (hand.Direction * 0.06f) + (hand.PalmNormal * 0.04f);
            return true;
        }
        if (_useRightHand && IsHandGood(_inputProvider.GetHand(Chirality.Right)))
        {
            Hand hand = _inputProvider.GetHand(Chirality.Right);
            _targetPosition = hand.PalmPosition + (hand.Direction * 0.06f) + (hand.PalmNormal * 0.04f);
            _releasePoseR = true;
            _releaseVelR = true;
            return true;
        }
        return false;
    }

    private bool IsHandGood(Hand hand)
    {
        if (hand == null)
            return false;

        Chirality handedness = hand.GetChirality();

        if (handedness == Chirality.Left)
        {
            if (_physicsProvider.LeftHand.IsGrasping)
            {
                return false;
            }
        }
        else
        {
            if (_physicsProvider.RightHand.IsGrasping)
            {
                return false;
            }
        }

        // If we're neither wanting pinch or grab
        bool pose = !_usePinch && !_useGrab;
        if ((_usePinch && (hand.PinchDistance / 1000f) < _pinchDistance) || (_useGrab && hand.GetFistStrength() > _grabThreshold))
        {
            if((handedness == Chirality.Left && !_releasePoseL) || (handedness == Chirality.Right && !_releasePoseR))
            {
                pose = true;
            }
        }
        else
        {
            if(handedness == Chirality.Left)
            {
                _releasePoseL = false;
            }
            else
            {
                _releasePoseR = false;
            }
        }

        // Ignore velocity if off
        bool vel = !_useVelocity;
        if (hand.PalmVelocity.y > _velocityThreshold)
        {
            if ((handedness == Chirality.Left && !_releaseVelL) || (handedness == Chirality.Right && !_releaseVelR))
            {
                vel = true;
            }
        }
        else
        {
            if (handedness == Chirality.Left)
            {
                _releaseVelL = false;
            }
            else
            {
                _releaseVelR = false;
            }
        }

        return pose && vel;
    }

    private void Summon()
    {
        int exists = _summons.FindIndex(x => x.rigid == _currentTarget);

        if (exists == -1)
        {
            SummonData sd = new SummonData(_currentTarget, _targetPosition, 0.5f, _physicsProvider);
            _summons.Add(sd);
        }
        else
        {
            SummonData sd = _summons[exists];
            sd.Reset(_targetPosition);
        }
    }
}
