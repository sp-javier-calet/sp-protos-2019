using UnityEngine;
using SocialPoint.GUIControl;
using System;
using SocialPoint.Base;

public class Tooltip3DElement : SPTooltipViewController 
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
        _go.transform.localPosition = Vector3.down * 40;
        _go.transform.localScale *= Math.Min(_placeholder.rect.width, _placeholder.rect.height) * 0.5f;

        _go.transform.rotation = Quaternion.Euler(10f, 0f, -30f);

        var anim = new RotateForEverAnimation(new Vector3(0f, 360f, 0f), true);
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
