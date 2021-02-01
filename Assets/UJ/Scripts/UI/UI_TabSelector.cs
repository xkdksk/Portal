using System.Collections.Generic;
using UJ.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UJ.UI
{
    public class UI_TabSelector : MonoBehaviour
    {

        int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
        }

        public string animatorBoolName;

        List<Animator> _Animators;
        List<Animator> Animators
        {
            get
            {
                if (_Animators == null)
                {
                    _Animators = new List<Animator>();
                    foreach (Transform t in this.transform)
                    {

                        _Animators.Add(t.GetComponent<Animator>());
                    }
                }

                return _Animators;

            }
        }

        public void TabClick(int idx)
        {

            int i;

            for (i = 0; i < Animators.Count; i++)
            {
                Animators[i].SetBool(animatorBoolName, i == idx);

            }
            _selectedIndex=idx;

            EventBus.Publish(this);

        }


        bool initialized = false;
        public void OnEnable()
        {

            if (initialized == false)
            {
                initialized = true;
                foreach (Transform t in this.transform)
                {
                    var btn = t.GetComponent<Button>();

                    btn.onClick.AddListener(MakeTabClickFunc(t));
                }
            }

        }

        private UnityAction MakeTabClickFunc(Transform t)
        {
            return () =>
            {
                var sidx = t.GetSiblingIndex();

                TabClick(sidx);
            };
        }
    }

}