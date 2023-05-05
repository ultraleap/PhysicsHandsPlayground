using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//If you rotate the Player GO towards tutorial bot, this will always recenter it back to tutorial bot at runtime when called
public class PlayerRecenter : MonoBehaviour
{
    [SerializeField] public GameObject anchor;
    [SerializeField] private GameObject player;

    private int _initialFrames = 2;

    public void Start()
    {
        StartCoroutine(RecenterAfterFrames(_initialFrames));
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Recenter();
        }
    }

    public void Recenter()
    {
        float q = (anchor.transform.rotation * Quaternion.Inverse(player.transform.rotation) * gameObject.transform.rotation).eulerAngles.y;
        gameObject.transform.rotation = Quaternion.Euler(0, q, 0);

        gameObject.transform.position -= player.transform.position - anchor.transform.position;
    }

    IEnumerator RecenterAfterFrames(int frames)
    {
        int frameCount = frames;
        while(frameCount > 0)
        {
            frameCount--;
            yield return null;
        }
        Recenter();
    }
}
