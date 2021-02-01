using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UJ.Graph;

namespace UJ.Graph.Editor
{

    public class BaseNodeGraphEditor< TGraph,TNode,TLink,TNodeData,TLinkData>  : EditorWindow
        where TGraph : Graph<TNode,TLink>
        where TNode : BaseNode<TNodeData>
        where TLink :BaseLink<TNode,TLinkData>


       
    {
        static BaseNodeGraphEditor<TGraph, TNode, TLink, TNodeData, TLinkData> mainWindow;

        private static Rect canvasRect; //rect within which the graph is drawn (the window)
        private static Rect viewRect; //the panning rect that is drawn within canvasRect


        bool _nowAddLink;
        public bool NowAddLink
        {
            get
            {
                return _nowAddLink;
            }
            set
            {

                _nowAddLink = value;

            }
        }

        const int TAB_HEIGHT = 21;
        const int LINK_CIRCLE_RADIUS= 20;

        const int TOP_MARGIN = TAB_HEIGHT + 0;
        const int BOTTOM_MARGIN = 5;
        const int SIDE_MARGIN = 5;
        public float zoomFactor=1;
        const float ZOOM_MAX = 1f;
        const float ZOOM_MIN = 0.25f;
        const int GRID_SIZE = 15;

        private static Vector2 viewCanvasCenter
        {
            get { return viewRect.size / 2; }
        }


        private Vector2 pan
        {
            get { return CurrentGraph != null ? CurrentGraph.translation : viewCanvasCenter; }
            set
            {
                if (CurrentGraph != null)
                {
                    var t = value;
                    t.x = Mathf.Round(t.x); //pixel perfect correction
                    t.y = Mathf.Round(t.y); //pixel perfect correction

                    CurrentGraph.translation = t;
                }
            }
        }


        TGraph _currentGraph;
        public TGraph CurrentGraph
        {
            get
            {
                return _currentGraph;
            }
            set
            {
                _currentGraph = value;
                CurrnetGraphChanged();
            }
        }

  

        public TNode currentNode,mouseOverNode;
        public TLink currentLink, mouseOverLink;

    //    [MenuItem("UJ/CreateGraph")]
        public static void CreateGraph_(string basePath)
        {

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/" + basePath + "/NewGraph.asset");

            var graph= CreateInstance<TGraph>();
       //     graph.nodes.Add(CreateInstance<TestNode>());


            graph.SaveToAsset(assetPathAndName);
            AssetDatabase.ImportAsset(assetPathAndName);



        }

        //        [MenuItem("UJ/ShowGraphEditor")]



        public virtual void CurrnetGraphChanged()
        {

        }
        protected virtual void OnCheckSelectionWhenCurrentGraphNull()
        {

        }


        protected  void OnSelectionChange()
        {

            if (CurrentGraph == null)
            {

                OnCheckSelectionWhenCurrentGraphNull();
                if (CurrentGraph == null)
                {
                    CurrentGraph = Selection.activeObject as TGraph;

                    Repaint();
                }
            }
            else         
            {

                var graph = Selection.activeObject as TGraph;

                if(graph!=null&& graph != CurrentGraph)
                {
                    CurrentGraph = graph;
                    Repaint();
                    return;
                }


                var link=Selection.activeObject as TLink;
                if (link != null) {
                   if(CurrentGraph.links.Contains(link))
                    {
                        return;
                    }
                }
                var node = Selection.activeObject as TNode;
                if (node != null)
                {
                    if (CurrentGraph.nodes.Contains(node))
                    {
                        return;
                    }
                }
                if (CurrentGraph != null)
                {

                    CurrentGraph = null;
                    Repaint();
                }
            }

        }

        static void DrawGrid(Rect container, Vector2 offset, float zoomFactor)
        {
            if (Event.current.type != EventType.Repaint) { return; }

            var color = Color.black;

            color.a = 0.3f;

            Handles.DrawSolidRectangleWithOutline(container, color, color);
            color.a = 0.15f;
            Handles.color = color;

            var drawGridSize = zoomFactor > 0.5f ? GRID_SIZE : GRID_SIZE * 5;
            var step = drawGridSize * zoomFactor;

            var xDiff = offset.x % step;
            var xStart = container.xMin + xDiff;
            var xEnd = container.xMax;
            for (var i = xStart; i < xEnd; i += step)
            {
                if (i > container.xMin)
                { //this avoids one step being drawn before x min on negative mod
                    Handles.DrawLine(new Vector3(i, container.yMin, 0), new Vector3(i, container.yMax, 0));
                }
            }

            var yDiff = offset.y % step;
            var yStart = container.yMin + yDiff;
            var yEnd = container.yMax;
            for (var i = yStart; i < yEnd; i += step)
            {
                if (i > container.yMin)
                { //this avoids one step being drawn before y min on negative mod
                    Handles.DrawLine(new Vector3(container.xMin, i, 0), new Vector3(container.xMax, i, 0));
                }
            }

            Handles.color = Color.white;
        }


        void OnGUI()
        {
            wantsMouseMove = true;
            canvasRect = Rect.MinMaxRect(SIDE_MARGIN, TOP_MARGIN, position.width - SIDE_MARGIN, position.height - BOTTOM_MARGIN);

            if (CurrentGraph == null)
            {
                return;
            }

            GUI.Label(new Rect(0, 0, 300, 30), new GUIContent("zoomFactor " + zoomFactor));





            if (zoomFactor == 0)
            {
                zoomFactor = 1;
            }

            DrawGrid(canvasRect, pan, zoomFactor);
            viewRect = EditorZoomArea.Begin(zoomFactor, canvasRect);
            {

                DrawLinks();

                DrawNodes();
            }

            EditorZoomArea.End();


            //            GUI.Box(rt, "<b>START</b>", Styles.I.Box);

            CheckEvents();


            CheckPopup();
        }


        void DrawPropWindow()
        {
         
        }



    public  Vector2 RotateVector2( Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }


        Vector2 lastMousePos;

        TLink[] tempArr = new TLink[100];

        private void DrawLinks()
        {
            Event e = Event.current;

            Handles.color = Color.black;
            if (NowAddLink && currentNode != null)
            {
                var nodePos = currentNode.pos + currentNode.GetSize() * 0.5f + CurrentGraph.translation;
                Handles.DrawAAPolyLine(3f, nodePos, lastMousePos);
            }

            foreach (var link in CurrentGraph.links)
            {
                link.e_maxCnt = 0;
            }

             
            foreach (var link in CurrentGraph.links)
            {

                int maxCnt = 0;
                if (link.e_maxCnt > 0)
                {
                    continue;
                }
                  
                foreach (var l2 in CurrentGraph.links)
                {
                    if ((link.from == l2.from && link.to == l2.to) || (link.to == l2.from && link.from == l2.to))
                    {
                        l2.e_idx = maxCnt;
                        tempArr[maxCnt++] = l2;  
                    }
                }

                int i = 0;
                for (i = 0; i < maxCnt; i++)
                {
                    tempArr[i].e_maxCnt = maxCnt;
                }
            }

                
            foreach (var link in CurrentGraph.links)
            {
                var startPos = link.from.pos + link.from.GetSize() * 0.5f + CurrentGraph.translation;
                var endPos = link.to.pos + link.to.GetSize() * 0.5f + CurrentGraph.translation;

               

                Handles.color = Color.black;
                Handles.DrawAAPolyLine(3f, startPos, endPos);

                DrawLinkCircle(link,startPos, endPos, link.e_maxCnt,link.e_idx);

            }


        }

        private void DrawLinkCircle(TLink link,Vector2 startPos, Vector2 endPos,int maxCnt,int idx)
        {
            Handles.color = Color.white;
            var t = (float)(idx + 1) / (maxCnt + 1);
            Vector2 centerPos;
            if(startPos.x + startPos.y< endPos.x + endPos.y) {

                centerPos = Vector2.Lerp(startPos, endPos, t);
            }
            else
            {
                centerPos = Vector2.Lerp(endPos, startPos, t);

            }

            link.e_pos = centerPos;


            if(currentLink== link)
            {
                Handles.color = Color.red;
                Handles.DrawSolidDisc(centerPos, Vector3.forward, LINK_CIRCLE_RADIUS+5);

            }

            if(link== mouseOverLink)
            {

                Handles.color = Color.yellow;
            }
            else
            {
                Handles.color = Color.white;

            }

            Handles.DrawSolidDisc(centerPos, Vector3.forward, LINK_CIRCLE_RADIUS);


            var v_n = (endPos - startPos).normalized;
            var forwardEndPos = centerPos + v_n * 10;
            Handles.color = Color.black;
            var backEndPos1 = centerPos - RotateVector2(v_n, 90) * 10;
            var backEndPos2 = centerPos - RotateVector2(v_n, -90) * 10;
            Handles.DrawAAPolyLine(3f, backEndPos1, forwardEndPos, backEndPos2);
        }

      


        bool dragStart = false;
        public void CheckEvents()
        {
            Event e = Event.current;



            if (e.type== EventType.ScrollWheel)
            {
                //     zoomFactor += e.delta.y;
                var delta = -e.delta.y;
                

                zoomFactor += delta >0 ? 0.01f : -0.01f;
                zoomFactor = Mathf.Clamp(zoomFactor, ZOOM_MIN, ZOOM_MAX);
                if (Mathf.Abs(1 - zoomFactor) < 0.00001f) { zoomFactor = 1; }
                Repaint();
                e.Use();
            }

            if(e.type== EventType.MouseMove)
            {
                var mp = e.mousePosition - canvasRect.TopLeft();             
                var mousePt = (mp) * 1 / zoomFactor;
                lastMousePos = mousePt;



                TNode overCandidate = null;
                foreach (var n in CurrentGraph.nodes)
                {
                    Vector2 pos = n.pos + CurrentGraph.translation;
                    Vector2 size = n.GetSize();
                    Rect rt = new Rect(pos, size);

                    if (rt.Contains(mousePt))
                    {
                        overCandidate = n;
                        Repaint();
                        break;
                    }

                }



                TLink cur_mouseOverLink = null;
                foreach (var l in CurrentGraph.links)
                {
                    if ((l.e_pos - mousePt).magnitude < LINK_CIRCLE_RADIUS)
                    {
                        cur_mouseOverLink = l;
                        mouseOverNode = null;
                        e.Use();
                        break;
                    }
                }
                if(cur_mouseOverLink!= mouseOverLink)
                {
                    mouseOverLink= cur_mouseOverLink;
                    Repaint();
                }


                if (overCandidate != mouseOverNode)
                {
                    mouseOverNode = overCandidate;
                    if (mouseOverNode != null)
                    {
                        mouseOverLink = null;
                    }

                    Repaint();
                }

                if (NowAddLink)
                {
                    Repaint();
                }
            }


            if (e.type== EventType.MouseDown && e.button == 0)
            {
                dragStart = true;
                var mp = e.mousePosition - canvasRect.TopLeft();
                var mousePt = (mp) * 1 / zoomFactor;
           
                bool nowOnNode=false;
                foreach (var n in CurrentGraph.nodes)
                {
                    Vector2 pos = n.pos + CurrentGraph.translation;
                    Vector2 size = n.GetSize();

                    Rect rt = new Rect(pos, size);

                    if (rt.Contains(mousePt))
                    {
                        nowOnNode = true;
                        if (NowAddLink)
                        {
                            if (currentNode != n && currentNode!=null)
                            {

                                var newLink = CreateInstance<TLink>();

                                newLink.from = currentNode;
                                newLink.to = n;
                                CurrentGraph.links.Add(newLink);

                                var path = AssetDatabase.GetAssetPath(CurrentGraph);
                                CurrentGraph.SaveToAsset(path);
                                
                            }

                        }

                        currentLink = null;
                        currentNode = n;
                        Selection.activeObject = n;
                        e.Use();
                        break;
                    }
                }

                foreach (var l in CurrentGraph.links)
                {


                    if((l.e_pos-mousePt).magnitude < LINK_CIRCLE_RADIUS)
                    {
                        currentLink = l;

                        Selection.activeObject = currentLink;
                        currentNode = null;
                        e.Use();
                        break;
                    }
                }
                   
                if (nowOnNode==false)
                {
                    currentNode = null;
                }
                if (NowAddLink)
                {
                    NowAddLink = false;
                    
                }



            }



            if (e.type== EventType.MouseUp)
            {
                dragStart = false;
                e.Use();

            }

            if (e.type == EventType.MouseDrag && e.button == 0  && dragStart)
            {

                var mp = e.mousePosition - canvasRect.TopLeft();
                var mousePt = (mp) * 1 / zoomFactor;
                if (currentNode != null)
                {
                    currentNode.pos += e.delta * 1/zoomFactor;
                    EditorUtility.SetDirty(currentNode);

                    e.Use();
                } else if( CurrentGraph != null )
                {
                    CurrentGraph.translation += e.delta * 1 / zoomFactor;
                    EditorUtility.SetDirty(CurrentGraph);

                    e.Use();

                }

                
            }


       
        }


        protected virtual void DrawNodes()
        {
            foreach (var n in CurrentGraph.nodes)
            {
                Vector2 pos = n.pos+ CurrentGraph.translation;
                Vector2 size = n.GetSize();

                Rect rt = new Rect(pos, size);
                var style = Styles.I.Box;
           

                if(n== currentNode)
                {
                    GUI.backgroundColor = Color.red;
                    Rect outRt = new Rect(pos, size+ new Vector2(10,10));
                    outRt.center = rt.center;
                    GUI.Box(outRt, string.Format("", n.data));
                }

                if (n == mouseOverNode)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }
                DrawNode(rt, n);


            }
        }

