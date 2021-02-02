using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_IntList : TableViewController<int>
{

  

    public void SetData(IList<int> tableData)
    {

        this.tableData = tableData;
        UpdateContents();

    }

}
