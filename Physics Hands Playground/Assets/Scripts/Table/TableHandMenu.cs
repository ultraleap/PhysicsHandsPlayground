using Leap.Unity.Attachments;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Leap.Unity.Interaction.PhysicsHands.Playground
{
    public class TableHandMenu : MonoBehaviour
    {
        [SerializeField]
        private List<TableButton> _tableButtons = new List<TableButton>();

        private SimpleFacingCameraCallbacks _cameraCallbacks;

        [SerializeField]
        private Transform _root;

        [SerializeField]
        private PhysicsHand _handToIgnore;
        private bool _hasIgnored = false;

        private bool _facingCamera = false, _grasping = false;
        private float _graspTimeout = 0.5f, _graspCurrent = 0f;

        [SerializeField]
        private PlayerRecenter _playerRecenter = null;

        [SerializeField, Tooltip("Set as null to start in the middle.")]
        private TableManager _startAtTable = null;

        public bool IsMenuVisible { get { return !_grasping && _handToIgnore.IsTracked && _facingCamera && _graspCurrent <= 0; } }

        private void Start()
        {
            if (!Application.isEditor)
            {
                _startAtTable = null;
            }

            foreach (var item in _tableButtons)
            {
                if (_startAtTable == null)
                {
                    _startAtTable = item.Table;
                }
                item.PhysicsButton.OnPress += () => { SetCurrentTable(item.Table); };
            }

            SetHandToIgnore();

            _cameraCallbacks = GetComponent<SimpleFacingCameraCallbacks>();
            _cameraCallbacks.OnBeginFacingCamera.AddListener(() => { _facingCamera = true; RecalcButtons(); });
            _cameraCallbacks.OnEndFacingCamera.AddListener(() => { _facingCamera = false; RecalcButtons(); });

            _handToIgnore.OnBeginPhysics += OnBeginPhysics;
            _handToIgnore.OnUpdatePhysics += OnUpdatePhysics;
            _handToIgnore.OnFinishPhysics += OnFinishPhysics;

            if (_startAtTable != null)
            {
                StartCoroutine(ZeroTable());
            }
        }

        private IEnumerator ZeroTable()
        {
            yield return null;
            yield return null;
            SetCurrentTable(_startAtTable, true);
        }

        private void OnBeginPhysics()
        {
            if (!_hasIgnored)
            {                    
                _hasIgnored = true;

                Collider[] colliders = _root.GetComponentsInChildren<Collider>(true);

                foreach (var button in colliders)
                {
                    Physics.IgnoreCollision(button, _handToIgnore.GetPhysicsHand().palmCollider);
                }

                foreach (var bone in _handToIgnore.GetPhysicsHand().jointColliders)
                {
                    foreach (var button in colliders)
                    {
                        Physics.IgnoreCollision(button, bone);
                    }
                }
            }
            RecalcButtons();
        }

        private void OnUpdatePhysics()
        {
            if (_grasping != _handToIgnore.IsGrasping)
            {
                _grasping = _handToIgnore.IsGrasping;
                if (!_grasping)
                {
                    _graspCurrent = _graspTimeout;
                }
                RecalcButtons();
            }
        }

        private void OnFinishPhysics()
        {
            RecalcButtons();
        }

        private void FixedUpdate()
        {
            if (_graspCurrent > 0)
            {
                _graspCurrent -= Time.fixedDeltaTime;
            }
            RecalcButtons();
        }

        private void RecalcButtons()
        {
            if (_root.gameObject.activeInHierarchy != IsMenuVisible)
            {
                _root.gameObject.SetActive(IsMenuVisible);
            }
        }

        private void SetCurrentTable(TableManager table, bool force = false)
        {
            if (!force)
            {
                if (!IsMenuVisible)
                    return;
            }

            _playerRecenter.anchor = table.TeleportToTable().gameObject;
            _playerRecenter.Recenter();
        }

        private void OnValidate()
        {
            _tableButtons = GetComponentsInChildren<TableButton>().ToList();
            _playerRecenter = FindObjectOfType<PlayerRecenter>();
            SetHandToIgnore();
        }

        private void SetHandToIgnore()
        {
            if (_handToIgnore == null)
            {
                PhysicsProvider provider = FindObjectOfType<PhysicsProvider>(true);
                if (provider != null)
                {
                    AttachmentHand attachmentHand = GetComponentInParent<AttachmentHand>(true);
                    if (attachmentHand != null)
                    {
                        if (attachmentHand.chirality == Leap.Unity.Chirality.Left)
                        {
                            _handToIgnore = provider.LeftHand;
                        }
                        else
                        {
                            _handToIgnore = provider.RightHand;
                        }
                    }
                }
            }
        }
    }
}
