using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


// For some reason, using namespace is not properly supported for custom shader editors.
// Therefore, the name of the class has been altered to enforce unique classname.
public class SPCustomMaterialEditor : MaterialEditor
{
    private const string advancedRenderStatePropName = "_AdvancedRenderState";
    private const string renderStateBasicTypesPropName = "_RenderStateBasicTypes";
    private const string blendScPopName = "_SrcBlend";
    private const string blendDstPropName = "_DstBlend";
    private const string renderQueueEnumPropName = "_RenderQueueEnum";
    private const string alphaMultipliesRGBPropName = "_AlphaMultipliesRGB";
    private const string cullModePropName = "_CullMode";
    private const string renderQueueOffsetPropName = "_RenderQueueOffset";
    private const string depthTestPropName = "_DepthTest";
    private const string depthWritePropName = "_DepthWrite";


    public struct RenderStateValues
    {
        public float _BlendSrc, _BlendDst, _RenderQueueEnum, _AlphaMultipliesRGB, _DepthWrite;
        public int _CullMode, _DepthTest;

        public RenderStateValues(float BlendSrc, float BlendDst, float RenderQueueEnum, float AlphaMultipliesRGB, float DepthWrite, int CullMode, int DepthTest)
        {
            _BlendSrc = BlendSrc;
            _BlendDst = BlendDst;
            _RenderQueueEnum = RenderQueueEnum;
            _AlphaMultipliesRGB = AlphaMultipliesRGB;
            _DepthWrite = DepthWrite;
            _CullMode = CullMode;
            _DepthTest = DepthTest;
        }
    }


    private List<RenderStateValues> _renderStateValuesStructs = new List<RenderStateValues>();


    public SPCustomMaterialEditor()
    {
        // SOLID
        _renderStateValuesStructs.Add(new RenderStateValues((float)UnityEngine.Rendering.BlendMode.One, (float)UnityEngine.Rendering.BlendMode.Zero, 2000f, 0f, 1f, (int)UnityEngine.Rendering.CullMode.Back, (int)UnityEngine.Rendering.CompareFunction.LessEqual));

        // TRANSPARENT
        _renderStateValuesStructs.Add(new RenderStateValues((float)UnityEngine.Rendering.BlendMode.SrcAlpha, (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha, 3000f, 0f, 0f, (int)UnityEngine.Rendering.CullMode.Back, (int)UnityEngine.Rendering.CompareFunction.LessEqual));

        // ADDITIVE
        _renderStateValuesStructs.Add(new RenderStateValues((float)UnityEngine.Rendering.BlendMode.One, (float)UnityEngine.Rendering.BlendMode.One, 3000f, 1f, 0f, (int)UnityEngine.Rendering.CullMode.Back, (int)UnityEngine.Rendering.CompareFunction.LessEqual));

        // SCREEN
        _renderStateValuesStructs.Add(new RenderStateValues((float)UnityEngine.Rendering.BlendMode.OneMinusDstColor, (float)UnityEngine.Rendering.BlendMode.One, 3000f, 1f, 0f, (int)UnityEngine.Rendering.CullMode.Back, (int)UnityEngine.Rendering.CompareFunction.LessEqual));

        // MULTIPLY
        _renderStateValuesStructs.Add(new RenderStateValues((float)UnityEngine.Rendering.BlendMode.DstColor, (float)UnityEngine.Rendering.BlendMode.Zero, 3000f, 0f, 0f, (int)UnityEngine.Rendering.CullMode.Back, (int)UnityEngine.Rendering.CompareFunction.LessEqual));

        // TRANSPARENT DOUBLE SIDED
        _renderStateValuesStructs.Add(new RenderStateValues((float)UnityEngine.Rendering.BlendMode.SrcAlpha, (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha, 3000f, 0f, 0f, (int)UnityEngine.Rendering.CullMode.Off, (int)UnityEngine.Rendering.CompareFunction.LessEqual));
    }


    public override void OnDisable ()
    {
        serializedObject.Update();

        MaterialProperty renderQueueEnumProp = GetMaterialProperty(targets, renderQueueEnumPropName);
        MaterialProperty renderQueueOffsetProp = GetMaterialProperty(targets, renderQueueOffsetPropName);
        if (renderQueueEnumProp == null || renderQueueOffsetProp == null)
        {
            _ResetMaterialRenderQueue();
        }


            base.OnDisable();
            
            // TODO: On shader change, check if this is not one of our own shaders, and if so, restore the queue to -1.
    }


    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        // if we are not visible... return
        if (!isVisible)
        {
            return;
        }
                
        var theShader = serializedObject.FindProperty("m_Shader");
        if (isVisible && !theShader.hasMultipleDifferentValues && theShader.objectReferenceValue != null)
        {
            MaterialProperty[] matProps = GetMaterialProperties(targets);

            // This line, among other things, updates the shader Toggles correctly upon material import.
            ApplyMaterialPropertyDrawers(targets);

            serializedObject.Update();

            EditorGUIUtility.fieldWidth = 64;

            EditorGUI.BeginChangeCheck();
            Shader shader = theShader.objectReferenceValue as Shader;

            MaterialProperty advancedRenderStateProp = GetMaterialProperty(targets, advancedRenderStatePropName);
            bool advancedRenderState = false;
            if (advancedRenderStateProp != null && advancedRenderStateProp.floatValue == 1)
            {
                advancedRenderState = true;
            }

            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                MaterialProperty matProp = matProps[i];
                bool drawProperty = true;

                if (matProp.name == advancedRenderStatePropName)
                {
                    drawProperty = false;
                    base.ShaderProperty(matProp, matProp.displayName);
                    advancedRenderState = advancedRenderStateProp.floatValue == 1f ? true : false;
                }
                else if (advancedRenderState)
                {
                    if (matProp.name == renderStateBasicTypesPropName)
                    {
                        drawProperty = false;
                    }                    
                } else
                {
                    if (matProp.name == blendScPopName || matProp.name == blendDstPropName ||
                        matProp.name == renderQueueEnumPropName || matProp.name == alphaMultipliesRGBPropName ||
                        matProp.name == cullModePropName || matProp.name == depthTestPropName ||
                        matProp.name == depthWritePropName)
                    {
                        drawProperty = false;
                    }
                }

                if (drawProperty)
                {
                   base.ShaderProperty(matProp, matProp.displayName);
                }
            }

            if (!advancedRenderState)
            {
                MaterialProperty renderStateBasicTypesProp = GetMaterialProperty(targets, renderStateBasicTypesPropName);
                _SetMaterialRenderStateValues((int)renderStateBasicTypesProp.floatValue);
            }

            MaterialProperty renderQueueEnumProp = GetMaterialProperty(targets, renderQueueEnumPropName);
            MaterialProperty renderQueueOffsetProp = GetMaterialProperty(targets, renderQueueOffsetPropName);
            if (renderQueueEnumProp != null && renderQueueOffsetProp != null)
            {
                _SetMaterialRenderQueue(renderQueueEnumProp, renderQueueOffsetProp);
            }

            //HelpBoxWithButton(new GUIContent("?"), new GUIContent("Help"));            
            //DefaultPreviewSettingsGUI();

            if (EditorGUI.EndChangeCheck())
            {
                PropertiesChanged();
            }
        }
    }

