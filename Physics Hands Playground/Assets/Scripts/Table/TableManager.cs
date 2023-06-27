using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Interaction.PhysicsHands.Playground
{
    public class TableManager : MonoBehaviour
    {
        [SerializeField]
        private Transform _objectArea = null;

        [SerializeField]
        private Color _tableColor = Color.black;
        public Color TableColor => _tableColor;

        [SerializeField]
        private Renderer _tableTop = null, _tableBottom = null, _buttonOuter = null, _buttonInner = null;

        [SerializeField]
        private Transform _buttonRotation = null;
        private PhysicsButton _physicsButton;

        private List<Rigidbody> _rigids = new List<Rigidbody>();
        private List<bool> _states = new List<bool>();
        private List<Pose> _poses = new List<Pose>();

        [SerializeField]
        private Transform _teleportParent = null;
        [SerializeField, HideInInspector]
        private List<Transform> _teleportPositions = new List<Transform>();

        private int _tableUserIndex = -1;
        [SerializeField, Tooltip("Increases the index at which the player will rotate around the table."), Range(1,8)]
        private int _teleportIndexIncreaseAmount = 2;

        public Action OnResetTable;

        private void Start()
        {
            Rigidbody[] rigids = _objectArea.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rigid in rigids)
            {
                _rigids.Add(rigid);
                _poses.Add(new Pose(rigid.transform.position, rigid.transform.rotation));
                _states.Add(rigid.gameObject.activeInHierarchy);
            }

            _physicsButton = GetComponentInChildren<PhysicsButton>(true);
            _physicsButton.OnPress += OnButtonPress;

            _teleportPositions.Clear();
            foreach (Transform transform in _teleportParent)
            {
                _teleportPositions.Add(transform);
            }

            ApplyColours();
        }

        private void OnButtonPress()
        {
            ResetObjects();
        }

        private void ResetObjects()
        {
            for (int i = 0; i < _rigids.Count; i++)
            {
                _rigids[i].velocity = Vector3.zero;
                _rigids[i].angularVelocity = Vector3.zero;
                _rigids[i].MovePosition(_poses[i].position);
                _rigids[i].MoveRotation(_poses[i].rotation);
                _rigids[i].gameObject.SetActive(_states[i]);
            }
            OnResetTable?.Invoke();
        }

        public Transform TeleportToTable()
        {
            _tableUserIndex = (_tableUserIndex + _teleportIndexIncreaseAmount) % _teleportPositions.Count;
            Transform teleportPos = _teleportPositions[_tableUserIndex];
            _buttonRotation.rotation = Quaternion.Euler(0, Quaternion.LookRotation(transform.position - teleportPos.position, Vector3.up).eulerAngles.y - 28f, 0);
            return teleportPos;
        }

        private void OnValidate()
        {
            _teleportPositions.Clear();
            foreach (Transform transform in _teleportParent)
            {
                _teleportPositions.Add(transform);
            }
            ApplyColours();
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var item in _teleportPositions)
            {
                Gizmos.color = _tableColor;
                Gizmos.DrawSphere(item.position, 0.05f);
            }
        }

        private void ApplyColours()
        {
            MaterialColorUtils.ApplyColorThemeToRenderers(_tableColor, _buttonInner, _buttonOuter, _tableTop, _tableBottom);
        }
    }
}