﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[AddComponentMenu("UI/Effects/Gradient")]
public class GradientFx : BaseMeshEffect
{
    public enum Type
    {
        Vertical,
        Horizontal
    }
    [SerializeField]
    public Type GradientType = Type.Vertical;

    [SerializeField]
    [Range(-1.5f, 1.5f)]
    public float Offset = 0f;

    [SerializeField]
    private Color32 StartColor = Color.white;
    [SerializeField]
    private Color32 EndColor = Color.black;

    public override void ModifyMesh(Mesh mesh)
    {
        if (!this.IsActive())
            return;

        List<UIVertex> list = new List<UIVertex>();
        using (VertexHelper vertexHelper = new VertexHelper(mesh))
        {
            vertexHelper.GetUIVertexStream(list);
        }

        ModifyVertices(list);

        using (VertexHelper vertexHelper2 = new VertexHelper())
        {
            vertexHelper2.AddUIVertexTriangleStream(list);
            vertexHelper2.FillMesh(mesh);
        }
    }

    public void ModifyVertices(List<UIVertex> _vertexList)
    {
        if (!IsActive())
            return;

        int nCount = _vertexList.Count;
        if (nCount == 0)
        {
            return;
        }

        switch (GradientType)
        {
            case Type.Vertical:
                {
                    float fBottomY = _vertexList[0].position.y;
                    float fTopY = _vertexList[0].position.y;
                    float fYPos = 0f;

                    for (int i = nCount - 1; i >= 1; --i)
                    {
                        fYPos = _vertexList[i].position.y;
                        if (fYPos > fTopY)
                            fTopY = fYPos;
                        else if (fYPos < fBottomY)
                            fBottomY = fYPos;
                    }

                    float fUIElementHeight = 1f / (fTopY - fBottomY);
                    for (int i = nCount - 1; i >= 0; --i)
                    {
                        UIVertex uiVertex = _vertexList[i];
                        uiVertex.color = Color32.Lerp(EndColor, StartColor, (uiVertex.position.y - fBottomY) * fUIElementHeight - Offset);
                        _vertexList[i] = uiVertex;
                    }
                }
                break;
            case Type.Horizontal:
                {
                    float fLeftX = _vertexList[0].position.x;
                    float fRightX = _vertexList[0].position.x;
                    float fXPos = 0f;

                    for (int i = nCount - 1; i >= 1; --i)
                    {
                        fXPos = _vertexList[i].position.x;
                        if (fXPos > fRightX)
                            fRightX = fXPos;
                        else if (fXPos < fLeftX)
                            fLeftX = fXPos;
                    }

                    float fUIElementWidth = 1f / (fRightX - fLeftX);
                    for (int i = nCount - 1; i >= 0; --i)
                    {
                        UIVertex uiVertex = _vertexList[i];
                        uiVertex.color = Color32.Lerp(StartColor, EndColor, (uiVertex.position.x - fLeftX) * fUIElementWidth - Offset);
                        _vertexList[i] = uiVertex;
                    }
                }
                break;
            default: break;
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        List<UIVertex> list = new List<UIVertex>();
        vh.GetUIVertexStream(list);

        ModifyVertices(list);

        vh.Clear();
        vh.AddUIVertexTriangleStream(list);
    }
}