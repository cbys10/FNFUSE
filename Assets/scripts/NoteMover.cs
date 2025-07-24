using UnityEngine;

public class NoteMover : MonoBehaviour
{
    private float strumTime;
    private AudioSource songAudio;
    private float speed;

    public void Init(float strum, AudioSource src, float spd)
    {
        strumTime = strum / 1000f; // Convert ms to seconds
        songAudio = src;
        speed = spd;
    }

    void Update()
    {
        float timeUntilHit = strumTime - songAudio.time;
        transform.localPosition = new Vector3(transform.localPosition.x, timeUntilHit * speed, transform.localPosition.z);
    }
}
