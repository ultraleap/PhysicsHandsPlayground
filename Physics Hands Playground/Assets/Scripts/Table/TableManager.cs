using Leap.Unity.Interaction.PhysicsHands;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
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
        private List<Pose> _poses = new List<Pose>();

        [SerializeField]
        private Transform _teleportParent = null;
        [SerializeField, HideInInspector]
        private List<Transform> _teleportPositions = new List<Transform>();

        private int _tableUserIndex = -1;

        private void Start()
        {
            Rigidbody[] rigids = _objectArea.GetComponentsInChildren<Rigidbody>();
            foreach (var rigid in rigids)
            {
                _rigids.Add(rigid);
                _poses.Add(new Pose(rigid.transform.position, rigid.transform.rotation));
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
            }
        }

        public Transform TeleportToTable()
        {
            _tableUserIndex = (_tableUserIndex + 1) % _teleportPositions.Count;
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