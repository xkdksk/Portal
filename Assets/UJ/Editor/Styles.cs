using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UJ.Graph.Editor
{
    public class Styles :ScriptableObject
    {

        [MenuItem("Assets/NodeGrpah/CreateStyles")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<Styles>("Editor/Resources/Styles");
        }

        static Styles _styles;
        public static Styles I
        {
            get
            {
                if (_styles == null)
                {
                    _styles = Resources.Load<Styles>("Styles/Default");
                }

                return _styles;
            }
        }


        public GUIStyle Box;


    }
}
