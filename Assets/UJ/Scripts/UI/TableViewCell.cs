using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableViewCell<T> : ViewController {

    public virtual void UpdateContent(T itemData)
    {
     

    }


    public bool nowUsing;

  
    public int DataIndex { get; set; }

    public float Height
    {
        get { return CachedRectTransform.sizeDelta.y; }
        set
        {
            Vector2 sizeDelta = CachedRectTransform.sizeDelta;
            sizeDelta.y = value;
            CachedRectTransform.sizeDelta = sizeDelta;

        }
    }

    public float Width
    {
        get { return CachedRectTransform.sizeDelta.x; }
        set
        {
            Vector2 sizeDelta = CachedRectTransform.sizeDelta;
            sizeDelta.x = value;
            CachedRectTransform.sizeDelta = sizeDelta;
        }
    }
    

    public float Top
    {
        get
        {
            Vector3[] corners = new Vector3[4];
            CachedRectTransform.GetLocalCorners(corners);
            return CachedRectTransform.anchoredPosition.y + corners[1].y;
        }

        set
        {
            Vector3[] corners = new Vector3[4];
            CachedRectTransform.GetLocalCorners(corners);
            var v = CachedRectTransform.anchoredPosition;
            v.y = value - corners[1].y;
        }
    }



    public float Bottom
    {
        get
        {
            Vector3[] corners = new Vector3[4];
            CachedRectTransform.GetLocalCorners(corners);
            return CachedRectTransform.anchoredPosition.y + corners[3].y;
        }

        set
        {
            Vector3[] corners = new Vector3[4];
            CachedRectTransform.GetLocalCorners(corners);
            var v = CachedRectTransform.anchoredPosition;
            v.y = value - corners[3].y;
        }
    }

    public Vector2 LeftTop
    {
        get
        {
            Vector3[] corners = new Vector3[4];
            CachedRectTransform.GetLocalCorners(corners);
            return CachedRectTransform.anchoredPosition + new Vector2(corners[0].x, -corners[0].y);
        }

        set
        {
            Vector3[] corners = new Vector3[4];
            CachedRectTransform.GetLocalCorners(corners);
            CachedRectTransform.anchoredPosition = value- new Vector2(corners[0].x, -corners[0].y);
        }
    }


    public float Left
    {
        get
        {
            Vector3[] corners = new Vector3[4];
            CachedRectTransform.GetLocalCorners(corners);
            return CachedRectTransform.anchoredPosition.x + corners[1].x;
        }

        set
        {
            Vector3[] corners = new Vector3[4];
            CachedRectTransform.GetLocalCorners(corners);
            var v = CachedRectTransform.anchoredPosition;
            v.x = value - corners[1].x;
        }
    
    }

    public float Right
    {
        get
        {
            Vector3[] corners = new Vector3[4];
            CachedRectTransform.GetLocalCorners(corners);
            return CachedRectTransform.anchoredPosition.x + corners[2].x;
    }

        set
        {
            Vector3[] corners = new Vector3[4];
            CachedRectTransform.GetLocalCorners(corners);
            var v = CachedRectTransform.anchoredPosition;
            v.x = value - corners[2].x;
        }
        
    }
    
}
