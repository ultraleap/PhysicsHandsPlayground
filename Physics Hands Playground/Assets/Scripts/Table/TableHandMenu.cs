using Leap.Unity.Attachments;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Leap.Unity.PhysicalHands;
using Leap.Unity.Interaction;

namespace Leap.Unity.PhysicalHands.Playground
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
        private bool _newHands = false;

        private bool _facingCamera = false, _grasping = false;
        private float _graspTimeout = 0.5f, _graspCurrent = 0f;

        [SerializeField]
        private PlayerRecenter _playerRecenter = null;

        [SerializeField, Tooltip("Set as null to start in the middle.")]
        private TableManager _startAtTable = null;

        private ContactHand _ignoredHand { get { return _handToIgnore == Chirality.Left ? _physicalHandsManager.ContactParent.LeftHand : _physicalHandsManager.ContactParent.RightHand; } }

        public bool IsMenuVisible
        {
            get
            {
                return !_grasping
                    && _handToIgnore == Chirality.Left ? _physicalHandsManager.ContactParent.LeftHand.Tracked : _physicalHandsManager.ContactParent.RightHand.Tracked
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

            SetHandToIgnore();

            _cameraCallbacks = GetComponent<SimpleFacingCameraCallbacks>();
            _cameraCallbacks.OnBeginFacingCamera.AddListener(() => { _facingCamera = true; RecalcButtons(); });
            _cameraCallbacks.OnEndFacingCamera.AddListener(() => { _facingCamera = false; RecalcButtons(); });

            if (_startAtTable != null)
            {
                StartCoroutine(ZeroTable());
            }
        }

        private void Update()
        {
            if (_newHands)
            {
                OnBeginPhysics();
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
                    Physics.IgnoreCollision(button, _ignoredHand.GetPalmBone().Collider);
                }



                foreach (var bone in _ignoredHand.contactBones)
                {
                    foreach (var button in colliders)
                    {
                        Physics.IgnoreCollision(button, bone.Collider);
                    }
                }
            }
            RecalcButtons();
        }

        private void OnUpdatePhysics()
        {
            if (_grasping != _ignoredHand.IsGrabbing)
            {
                _grasping = _ignoredHand.IsGrabbing;
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
            AttachmentHand attachmentHand = GetComponentInParent<AttachmentHand>(true);
            if (attachmentHand != null)
            {
                if (attachmentHand.chirality == Leap.Unity.Chirality.Left)
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
