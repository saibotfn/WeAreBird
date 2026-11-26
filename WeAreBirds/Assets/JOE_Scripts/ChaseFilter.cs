using UnityEngine;
using UnityEngine.Rendering;

public class ChaseFilter : MonoBehaviour
{
    public static ChaseFilter Instance;
    public Volume volume;
    public float fadeSpeed = 2f;
    public bool isChased;

    void Awake()
    {
        Instance = this;
        volume.weight = 0f;
    }

    void Update()
    {
        float target = isChased ? 1f : 0f;
        volume.weight = Mathf.MoveTowards(volume.weight, target, fadeSpeed * Time.deltaTime);
    }
}