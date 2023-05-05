using UnityEngine;

public static class MaterialColorUtils
{
    public static void ApplyColorThemeToRenderers(Color color,  Renderer light, Renderer dark, Renderer transparent, Renderer transparentDark)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        if(light != null)
        {
            ApplySingleColor(Color.HSVToRGB(h, s * .9f, v), light);
        }
        if(dark != null)
        {
            ApplySingleColor(Color.HSVToRGB(h, s * .9f, v * .65f), dark);
        }
        if(transparent != null)
        {
            ApplySingleColor(color, transparent);
        }
        if(transparent != null)
        {
            ApplySingleColor(new Color(color.r, color.g, color.b, color.a * .9f), transparentDark);
        }
    }

    public static void ApplySingleColor(Color color, Renderer renderer)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(block);
    }
}
