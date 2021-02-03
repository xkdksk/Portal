using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IZenMonoContext
{
    bool AddBinder(ZenBinder binder);
    bool ValidateBinders();

}

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

    public bool AddBinder(ZenBinder binder)
    {
        if (binder.zenContext != this)
            return false;

        bool changed = false;
        if (binders.Contains(binder)==false)
        {
            binders.Add(binder);
            changed = true;
        }

        changed = changed || ValidateBinders();

        return changed;
    }

    public bool ValidateBinders()
    {
        int i;
        bool changed = false;
        for (i = binders.Count - 1; i > 0; i--)
        {
            if (binders[i] == null)
            {
                binders.RemoveAt(i);
                changed = true;
            }
            var b = binders[i];
            if (b.zenContext != this)
            {
                binders.RemoveAt(i);
                changed = true;
            }
        }

        return changed;

    }
}