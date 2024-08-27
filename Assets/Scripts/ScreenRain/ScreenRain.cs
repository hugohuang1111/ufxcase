using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(ScreenRainRenderer), PostProcessEvent.AfterStack, "UFXCase/ScreenRain")]
public sealed class ScreenRain : PostProcessEffectSettings
{
    [Range(0f, 3.14f), Tooltip("rain direction")]
    public FloatParameter rotate = new FloatParameter { value = 0f };
    [Range(0f, 10f), Tooltip("rain speed")]
    public FloatParameter speed = new FloatParameter { value = 1f };
    public Vector2Parameter winSize = new Vector2Parameter { value = new Vector2(256, 256) };
}

public sealed class ScreenRainRenderer : PostProcessEffectRenderer<ScreenRain>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("UFXCase/ScreenRain"));
        sheet.properties.SetFloat("_Rotate", settings.rotate);
        sheet.properties.SetFloat("_Speed", settings.speed);
        sheet.properties.SetVector("_WinSize", settings.winSize);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
