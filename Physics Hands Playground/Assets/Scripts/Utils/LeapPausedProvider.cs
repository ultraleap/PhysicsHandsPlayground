using Leap;
using Leap.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Used for debugging hand pose specific issues
/// </summary>
public class LeapPausedProvider : PostProcessProvider
{
    [SerializeField]
    private Key _pauseKey = Key.UpArrow;

    private bool _paused = false;

    private Frame _copyFrame = new Frame();

    private void Update()
    {
        if (Keyboard.current[_pauseKey].wasPressedThisFrame)
        {
            _paused = !_paused;
        }
    }

    public override void ProcessFrame(ref Frame inputFrame)
    {
        if (_paused)
        {
            inputFrame.CopyFrom(_copyFrame);
        }
        else
        {
            _copyFrame.CopyFrom(inputFrame);
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        dataUpdateMode = DataUpdateMode.UpdateAndFixedUpdate;
    }
}
