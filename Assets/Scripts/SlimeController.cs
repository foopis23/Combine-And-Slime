using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

public class SlimeController : MonoBehaviour
{
    public static int moveCount;
    public static int mergeCount;
    public static int splitCount;

    //editor properties
    [SerializeField] private LayerMask selectionLayer;
    [SerializeField] private TileBase validMoveTile;
    [SerializeField] private TileBase possibleMoveTile;
    [SerializeField] private TileBase possibleSplitTile;
    [SerializeField] private TileBase possibleMergeTile;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap movementOverlay;
    [SerializeField] private Tilemap cursorOverlay;
    [SerializeField] private Slime startingSlime;
    [SerializeField] private Slime[] placedSlimes;

    //internal properties
    private Camera mainCamera;
    private Slime currentSlime;
    private Slime mergeTarget;
    private HashSet<Slime> allSlimes;
    private Dictionary<Vector3Int, Vector3Int> moveLocationFromMouseTileLocation;
    private Dictionary<Vector3Int, Vector3Int> splitLocationFromMouseTileLocation;
    private Dictionary<Vector3Int, Vector3Int> mergeLocationFromTargetLocation;
    private Dictionary<Vector3Int, Slime> slimeFromTileLocation;
    private bool getMoves;
    private bool performSplit;

    void Start()
    {
        currentSlime = startingSlime;
        allSlimes = new HashSet<Slime>();
        moveLocationFromMouseTileLocation = new Dictionary<Vector3Int, Vector3Int>();
        splitLocationFromMouseTileLocation = new Dictionary<Vector3Int, Vector3Int>();
        mergeLocationFromTargetLocation = new Dictionary<Vector3Int, Vector3Int>();
        slimeFromTileLocation = new Dictionary<Vector3Int, Slime>();
        mainCamera = Camera.main;
        mergeTarget = null;
        getMoves = true;
        performSplit = false;

        moveCount = 0;
        mergeCount = 0;
        splitCount = 0;

        foreach(Slime slime in placedSlimes)
        {
            AddSlime(slime);
        }
    }

    private void AddSlime(Slime slime)
    {
        allSlimes.Add(slime);
        foreach(Vector3Int tile in slime.OccupiedTiles)
        {
            slimeFromTileLocation.Add(tile, slime);
        }
    }

    private bool RemoveSlime(Slime slime)
    {
        if(allSlimes.Contains(slime))
        {
            foreach(Vector3Int tile in slime.OccupiedTiles)
            {
                slimeFromTileLocation.Remove(tile);
            }
        }

        return allSlimes.Remove(slime);
    }

    private Vector3 roundToGrid(Vector3 pos)
    {
        Vector3Int cellPos = tilemap.WorldToCell(new Vector3(pos.x, pos.y, 0));
        Vector3 gridWorldPos = tilemap.CellToWorld(cellPos); //makes pos on grid
        return gridWorldPos;
    }