        protected virtual void DrawNode(Rect rt, TNode n)
        {

            GUI.Box(rt, n.ToString());
        }

        protected  virtual void AppendMenuWhenNodeSelected(TNode currentNode, GenericMenu menu)
        {

        }


        private void CheckPopup()
        {
            Event evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 1)
            {

                dragStart = false;
                // Now create the menu, add items and show it
                GenericMenu menu = new GenericMenu();

                if (mouseOverNode == null && mouseOverLink == null)
                {
                    var mp = evt.mousePosition - canvasRect.TopLeft();
                    menu.AddItem(new GUIContent("New Node"), false, (v) =>
                    {
                        var newNode = CreateInstance<TNode>();

                        newNode.pos = mp * 1 / zoomFactor - CurrentGraph.translation;
                        CurrentGraph.AppendNewNode(newNode);
                      

                        var path = AssetDatabase.GetAssetPath(CurrentGraph);
                        CurrentGraph.SaveToAsset(path);

                    }, "");

                    menu.ShowAsContext();
                    evt.Use();
                }
                else if(mouseOverNode!=null)
                {
                    currentNode = mouseOverNode;
                    currentLink = null;
                    Repaint();

                    menu.AddItem(new GUIContent("Create Link"), false, (v) =>
                    {


                        NowAddLink = true;                        
                    }, "");


                    menu.AddItem(new GUIContent("Delete Node"), false, (v) =>
                    {
                        BeforeDeleteNode(mouseOverNode);

                        CurrentGraph.nodes.Remove(mouseOverNode);
                        AssetDatabase.RemoveObjectFromAsset(mouseOverNode);

                        int victimCnt = 0;
                        foreach(var link in CurrentGraph.links)
                        {
                            if (link.from == mouseOverNode || link.to == mouseOverNode)
                            {
                                tempArr[victimCnt++] = link;
                            }
                            
                        }

                        int i = 0;
                        for (i = 0; i < victimCnt; i++)
                        {
                            var victimLink = tempArr[i];
                            CurrentGraph.links.Remove(victimLink);
                            AssetDatabase.RemoveObjectFromAsset(victimLink);
                        }


                        var path = AssetDatabase.GetAssetPath(CurrentGraph);
                        CurrentGraph.SaveToAsset(path);

                    }, "");

                    AppendMenuWhenNodeSelected(currentNode, menu);
                    menu.ShowAsContext();
                    evt.Use();

                }else if (mouseOverLink != null)
                {

                    menu.AddItem(new GUIContent("Delete Link"), false, (v) =>
                    {
                        CurrentGraph.links.Remove(mouseOverLink);
                        AssetDatabase.RemoveObjectFromAsset(mouseOverLink);
                     

                        var path = AssetDatabase.GetAssetPath(CurrentGraph);
                        CurrentGraph.SaveToAsset(path);

                    }, "");
                    menu.ShowAsContext();
                    evt.Use();
                }
            }
        }

        protected virtual void BeforeDeleteNode(TNode mouseOverNode)
        {
        }
    }

}