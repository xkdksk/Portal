using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ZenMonoContext<T> : ZenMonoBase<ZenMonoContext<T>> 
{
    public List<ZenBinder> binders = new List<ZenBinder>();


    
    public void UpdateVals()
    {
        foreach (var b in binders)
        {
            b.UpdateVal();
        }
    }

    public void AddBinder(ZenBinder binder)
    {
        if (binder.zenContext != this)
            return;

        if (binders.Contains(binder)==false)
        {
            binders.Add(binder);
        }

        ValidateBinders();

    }

    public void ValidateBinders()
    {
        int i;
        for (i = binders.Count - 1; i > 0; i--)
        {
            if (binders[i] == null)
            {
                binders.RemoveAt(i);
            }
            var b = binders[i];
            if (b.zenContext != this)
            {
                binders.RemoveAt(i);

            }
        }

    }
}