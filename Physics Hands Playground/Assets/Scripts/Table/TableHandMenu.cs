using Leap.Attachments;
using Leap.PhysicalHands;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Leap.PhysicalHands.Playground
{
    public class TableHandMenu : MonoBehaviour
    {
        [SerializeField]
        private List<TableButton> _tableButtons = new List<TableButton>();

        private SimpleFacingCameraCallbacks _cameraCallbacks;

        [SerializeField]
        private Transform _root;

        [SerializeField]
        private Chirality _handToIgnore;
        private PhysicalHandsManager _physicalHandsManager;
        private bool _hasIgnored = false;

        private bool _facingCamera = false, _grabbingLeft = false, _grabbingRight = false;
        private float _graspTimeout = 0.5f, _graspCurrent = 0f;

        private float _changeTime = 0.25f, _currentChangeTimer = 0f;


        [SerializeField]
        private PlayerRecenter _playerRecenter = null;

        private TableManager _currentTable = null;
        [SerializeField, Tooltip("Set as null to start in the middle.")]
        private TableManager _startAtTable = null;

        private ContactHand _ignoredHand { get { return _handToIgnore == Chirality.Left ? _physicalHandsManager.ContactParent.LeftHand : _physicalHandsManager.ContactParent.RightHand; } }
        private ContactHand _otherHand { get { return _handToIgnore == Chirality.Left ? _physicalHandsManager.ContactParent.RightHand : _physicalHandsManager.ContactParent.LeftHand; } }

        public bool IsMenuVisible
        {
            get
            {
                if(_physicalHandsManager == null)
                {
                    _physicalHandsManager = FindAnyObjectByType<PhysicalHandsManager>();
                }

                if(_physicalHandsManager == null || _physicalHandsManager.ContactParent == null ||
                    _physicalHandsManager.ContactParent.LeftHand == null || _physicalHandsManager.ContactParent.RightHand == null)
                    return false;

                return (!_grabbingLeft && !_grabbingRight)
                    && (_handToIgnore == Chirality.Left ? _physicalHandsManager.ContactParent.LeftHand.Tracked : _physicalHandsManager.ContactParent.RightHand.Tracked)
                    && _facingCamera
                    && _graspCurrent <= 0;
            }
        }

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
                item.PhysicsButton.OnButtonPressed.AddListener(() => SetCurrentTable(item.Table));
            }

            if (_physicalHandsManager == null)
            {
                _physicalHandsManager = FindAnyObjectByType<PhysicalHandsManager>();
            }

            SetHandToIgnore();

            _physicalHandsManager.OnPrePhysicsUpdate += OnBeginPhysics;

            _cameraCallbacks = GetComponent<SimpleFacingCameraCallbacks>();
            _cameraCallbacks.OnBeginFacingCamera.AddListener(() => { ChangeVisible(true); });
            _cameraCallbacks.OnEndFacingCamera.AddListener(() => { ChangeVisible(false); });
        }

        private void ChangeVisible(bool visible)
        {
            _facingCamera = visible;
            RecalcButtons();
        }

        private void OnBeginPhysics()
        {
            if (!_hasIgnored)
            {
                _hasIgnored = true;

                Collider[] colliders = _root.GetComponentsInChildren<Collider>(true);

                foreach (var button in colliders)
                {
                    Physics.IgnoreCollision(button, _ignoredHand.palmBone.Collider);
                }

                foreach (var bone in _ignoredHand.bones)
                {
                    foreach (var button in colliders)
                    {
                        Physics.IgnoreCollision(button, bone.Collider);
                    }
                }
            }
            RecalcButtons();
        }

        private void FixedUpdate()
        {
            if(_currentChangeTimer > 0)
            {
                _currentChangeTimer -= Time.fixedDeltaTime;
            }
            if (_grabbingLeft != _ignoredHand.IsGrabbing)
            {
                _grabbingLeft = _ignoredHand.IsGrabbing;
                if (!_grabbingLeft)
                {
                    _graspCurrent = _graspTimeout;
                }
            }
            if(_grabbingRight != _otherHand.IsGrabbing)
            {
                _grabbingRight = _otherHand.IsGrabbing;
                if (!_grabbingRight)
                {
                    _graspCurrent = _graspTimeout;
                }
            }
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

        public void SetCurrentTable(TableManager table, bool force = false)
        {
            if (!force)
            {
                if (_currentChangeTimer > 0)
                    return;

                if (!IsMenuVisible)
                    return;
            }

            _playerRecenter.anchor = table.TeleportToTable().gameObject;
            _playerRecenter.Recenter();
            _currentChangeTimer = _changeTime;
            _currentTable = table;
        }

        public void ResetTable()
        {
            if (_currentChangeTimer > 0)
                return;

            if (_currentTable != null)
            {
                _currentTable.ResetObjects();
                _currentChangeTimer = _changeTime;
            }
        }

        public void RecenterPlayer()
        {
            if (_currentChangeTimer > 0)
                return;

            if (_currentTable != null)
            {
                _playerRecenter.Recenter();
                _currentChangeTimer = _changeTime;
            }
        }

        private void OnValidate()
        {
            _tableButtons = GetComponentsInChildren<TableButton>().ToList();
            _playerRecenter = FindObjectOfType<PlayerRecenter>();
            SetHandToIgnore();
        }

        private void SetHandToIgnore()
        {
            AttachmentHand attachmentHand = GetComponentInParent<AttachmentHand>(true);
            if (attachmentHand != null)
            {
                if (attachmentHand.chirality == Chirality.Left)
                {
                    _handToIgnore = Chirality.Left;
                }
                else
                {
                    _handToIgnore = Chirality.Right;
                }
            }
        }
    }
}
