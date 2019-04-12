//-----------------------------------------------------------------------
// ComplexExampleTooltipTrigger.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.GUIControl;
using UnityEngine;

public class ComplexTooltipData : BaseTooltipData
{
    public GameObject ModelPrefab;
    public float LocalPositionOffset;
    public float LocalScaleMultiplier;
    public Quaternion Rotation;
}

public class ComplexExampleTooltipTrigger : SPTooltipTrigger
{
    [SerializeField] GameObject _modelPrefab;
    
    protected override BaseTooltipData Data
    {
        get
        {
            var data = new ComplexTooltipData()
            {
                ModelPrefab = _modelPrefab,
                LocalPositionOffset = 40f,
                LocalScaleMultiplier = 0.5f,
                Rotation = Quaternion.Euler(10f, 0f, -30f)
            };

            return data;
        }
    }
}