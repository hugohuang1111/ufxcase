using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(ScreenMagnifierRenderer), PostProcessEvent.AfterStack, "UFXCase/ScreenMagnifier")]
public sealed class ScreenMagnifier : PostProcessEffectSettings
{
    public FloatParameter Ratio = new FloatParameter { value = 0f };
    public FloatParameter Radius = new FloatParameter { value = 0f };
    [Range(1f, 10f), Tooltip("Amplify")]
    public FloatParameter Amplify = new FloatParameter { value = 0f };
    [Range(1f, 30f), Tooltip("Rate")]
    public FloatParameter Rate = new FloatParameter { value = 4f };
    public Vector2Parameter Postion = new Vector2Parameter { value = new Vector2(256, 256) };
}

public sealed class ScreenMagnifierRenderer : PostProcessEffectRenderer<ScreenMagnifier>
{
    private Vector2 ScreenSize;

    public override void Render(PostProcessRenderContext context)
    {
        ScreenSize.x = UnityEngine.Screen.width;
        ScreenSize.y = UnityEngine.Screen.height;

        var sheet = context.propertySheets.Get(Shader.Find("UFXCase/ScreenMagnifier"));
        sheet.properties.SetFloat("_Radius", settings.Radius / ScreenSize.y);
        sheet.properties.SetFloat("_Ratio", settings.Ratio);
        sheet.properties.SetFloat("_Amplify", settings.Amplify);
        sheet.properties.SetFloat("_Rate", settings.Rate);
        sheet.properties.SetVector("_Position", settings.Postion / ScreenSize);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }

}
