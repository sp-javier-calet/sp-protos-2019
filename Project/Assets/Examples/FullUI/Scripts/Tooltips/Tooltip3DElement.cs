using UnityEngine;
using SocialPoint.GUIControl;
using System;
using SocialPoint.Base;

public class Tooltip3DElement : SPTooltipViewController 
{
    [SerializeField] RectTransform _placeholder;

    GameObject _go;

    public override void SetTooltipInfo(BaseTooltipData data)
    {
        var tooltipData = data as ComplexTooltipData;
        if(tooltipData == null || tooltipData.ModelPrefab == null)
        {
            return;
        }
        
        _go = UnityEngine.Object.Instantiate(tooltipData.ModelPrefab);

        if(_go == null)
        {
            return;
        }

        _go.transform.SetParent(_placeholder, false);
        _go.transform.localPosition = Vector3.down * tooltipData.LocalPositionOffset;
        _go.transform.localScale *= Math.Min(_placeholder.rect.width, _placeholder.rect.height) * tooltipData.LocalScaleMultiplier;
        _go.transform.rotation = tooltipData.Rotation;

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
        // As we don't have the ability to fade the 3D model we simply destroy it
        if(_go != null)
        {
            _go.DestroyAnyway();
        }

        base.OnDisappeared();
    }
}
