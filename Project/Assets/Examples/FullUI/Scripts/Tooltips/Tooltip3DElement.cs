using UnityEngine;
using SocialPoint.GUIControl;
using System;
using SocialPoint.Base;

public class Tooltip3DElement : SPTooltipView 
{
    [SerializeField]
    RectTransform _placeholder;

    [SerializeField]
    GameObject _prefab;

    GameObject _go;

    public override void SetTooltipInfo()
    {
        _go = UnityEngine.Object.Instantiate(_prefab);

        if(_go == null)
        {
            return;
        }

        _go.transform.SetParent(_placeholder, false);
        _go.transform.localScale *= Math.Min(_placeholder.rect.width, _placeholder.rect.height) * 0.5f;

        _go.transform.rotation = Quaternion.Euler(30f, 30f, 0f);

        var anim = new RotateForEverAnimation();
        if(anim != null)
        {
            anim.Load(_go);
            StartCoroutine(anim.Animate());
        }

        Add3DContainer(_go);
    }

    protected override void OnDisappeared()
    {
        if(_go != null)
        {
            _go.DestroyAnyway();
        }

        base.OnDisappeared();
    }
}
