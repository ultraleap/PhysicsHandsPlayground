using UnityEngine;

public class DeleteIfAndroid : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Destroy(gameObject);
#endif
    }
}
