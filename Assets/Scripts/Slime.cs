using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour
{
    public float MoveSpeed = 1.0f;
    public float StoppingDstiance = 0.1f;

    private Vector3 destination;
    private bool atDestination;

    public bool IsMoving {
        get{
            return !atDestination;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        atDestination = true;
        destination = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!atDestination) {
            transform.position = Vector3.MoveTowards(transform.position, destination, MoveSpeed * Time.deltaTime);

            atDestination = Vector3.Distance(transform.position, destination) < StoppingDstiance;
        }
    }

    public void SetDestination(Vector3 pos) {
        destination = pos;
        atDestination = false;
    }
}
