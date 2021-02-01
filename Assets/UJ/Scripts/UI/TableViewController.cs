using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;


public class TableViewController<T> : ViewController {



    protected IList<T> tableData;

    protected Rect[] itemRect;
    [SerializeField]
    public RectOffset padding;
    [SerializeField]
    public Vector2 spacing;


    [SerializeField]
    public int column=1;


   

    [SerializeField]
    public int row = 1;


    private Rect visibleRect;
    [SerializeField]
    public RectOffset visibleRectPadding;


    GameObject _cellBase;

    [SerializeField]
    public GameObject CellBase
    {
        get
        {
            if (_cellBase == null)
            {
                _cellBase=cellParent.GetChild(0).gameObject;
            }
            return _cellBase;
        }

    }

    [SerializeField]
    public RectTransform cellParent;



    public IList<T> GetTable()
    {
        return tableData;
    }

    protected List<TableViewCell<T> > cells = new List<TableViewCell<T> >();

    public List<TableViewCell<T>> Cells
    {
        get { return cells; }
    }

    Vector2 yInverse(Vector2 v)
    {
        return new Vector2(v.x, -v.y);

    }


    private void OnDrawGizmos()
    {


        Gizmos.color = new Color(0, 1, 0, 0.5F);


      
        Gizmos.DrawCube(yInverse(visibleRect.center) , visibleRect.size);

     


        var rr=new Rect();
        rr.x = visibleRect.x;
        rr.y = visibleRect.y;
        rr.width = 100;
        rr.height = 100;
        Gizmos.color = new Color(1, 0, 0, 0.5F);
        Gizmos.DrawCube(yInverse(rr.center), rr.size);
        

        if (itemRect != null)
        {

            int i;
            for (i = 0; i < itemRect.Length; i++)
            {

                var alpha = 0.1f + ((float)i / itemRect.Length)*0.9f;
                if (i % 2 == 0)
                {
                    Gizmos.color = new Color(0, 0, 0.5f, alpha);
                }
                else
                {
                    Gizmos.color = new Color(0, 0, 1, alpha);
                }

                var rect = itemRect[i]; ;


           


                Gizmos.DrawCube(yInverse(rect.center), rect.size);

            }
        }


    }


    private void UpdateVisibleRect()
    {

        visibleRect = cachedScrollRect.viewport.rect; //CachedRectTransform.rect;

        var contentRT = cachedScrollRect.content;
        var pos = contentRT.anchoredPosition;


        visibleRect.x = -pos.x;
        visibleRect.y = pos.y + contentRT.sizeDelta.y;
    }


    public void UpdateContents()
    {
        UpdateContentSize();
        UpdateVisibleRect();
        FillVisibleRectWithCells();


    }

    public TableViewCell<T> getCell(int dataIndex) {

        var cell=cells.FirstOrDefault(l => l.nowUsing && l.DataIndex == dataIndex);

        if (cell != null)
        {
            return cell;
        }

        cell=cells.FirstOrDefault(l => l.nowUsing == false);

        if (cell == null)
        {
            GameObject obj = GameObject.Instantiate<GameObject>(CellBase, cellParent);
            obj.SetActive(true);
            

            cell = obj.GetComponent<TableViewCell<T>>();
            cell.CachedRectTransform.sizeDelta = new Vector2(CellWidthAtIndex(dataIndex), CellHeightAtIndex(dataIndex));
            cell.nowUsing = false;
            cells.Add(cell);
        }

        return cell;
    }



    private void FillVisibleRectWithCells()
    {
        if (itemRect == null)
        {
            return;
        }

        int i;

        i = 0;

        foreach (var cell in cells)
        {

            if(i>= itemRect.Length)
            {
                cell.nowUsing = false;
                cell.gameObject.SetActive(false);
                continue; 
            }
            i++;
            

            if (cell.nowUsing)
            {

                cell.nowUsing = false;
                try
                {
                    var rect = itemRect[cell.DataIndex];


                    if (visibleRect.Overlaps(rect) == false)
                    {
                        cell.gameObject.SetActive(false);
                    }
                }
                catch (Exception)
                {
                    cell.gameObject.SetActive(false);
                    continue;
                }
            }
        }

        var cellRT = (CellBase.transform as RectTransform);


        var contentSize = CachedScrollRect.content.sizeDelta;


        for (i=0;i< itemRect.Length; i++)
        {
            var rect = itemRect[i];

            if (visibleRect.Overlaps(rect))
            {
                TableViewCell<T> cell = getCell(i);
                if (cell.nowUsing == false)
                {
                    UpdateCellForIndex(cell, i);
                    cell.nowUsing = true;
                }

                cell.LeftTop = new Vector2(rect.x, -rect.y );
         


            }
        }
        foreach (var cell in cells)
        {
            cell.gameObject.SetActive(cell.nowUsing);
        }

    }


    protected virtual void Start()
    {
        CellBase.SetActive(false);

        CachedScrollRect.onValueChanged.AddListener(OnScrollPosChanged);
        CachedScrollRect.content.anchorMin = new Vector2(0, 1);
        CachedScrollRect.content.anchorMax = new Vector2(0, 1);
        CachedScrollRect.content.pivot = new Vector2(0, 0);
    }

    public void OnScrollPosChanged(Vector2 scrollPos)
    {
        UpdateVisibleRect();
        FillVisibleRectWithCells();
    }



