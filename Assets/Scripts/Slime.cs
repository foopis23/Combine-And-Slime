using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

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
        MoveInstant(TileLocation + offset);

        // create the new slime
        GameObject newSlimeObject = Instantiate(SlimePrefab, transform.position, Quaternion.Euler(0, 0, 0));
        Slime newSlime = newSlimeObject.GetComponent<Slime>();
        newSlime.Init();
        newSlime.SetScale(Scale);
        newSlime.Move(splitLocation);
        return newSlime;
    }

    public void MergeWith(Slime other, Vector3Int mergeLocation)
    {
        if(!CanMergeWith(other)) throw new CannotMergeException();

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
