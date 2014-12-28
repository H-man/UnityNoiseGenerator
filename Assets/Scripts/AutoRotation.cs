using UnityEngine;

namespace Assets.Scripts
{
    public class AutoRotation : MonoBehaviour
    {
        public float Speed = 15f;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var speed = Speed*Time.deltaTime;
            transform.Rotate(new Vector3(speed / 1.5f, speed/ 2f, speed));
        }
    }
}
