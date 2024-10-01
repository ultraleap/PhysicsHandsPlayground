using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.PhysicalHands.Playground
{
    public class StartAtTable : MonoBehaviour
    {
        [SerializeField]
        private TableManager _startingTable;

        private TableHandMenu _menu;

        private void Awake()
        {
            if(_startingTable != null)
            {
                _menu = FindAnyObjectByType<TableHandMenu>(FindObjectsInactive.Include);
                if(_menu != null)
                {
                    StartCoroutine(WaitToMove());
                }
            }
        }

        private IEnumerator WaitToMove()
        {
            yield return new WaitForSeconds(0.5f);
            _menu.SetCurrentTable(_startingTable, true);
        }
    }
}