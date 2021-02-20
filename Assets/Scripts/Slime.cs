using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

public class SlimeStartMovingContext : EventContext {
    public Slime Slime;

    public SlimeStartMovingContext(Slime Slime) {
        this.Slime = Slime;
    }
}

public class SlimeFinishMovingContext : EventContext {
    public Slime Slime;

    public SlimeFinishMovingContext(Slime Slime) {
        this.Slime = Slime;
    }
}

public class SlimeSplitContext : EventContext {
    public Slime OldSlime;
    public Slime NewSlime;

    public SlimeSplitContext(Slime OldSlime, Slime NewSlime) {
        this.OldSlime = OldSlime;
        this.NewSlime = NewSlime;
    }
}

public class SlimeMergeContext : EventContext {
    public Slime Slime;
    public Slime Assimilated;

    public SlimeMergeContext(Slime Slime, Slime Assimilated) {
        this.Slime = Slime;
        this.Assimilated = Assimilated;
    }
}

public class CannotMergeException : Exception
{
    public CannotMergeException() {}
    public CannotMergeException(string message) : base(message) {}
    public CannotMergeException(string message, Exception inner) : base(message, inner) {}
}

public class CannotSplitException : Exception
{
    public CannotSplitException() {}
    public CannotSplitException(string message) : base(message) {}
    public CannotSplitException(string message, Exception inner) : base(message, inner) {}
}
public class Slime : MonoBehaviour
{
    // static stuff
    public static Vector3[] SLIME_SCALES = {
        new Vector3(1, 1, 1),
        new Vector3(2, 2, 1),
        new Vector3(3, 3, 1)
    };

    // editor
    [SerializeField] private int StartingScale = 0;
    [SerializeField] private float MoveSpeed = 10.0f;
    [SerializeField] private float StoppingDistance = 0.001f;
    [SerializeField] private GameObject SlimePrefab;

    // internal
    private Tilemap tilemap;
    private int scale = -1;
    private Vector3 destination;
    private bool atDestination = true;
    private bool initialized = false;

    // public properties
    public bool IsMoving {
        get {
            return !atDestination;
        }
    }
    public int Scale {
        get {
            return scale;
        }
    }
    public Vector3Int TileLocation { get; private set; }
    public HashSet<Vector3Int> OccupiedTiles { get; private set; }

    void Init()
    {
        if(!initialized)
        {
            tilemap = GameObject.FindGameObjectWithTag("FloorTiles").GetComponent<Tilemap>();
            TileLocation = tilemap.WorldToCell(transform.position);
            OccupiedTiles = new HashSet<Vector3Int>();
            if (scale == -1) scale = StartingScale;
            transform.localScale = SLIME_SCALES[scale];
            UpdateTiles();

            initialized = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (!atDestination)
        {
            float dist = Vector3.Distance(transform.position, destination);
            transform.position = Vector3.MoveTowards(transform.position, destination, MoveSpeed * Time.deltaTime);

            atDestination = dist < StoppingDistance;

            if (atDestination) FinishedMove();
        }
    }

    public bool CanSplit()
    {
        return scale > 0;
    }

    public bool CanMergeWith(Slime other)
    {
        return scale + 1 < SLIME_SCALES.Length && scale == other.Scale;
    }

    public Slime Split(Vector3Int splitLocation)
    {
        if(!CanSplit()) throw new CannotSplitException();

        // decrease scale and move the slime
        SetScale(scale - 1);
        Vector3Int offset = splitLocation - TileLocation;
        offset.Clamp(Vector3Int.zero, new Vector3Int(1, 1, 0));


        // create the new slime
        GameObject newSlimeObject = Instantiate(SlimePrefab, transform.position, Quaternion.Euler(0, 0, 0));
        Slime newSlime = newSlimeObject.GetComponent<Slime>();
        newSlime.Init();
        newSlime.SetScale(scale);

        EventSystem.Current.FireEvent(new SlimeSplitContext(this, newSlime));

        // move slimes
        MoveInstant(TileLocation + offset);
        newSlime.Move(splitLocation);

        return newSlime;
    }

    public void MergeWith(Slime other, Vector3Int mergeLocation)
    {
        if(!CanMergeWith(other)) throw new CannotMergeException();

        EventSystem.Current.FireEvent(new SlimeMergeContext(this, other));

        SetScale(scale + 1);
        MoveInstant(mergeLocation);

        Destroy(other.gameObject);
    }

    public void Move(Vector3Int tileLocation)
    {
        EventSystem.Current.FireEvent(new SlimeStartMovingContext(this));

        SetDestination(tilemap.CellToWorld(tileLocation));
        TileLocation = tileLocation;
        UpdateTiles();
    }

    private void FinishedMove() {
        EventSystem.Current.FireEvent(new SlimeFinishMovingContext(this));
    }

    public void MoveInstant(Vector3Int tileLocation)
    {
        transform.position = tilemap.CellToWorld(tileLocation);
        TileLocation = tileLocation;
        UpdateTiles();

        EventSystem.Current.FireEvent(new SlimeFinishMovingContext(this));
    }
    
    private void SetScale(int newScale)
    {
        scale = newScale;
        transform.localScale = SLIME_SCALES[scale];
    }

    private void SetDestination(Vector3 pos)
    {
        destination = pos;
        atDestination = false;
    }

    private void UpdateTiles()
    {
        OccupiedTiles.Clear();
        for(int h = 0; h < scale + 1; h++)
        {
            for(int k = 0; k < scale + 1; k++)
            {
                OccupiedTiles.Add(new Vector3Int(TileLocation.x + h, TileLocation.y + k, TileLocation.z));
            }
        }
    }
}
