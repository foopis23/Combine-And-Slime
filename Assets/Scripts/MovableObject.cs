using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

public class ObjectStartMovingContext : EventContext {
    public MovableObject obj;

    public ObjectStartMovingContext(MovableObject obj) {
        this.obj = obj;
    }
}

public class ObjectFinishMovingContext : EventContext {
    public MovableObject obj;

    public ObjectFinishMovingContext(MovableObject obj) {
        this.obj = obj;
    }
}

public abstract class MovableObject : MonoBehaviour
{
    //editor properties
    [SerializeField] protected float MoveSpeed = 10.0f;
    [SerializeField] protected float StoppingDistance = 0.001f;

    //internal
    protected Tilemap tilemap;
    protected bool atDestination = true;
    protected bool initialized = false;

    // public properties
    public bool IsMoving {
        get {
            return !atDestination;
        }
    }
    public Vector3 Destination { get; protected set; }
    public int Scale { get; protected set; }
    public Vector3Int TileLocation { get; protected set; }
    public HashSet<Vector3Int> OccupiedTiles { get; protected set; }

    public abstract void Init();
    protected abstract void UpdateObject();

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Init();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        UpdateObject();

        if (!atDestination)
        {
            float dist = Vector3.Distance(transform.position, Destination);
            transform.position = Vector3.MoveTowards(transform.position, Destination, MoveSpeed * Time.deltaTime);

            atDestination = dist < StoppingDistance;

            if (atDestination) FinishedMove();
        }
    }

    public void Move(Vector3Int tileLocation)
    {
        EventSystem.Current.FireEvent(new ObjectStartMovingContext(this));
        SetDestination(tilemap.CellToWorld(tileLocation));
        TileLocation = tileLocation;
        UpdateTiles();
    }

    public void MoveInstant(Vector3Int tileLocation)
    {
        EventSystem.Current.FireEvent(new ObjectFinishMovingContext(this));
        transform.position = tilemap.CellToWorld(tileLocation);
        TileLocation = tileLocation;
        UpdateTiles();
    }

    protected void FinishedMove()
    {
        EventSystem.Current.FireEvent(new ObjectFinishMovingContext(this));
    }

    protected void SetDestination(Vector3 pos)
    {
        Destination = pos;
        atDestination = false;
    }

    protected void UpdateTiles()
    {
        OccupiedTiles.Clear();
        for(int h = 0; h < Scale + 1; h++)
        {
            for(int k = 0; k < Scale + 1; k++)
            {
                OccupiedTiles.Add(TileLocation + new Vector3Int(h, k, 0));
            }
        }
    }
}
