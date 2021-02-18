using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour
{
    // static stuff
    public static Vector3[] SLIME_SCALES = {
        new Vector3(1, 1, 1),
        new Vector3(2, 2, 1)
    };

    // public
    public bool IsMoving {
        get{
            return !atDestination;
        }
    }
    public int Scale {
        get {
            return scale;
        }
    }

    // editor
    [SerializeField] private int startingScale = 0;
    [SerializeField] private float MoveSpeed = 1.0f;
    [SerializeField] private float StoppingDistance = 0.1f;
    [SerializeField] private int SlimeLayer;

    // internal
    private int scale = -1;
    private Vector3 destination;
    private bool atDestination;
    private float fullDistance;



    // Start is called before the first frame update
    void Start()
    {
        atDestination = true;
        destination = transform.position;
        if (scale == -1)
            scale = startingScale;
        transform.localScale = SLIME_SCALES[scale];
    }

    // Update is called once per frame
    void Update()
    {
        if (!atDestination) {
            float dist = Vector3.Distance(transform.position, destination);
            transform.position = Vector3.MoveTowards(transform.position, destination, MoveSpeed * Time.deltaTime);

            atDestination = dist < StoppingDistance;
        }
    }

    public void SetDestination(Vector3 pos) {
        destination = pos;
        atDestination = false;
        fullDistance = Vector3.Distance(transform.position, destination);
    }
    public bool SetScale(int val) {
        if (val < 0 || val >= SLIME_SCALES.Length) return false;

        scale = val;
        transform.localScale = SLIME_SCALES[scale];

        return true; 
    }

    public bool Split() {
        if (scale-1 < 0) return false;
        scale--;
        transform.localScale = SLIME_SCALES[scale];
        return true;
    }


    public bool Merge() {
        if (scale+1 >= SLIME_SCALES.Length) return false;
        scale++;
        transform.localScale = SLIME_SCALES[scale];
        return true;        
    }

    public bool CanMerge() {
        return scale+1 < SLIME_SCALES.Length;
    }

    public bool CanSplit() {
        return scale-1 > -1;
    }
}
