
using UnityEngine;


[RequireComponent(typeof(RectTransform))]
public class ViewController : MonoBehaviour {


    public RectTransform cachedRectTransform;
    public RectTransform CachedRectTransform
    {
        get
        {
            if (cachedRectTransform == null)
            {
                cachedRectTransform = GetComponent<RectTransform>();
            }

            return cachedRectTransform;
        }
    }

    public virtual string Title
    {
        get { return ""; }
        set { }
    }
    
}
