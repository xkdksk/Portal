using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UI_Hearts : MonoBehaviour
{

    Image _heartImage;
    Image HeartImage
    {

        get
        {
            if (_heartImage == null)
            {
                _heartImage = this.GetComponent<Image>();
            }

            return _heartImage;
        }
    }

    float _heartCount;

    public float HeartCount
    {
        get
        {
            return _heartCount;

        }
        set
        {
            _heartCount = value;

            var rect = HeartImage.sprite.rect;
            var width=rect.width;

            HeartImage.rectTransform.sizeDelta = new Vector2(width * _heartCount, rect.height);
        }
    }



}
