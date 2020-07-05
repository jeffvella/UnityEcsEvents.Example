using UnityEngine;
using UnityEngine.UI;

public class CameraScroll : MonoBehaviour
{
    public float moveSpeed = 0.5f;

    public Slider speedSlider;

    // Use this for initialization
    private void Start()
    {
        speedSlider.onValueChanged.AddListener(delegate { ChangeSpeed(); });
    }

    // Update is called once per frame
    private void Update()
    {
        //Move the camera to the left based on current speedSlider setting
        transform.Translate(Vector3.left * (Time.deltaTime * moveSpeed));

        //If the camera passes the last animation, loop to the beginning
        if (transform.position.x > 110)
            transform.position = new Vector3(0f, transform.position.y, transform.position.z);
    }

    private void ChangeSpeed()
    {
        moveSpeed = speedSlider.value;
    }
}