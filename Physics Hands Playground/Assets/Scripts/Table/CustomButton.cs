using UnityEngine;

public class CustomButton : MonoBehaviour
{
    [SerializeField]
    private Color _customColor = Color.white;

    [SerializeField]
    private Renderer _buttonInner, _buttonOuter;

    void Start()
    {
        ApplySetup();
    }

    private void OnValidate()
    {
        ApplySetup();
    }

    private void ApplySetup()
    {
        MaterialColorUtils.ApplyColorThemeToRenderers(_customColor, _buttonInner, _buttonOuter, null, null);
    }
}