    private Vector3Int GetMergeOffset(Slime slime)
    {
        if(!currentSlime.CanMergeWith(slime)) throw new CannotMergeException();

        List<Vector3Int> mergeOffsets = new List<Vector3Int>();
        int width = currentSlime.Scale + 2;
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < width; y++)
            {
                mergeOffsets.Add(new Vector3Int(x, y, 0));
            }
        }

        mergeOffsets.Sort((a, b) => (a.x + a.y - b.x + b.y));
        foreach(Vector3Int offset in mergeOffsets)
        {
            bool isValidMergeLocation = true;
            for(int h = 0; h < width; h++)
            {
                for(int k = 0; k < width; k++)
                {
                    if(!tilemap.HasTile(slime.TileLocation - offset + new Vector3Int(h, k, 0)))
                    {
                        isValidMergeLocation = false;
                        break;
                    }
                }
            }

            if(isValidMergeLocation)
            {
                return -offset;
            }
        }

        throw new CannotMergeException();
    }

    private void GetValidMoves()
    {
        movementOverlay.ClearAllTiles();
        moveLocationFromMouseTileLocation.Clear();
        splitLocationFromMouseTileLocation.Clear();
        mergeLocationFromTargetLocation.Clear();
        int width = currentSlime.Scale + 1;

        Vector3Int testPos;
        Vector3Int splitPos = Vector3Int.zero;
        HashSet<Vector3Int> validTiles = new HashSet<Vector3Int>();
        Vector3Int[] finalPositions = new Vector3Int[4];

        Vector3Int[] directions = {
            new Vector3Int( 1,  0, 0),
            new Vector3Int(-1,  0, 0),
            new Vector3Int( 0,  1, 0),
            new Vector3Int( 0, -1, 0)
        };

        for(int i = 0; i < 4; i++)
        {
            Vector3Int direction = directions[i];

            if(performSplit)
            {
                testPos = currentSlime.TileLocation + direction * (direction.x > 0 || direction.y > 0 ? width : width - 1);
                splitPos = currentSlime.TileLocation + direction * (direction.x > 0 || direction.y > 0 ? 1 : 0);
            }
            else
            {
                testPos = new Vector3Int(currentSlime.TileLocation.x, currentSlime.TileLocation.y, currentSlime.TileLocation.z);
            }

            Vector3Int perpendicular = direction.x == 0 ? directions[0] : directions[2];
            HashSet<Vector3Int> possibleValidTiles;
            bool piss = false; //? Now if this isn't me when I am pissing I don't know what is
            bool valid;
            do
            {
                if(!performSplit)
                {
                    testPos += direction;
                }

                possibleValidTiles = new HashSet<Vector3Int>();
                valid = true;
                int squareWidth = performSplit ? width - 1 : width;
                //check if every tile in the area covered by the slime is valid
                for(int h = 0; h < squareWidth; h++)
                {
                    for(int k = 0; k < squareWidth; k++)
                    {
                        Vector3Int slimeTestPos = new Vector3Int(testPos.x + h, testPos.y + k, testPos.z);
                        possibleValidTiles.Add(slimeTestPos);
                        if (!tilemap.HasTile(slimeTestPos) || slimeFromTileLocation.ContainsKey(slimeTestPos))
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                possibleValidTiles.ExceptWith(currentSlime.OccupiedTiles);

                if(valid)
                {
                    // map the tiles the the slime will take up to the actual slime position the move would put you at
                    foreach(Vector3Int tilePos in possibleValidTiles)
                    {
                        if(!moveLocationFromMouseTileLocation.ContainsKey(tilePos))
                        {
                            moveLocationFromMouseTileLocation.Add(tilePos, testPos);
                        }
                    }

                    // add the tiles to valid tiles
                    validTiles.UnionWith(possibleValidTiles);
                }

                if(piss)
                {
                    break;
                }

                if(performSplit)
                {
                    piss = true;
                    testPos += perpendicular;
                }
            }
            while(valid);

            finalPositions[i] = testPos;
        }

        for(int i = 0; i < 4; i++)
        {
            Vector3Int testMergePos = finalPositions[i] + directions[i] * currentSlime.Scale;
            if(slimeFromTileLocation.ContainsKey(testMergePos))
            {
                Slime testMergeSlime = slimeFromTileLocation[testMergePos];
                if(testMergeSlime.TileLocation == testMergePos && testMergeSlime.Scale == currentSlime.Scale)
                {
                    try
                    {
                        Vector3Int mergeLocation = testMergePos + GetMergeOffset(testMergeSlime);
                        mergeLocationFromTargetLocation.Add(testMergeSlime.TileLocation, mergeLocation);
                        for(int h = 0; h < width + 1; h++)
                        {
                            for(int k = 0; k < width + 1; k++)
                            {
                                Vector3Int tilePos = mergeLocation + new Vector3Int(h, k, 0);
                                if(!moveLocationFromMouseTileLocation.ContainsKey(tilePos))
                                {
                                    moveLocationFromMouseTileLocation.Add(tilePos, testMergePos);
                                }

                                movementOverlay.SetTile(tilePos, possibleMergeTile);
                            }
                        }
                    }
                    catch(CannotMergeException) {}
                }
            }
        }

        foreach(Vector3Int tilePos in validTiles)
        {
            movementOverlay.SetTile(tilePos, performSplit ? possibleSplitTile : possibleMoveTile);
        }

        foreach(Vector3Int tilePos in currentSlime.OccupiedTiles)
        {
            moveLocationFromMouseTileLocation.Remove(tilePos);
            if(currentSlime.CanSplit() && !performSplit)
            {
                movementOverlay.SetTile(tilePos, possibleSplitTile);
            }
        }
    }

    private void MoveCurrentSlime(Vector3Int tileLocation)
    {
        movementOverlay.ClearAllTiles();
        cursorOverlay.ClearAllTiles();

        if(slimeFromTileLocation.ContainsKey(tileLocation) && slimeFromTileLocation[tileLocation].TileLocation == tileLocation)
        {
            mergeTarget = slimeFromTileLocation[tileLocation];
        }

        currentSlime.Move(tileLocation);
        moveCount++;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mouseTileLocation = tilemap.WorldToCell(new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0));

        bool isValidSelectionTile = moveLocationFromMouseTileLocation.ContainsKey(mouseTileLocation);

        if(!currentSlime.IsMoving)
        {
            if (mergeTarget != null && !currentSlime.IsMoving)
            {
                RemoveSlime(mergeTarget);
                currentSlime.MergeWith(mergeTarget, mergeLocationFromTargetLocation[mergeTarget.TileLocation]);
                mergeCount++;
                mergeTarget = null;
                getMoves = true;
            }

            // this is just run every time the slime reaches its dest
            if(getMoves)
            {
                GetValidMoves();
                getMoves = false;
            }

            cursorOverlay.ClearAllTiles();
            if (isValidSelectionTile && moveLocationFromMouseTileLocation.ContainsKey(mouseTileLocation))
            {
                Vector3Int moveTilePos = moveLocationFromMouseTileLocation[mouseTileLocation];
                int highlightSize = performSplit ? currentSlime.Scale : currentSlime.Scale + 1;
                if(mergeLocationFromTargetLocation.ContainsKey(moveTilePos))
                {
                    moveTilePos = mergeLocationFromTargetLocation[moveTilePos];
                    highlightSize++;
                }

                for(int h = 0; h < highlightSize; h++)
                {
                    for(int k = 0; k < highlightSize; k++)
                    {
                        cursorOverlay.SetTile(new Vector3Int(moveTilePos.x + h, moveTilePos.y + k, moveTilePos.z), validMoveTile);
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (isValidSelectionTile)
                {
                    if(performSplit)
                    {
                        // split the slime to the selected location
                        Slime splitSlime = currentSlime;
                        currentSlime = currentSlime.Split(moveLocationFromMouseTileLocation[mouseTileLocation]);
                        AddSlime(splitSlime);
                        performSplit = false;
                        splitCount++;
                    }
                    else
                    {
                        // move slime to the selected location
                        MoveCurrentSlime(moveLocationFromMouseTileLocation[mouseTileLocation]);
                    }

                    getMoves = true;
                }
                else if(currentSlime.OccupiedTiles.Contains(mouseTileLocation) && currentSlime.CanSplit())
                {
                    // initiate split
                    performSplit = true;
                    getMoves = true;
                }
            }
        }
    }
}