    private void UpdateCellForIndex(TableViewCell<T> cell, int index)
    {
        cell.DataIndex = index;

        if(cell.DataIndex >= 0 && cell.DataIndex <= tableData.Count - 1)
        {
         
            cell.UpdateContent(tableData[cell.DataIndex]);
            cell.gameObject.SetActive(true);
            cell.Height = CellHeightAtIndex(cell.DataIndex);

            if (cell.CachedRectTransform.anchorMax.x != 1)
            {
                cell.Width = CellWidthAtIndex(cell.DataIndex);
            }
        }else
        {
            cell.gameObject.SetActive(false);
        }

    }

    public ScrollRect cachedScrollRect;
    public ScrollRect CachedScrollRect
    {
        get
        {
            if (cachedScrollRect == null)
            {
                cachedScrollRect = GetComponent<ScrollRect>();

                if (cachedScrollRect == null)
                {
                    cachedScrollRect = GetComponentInChildren<ScrollRect>();

                }
            }
            return cachedScrollRect;
        }
    }
    
    protected virtual void Awake()
    {

    }    

    protected virtual float CellHeightAtIndex(int index)
    {
        var t = (CellBase.transform as RectTransform);
        if (Mathf.Approximately(0, t.sizeDelta.y)){

            return CachedScrollRect.viewport.rect.height;
        }

        return t.sizeDelta.y;
    }

    protected virtual float CellWidthAtIndex(int index)
    {
        var t = (CellBase.transform as RectTransform);
        if (Mathf.Approximately(0, t.sizeDelta.x))
        {


            return CachedScrollRect.viewport.rect.width;
        }

        return t.sizeDelta.x;
    }

    protected void UpdateContentSize()
    {
        Vector2 conetentSize = Vector2.zero;
        int columnIndex, rowIndex;
   
        int column = this.column;

       

        int itemIter = 0;

        if(itemRect == null || itemRect.Length != tableData.Count)
        {
            itemRect = new Rect[tableData.Count];
        }


        if (column != 0)
        {
            float maxWidth = 0f;
            int row;
            float yPos = padding.top;
            row = Mathf.CeilToInt((float)tableData.Count / column);


            for (rowIndex = 0; rowIndex < row; rowIndex++)
            {
                float xPos = padding.left;

                float maxItemHeight = 0;

                for (columnIndex = 0; columnIndex < column; columnIndex++)
                {
                    var itemWidth = CellWidthAtIndex(itemIter);
                    var itemHeight = CellHeightAtIndex(itemIter);

                    if (itemIter >= tableData.Count)
                    {
                        break;
                    }

                    var rect = itemRect[itemIter];
                    rect.x = xPos;
                    rect.y = yPos;
                    rect.width = itemWidth;
                    rect.height = itemHeight;

                    itemRect[itemIter] = rect;

                    itemIter++;

                    xPos += itemWidth;
                    xPos += spacing.x;
                    if (maxItemHeight < itemHeight)
                    {
                        maxItemHeight = itemHeight;
                    }
                }

                yPos += maxItemHeight;

                if (columnIndex > 0)
                {
                    yPos+= spacing.y;
                }

                if (maxWidth < xPos)
                {
                    maxWidth = xPos;
                }
            }

            conetentSize.y = yPos;
            conetentSize.x = maxWidth;
        }
        else if(row!=0)
        {


            column = Mathf.CeilToInt((float)tableData.Count / row);
         
            float xPos = padding.left;
            for (columnIndex = 0; columnIndex < column; columnIndex++)
            {
  
                float yPos = padding.top;
                float maxItemWidth = 0;
                var itemWidth = CellWidthAtIndex(itemIter);

                for (rowIndex = 0; rowIndex < row; rowIndex++)
                {
              
                    var itemHeight = CellHeightAtIndex(itemIter);

                    if (itemIter >= tableData.Count)
                    {
                        break;
                    }

                    var rect = itemRect[itemIter];
                    rect.x = xPos;                
                    rect.y = yPos;
                    yPos += itemHeight;
                    yPos += spacing.y;


                    rect.width = itemWidth;
                    rect.height = itemHeight;

                    itemRect[itemIter] = rect;
                    itemIter++;

                    
                    if (maxItemWidth < itemWidth)
                    {
                        maxItemWidth = itemWidth;
                    }

                    if (conetentSize.y < yPos)
                    {
                        conetentSize.y = yPos;
                    }


                }

                xPos += maxItemWidth;
                xPos += spacing.x;

            }

            conetentSize.x = xPos;
   

        }
        else
        {
            Debug.LogError("Column Row is zero");
        }


        Vector2 sizeDelta = CachedScrollRect.content.sizeDelta;
    
        
        sizeDelta.y = padding.top + conetentSize.y + padding.bottom;
        sizeDelta.x = padding.left + conetentSize.x + padding.right;


        var viewPortRect = cachedScrollRect.viewport.rect;

        if(sizeDelta.x< viewPortRect.width)
        {
            sizeDelta.x = viewPortRect.width;
        }

        if (sizeDelta.y < viewPortRect.height)
        {
            sizeDelta.y = viewPortRect.height;
        }

        CachedScrollRect.content.sizeDelta = sizeDelta;
        CachedScrollRect.content.anchoredPosition = new Vector2(0, -sizeDelta.y);

     //   CachedScrollRect.content.anchoredPosition= -=

    }
}
