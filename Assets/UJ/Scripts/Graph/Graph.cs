using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UJ.Graph
{

    public class BaseNode<T> : ScriptableObject 
       
    {
        [HideInInspector]
        public Vector2 pos;
        public T data;

#if UNITY_EDITOR
        public Vector2 GetSize()
        {
            return new Vector2(200, 50);
        }
#endif

    }


    public class BaseLink<TNode, TData> : ScriptableObject
    {
        [HideInInspector]
        public TNode from, to;

        public TData data;

#if UNITY_EDITOR
        [HideInInspector]
        public int e_maxCnt;

        [HideInInspector]
        public int e_idx;

        [HideInInspector]
        public Vector2 e_pos;
#endif

    }


    public class Graph<TNode,TLink> : ScriptableObject
         where TNode :ScriptableObject
         where TLink : ScriptableObject

    {

        [HideInInspector]
        public List<TNode> nodes = new List<TNode>();

        [HideInInspector]
        public List<TLink> links = new List<TLink>();

        [HideInInspector]
        public Vector2 translation;


#if UNITY_EDITOR

        protected virtual void BeforeSaveToAsset()
        {

        }

        public  void SaveToAsset(string path)
        {


            foreach (var n in nodes)
            {
                if (AssetDatabase.Contains(n) == false)
                {
                    AssetDatabase.AddObjectToAsset(n, path);
                }
            }

            foreach (var n in links)
            {
                if (AssetDatabase.Contains(n) == false)
                {
                    AssetDatabase.AddObjectToAsset(n, path);
                }
            }



            if (AssetDatabase.GetAssetPath(this) != path)
            {

                AssetDatabase.CreateAsset(this, path);
            }

            BeforeSaveToAsset();

            AssetDatabase.SaveAssets();
        }

        public virtual void AppendNewNode(TNode newNode) 
        {
            nodes.Add(newNode);
        }


#endif

    }


}