    private void _SetMaterialRenderStateValues(int renderStateValuesIndex)
    {
        RenderStateValues rsv = _renderStateValuesStructs[renderStateValuesIndex];

        GetMaterialProperty(targets, blendScPopName).floatValue = rsv._BlendSrc;
        GetMaterialProperty(targets, blendDstPropName).floatValue = rsv._BlendDst;
        GetMaterialProperty(targets, renderQueueEnumPropName).floatValue = rsv._RenderQueueEnum;
        GetMaterialProperty(targets, alphaMultipliesRGBPropName).floatValue = rsv._AlphaMultipliesRGB;
        GetMaterialProperty(targets, depthWritePropName).floatValue = rsv._DepthWrite;
        GetMaterialProperty(targets, cullModePropName).floatValue = (float)rsv._CullMode;
        GetMaterialProperty(targets, depthTestPropName).floatValue = (float)rsv._DepthTest;
    }

    private void _SetMaterialBlendState(MaterialProperty renderStateBasicTypesProp, MaterialProperty srcBlendProp, MaterialProperty dstBlendProp)
    {
        // Remember, the values are: Solid, Transparent, Additive, Screen, Multiply, Transparent Double Sided
        float src = (float)UnityEngine.Rendering.BlendMode.One;
        float dst = (float)UnityEngine.Rendering.BlendMode.Zero;

        if (renderStateBasicTypesProp.floatValue == 1)
        {
            src = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
            dst = (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
        }
        else if(renderStateBasicTypesProp.floatValue == 2)
        {
            dst = (float)UnityEngine.Rendering.BlendMode.One;
        } else if (renderStateBasicTypesProp.floatValue == 3)
        {
            src = (float)UnityEngine.Rendering.BlendMode.OneMinusDstColor;
            dst = (float)UnityEngine.Rendering.BlendMode.One;
        }
        else if (renderStateBasicTypesProp.floatValue == 4)
        {
            src = (float)UnityEngine.Rendering.BlendMode.DstColor;
            dst = (float)UnityEngine.Rendering.BlendMode.Zero;
        } else if (renderStateBasicTypesProp.floatValue == 5)
        {
            src = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
            dst = (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
        }

        // Only set the values if they have changed. Not sure that this check is necessary.
        if (srcBlendProp.floatValue != (float)src || dstBlendProp.floatValue != dst)
        {
            srcBlendProp.floatValue = src;
            dstBlendProp.floatValue = dst;
        }
    }


    private void _SetMaterialRenderQueue(MaterialProperty renderQueueEnumProp, MaterialProperty renderQueueOffsetProp)
    {
        int queueValue = (int)(renderQueueEnumProp.floatValue + renderQueueOffsetProp.floatValue);
        foreach (Material mat in targets)
        {
            mat.renderQueue = queueValue;
        }
    }


    private void _ResetMaterialRenderQueue()
    {
        foreach (Material mat in targets)
        {
            mat.renderQueue = -1;
        }
    }
}
