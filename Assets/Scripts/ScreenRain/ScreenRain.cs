using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(ScreenRainRenderer), PostProcessEvent.AfterStack, "UFXCase/ScreenRain")]
public sealed class ScreenRain : PostProcessEffectSettings
{
    [Range(-0.3f, 0.6f), Tooltip("rain amount")]
    public FloatParameter rainAmount = new FloatParameter { value = 0f };
}

public sealed class ScreenRainRenderer : PostProcessEffectRenderer<ScreenRain>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("UFXCase/ScreenRain"));
        sheet.properties.SetFloat("_RainAmount", settings.rainAmount);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
