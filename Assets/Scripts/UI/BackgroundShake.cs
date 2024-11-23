using UnityEngine;

public class BackgroundShake : MonoBehaviour
{
    public float shakeAmount = 0.1f;
    public float shakeSpeed = 1.0f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
        float offsetY = Mathf.Cos(Time.time * shakeSpeed) * shakeAmount;

        transform.position = startPos + new Vector3(offsetX, offsetY, 0);
    }
}
