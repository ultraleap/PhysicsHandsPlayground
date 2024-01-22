using Leap.Unity.PhysicalHands;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Leap.Unity.PhysicalHands.Playground
{
    public class TableButton : MonoBehaviour
    {
        [SerializeField]
        private TableManager _tableManager = null;
        public TableManager Table => _tableManager;
        [SerializeField]
        private PhysicalHandsButton _physicsButton = null;
        public PhysicalHandsButton PhysicsButton => _physicsButton;

        [SerializeField]
        private TextMeshPro _text;

        [SerializeField]
        private Renderer _buttonInner, _buttonOuter;

        void Start()
        {
            ApplySetup();
        }

        private void OnValidate()
        {
            _physicsButton = GetComponent<PhysicalHandsButton>();
            _text = GetComponentInChildren<TextMeshPro>();
            ApplySetup();
        }

        private void ApplySetup()
        {
            if (_tableManager != null)
            {
                MaterialColorUtils.ApplyColorThemeToRenderers(_tableManager.TableColor, _buttonInner, _buttonOuter, null, null);
                _text.text = _tableManager.name;
            }
        }
    }
}