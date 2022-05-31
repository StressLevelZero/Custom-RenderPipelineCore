using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
/*
class ShaderVariantStripper : IPreprocessShaders
{
    public int callbackOrder { get { return 0; } }

    ShaderKeyword m_Lightmap;
    ShaderKeyword m_AddLights;
    ShaderKeyword m_AddLightSh;

    ShaderKeyword[] illegalVariants;
    string[] illegalVariantNames = new string[] { "_DBUFFER_MRT1", "_DBUFFER_MRT2", "_DBUFFER_MRT3", "_LIGHT_LAYERS", "_CLUSTERED_RENDERING",
    "DOTS_INSTANCING_ON", "FOG_LINEAR", "FOG_EXP", "DEBUG_DISPLAY", "_RENDER_PASS_ENABLED", "_GBUFFER_NORMALS_OCT", "EDITOR_VISUALIZATION"};

    ShaderKeyword[] illegalVariantsQuest;
    string[] illegalVariantNamesQuest = new string[] { "_ADDITIONAL_LIGHTS", "_ADDITIONAL_LIGHT_SHADOWS", "_REFLECTION_PROBE_BLENDING", 
        "_REFLECTION_PROBE_BOX_PROJECTION", "_SCREEN_SPACE_OCCLUSION", "_SHADOWS_SOFT"};

    ShaderKeyword[] illegalVariantsPC;
    string[] illegalVariantNamesPC = new string[] { "_ADDITIONAL_LIGHTS_VERTEX" };

    ShaderKeyword[] illegalVariantsNonLM;
    string[] illegalVariantNamesNonLM = new string[] { "DIRLIGHTMAP_COMBINED", "LIGHTMAP_SHADOW_MIXING", "SHADOWS_SHADOWMASK", "DYNAMICLIGHTMAP_ON"};


    public ShaderVariantStripper()
    {
        m_Lightmap = new ShaderKeyword("LIGHTMAP_ON");
        m_AddLights = new ShaderKeyword("_ADDITIONAL_LIGHTS");
        m_AddLightSh = new ShaderKeyword("_ADDITIONAL_LIGHT_SHADOWS");

        illegalVariants = populateKWArray(illegalVariantNames);
        illegalVariantsNonLM = populateKWArray(illegalVariantNamesNonLM);

#if UNITY_ANDROID
        illegalVariantsQuest = populateKWArray(illegalVariantNamesQuest);
#elif UNITY_STANDALONE
        illegalVariantsPC = populateKWArray(illegalVariantNamesPC);
#endif
    }



    public void OnProcessShader(
        Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> shaderCompilerData)
    {
        int stripCount = 0;
        for (int i = 0; i < shaderCompilerData.Count; ++i)
        {
            if (RemoveIfInList(ref shaderCompilerData, ref i, illegalVariants)) 
            {
                ++stripCount;
            }
#if UNITY_ANDROID
            else if (RemoveIfInList(ref shaderCompilerData, ref i, illegalVariantsQuest))
            {
                ++stripCount;
            }
#else
            else if (RemoveIfInList(ref shaderCompilerData, ref i, illegalVariantsPC))
            {
                ++stripCount;
            }
#endif
            else if (!shaderCompilerData[i].shaderKeywordSet.IsEnabled(m_Lightmap) && 
                RemoveIfInList(ref shaderCompilerData, ref i, illegalVariantsNonLM))
            {
                ++stripCount;
            }
            else if (shaderCompilerData[i].shaderKeywordSet.IsEnabled(m_AddLightSh) && !shaderCompilerData[i].shaderKeywordSet.IsEnabled(m_AddLights))
            {
                shaderCompilerData.RemoveAt(i);
                --i;
                ++stripCount;
            }     
        }
        if (stripCount > 0)
        {
            Debug.Log("Scriptable Shader Stripper: Stripped " + stripCount + " variants from " + shader.name + " " + snippet.shaderType + " pass " + snippet.pass.PassIndex + " " + snippet.passName);
        }
    }

    private ShaderKeyword[] populateKWArray(string[] kwNames)
    {
        ShaderKeyword[] kwList = new ShaderKeyword[kwNames.Length];
        for (int i = 0; i < kwNames.Length; i++)
        {
            kwList[i] = new ShaderKeyword(kwNames[i]);
        }
        return kwList;
    }
    private bool RemoveIfInList(ref IList<ShaderCompilerData> data, ref int index, ShaderKeyword[] keyList)
    {
        for (int i = 0; i < keyList.Length; ++i)
        {
            if (data[index].shaderKeywordSet.IsEnabled(keyList[i]))
            {
                data.RemoveAt(index);
                --index;
                return true;
            }
        }
        return false;
    }

    
}
*/