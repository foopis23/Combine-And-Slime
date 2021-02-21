using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

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
public class Slime : MovableObject
{
    // static stuff
    public static Vector3[] SLIME_SCALES = {
        new Vector3(1, 1, 1),
        new Vector3(2, 2, 1),
        new Vector3(3, 3, 1)
    };

    // editor
    [SerializeField] private int StartingScale = 0;
    [SerializeField] private GameObject SlimePrefab;

    public override void Init()

    {
        if(!initialized)
        {
            tilemap = GameObject.FindGameObjectWithTag("FloorTiles").GetComponent<Tilemap>();
            TileLocation = tilemap.WorldToCell(transform.position);
            Destination = transform.position;
            OccupiedTiles = new HashSet<Vector3Int>();
            Scale = StartingScale;
            transform.localScale = SLIME_SCALES[Scale];
            UpdateTiles();

            initialized = true;
        }
    }

    protected override void UpdateObject() {}

    public bool CanSplit()
    {
        return Scale > 0;
    }

    public bool CanMergeWith(Slime other)
    {
        return Scale + 1 < SLIME_SCALES.Length && Scale == other.Scale;
    }

    public Slime Split(Vector3Int splitLocation)
    {
        if(!CanSplit()) throw new CannotSplitException();

        // decrease Scale and move the slime
        SetScale(Scale - 1);
        Vector3Int offset = splitLocation - TileLocation;
        offset.Clamp(Vector3Int.zero, new Vector3Int(1, 1, 0));

        // create the new slime
        GameObject newSlimeObject = Instantiate(SlimePrefab, transform.position, Quaternion.Euler(0, 0, 0));
        Slime newSlime = newSlimeObject.GetComponent<Slime>();
        newSlime.Init();
        newSlime.SetScale(Scale);

        EventSystem.Current.FireEvent(new SlimeSplitContext(this, newSlime));

        // move slimes
        newSlime.Move(splitLocation);
        MoveInstant(TileLocation + offset);

        return newSlime;
    }

    public void MergeWith(Slime other, Vector3Int mergeLocation)
    {
        if(!CanMergeWith(other)) throw new CannotMergeException();

        EventSystem.Current.FireEvent(new SlimeMergeContext(this, other));

        SetScale(Scale + 1);
        MoveInstant(mergeLocation);

        Destroy(other.gameObject);
    }
    
    public void SetScale(int newScale)
    {
        Scale = newScale;
        transform.localScale = SLIME_SCALES[Scale];
    }
}
