using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

public class SlimeController : MonoBehaviour
{
    //editor properties
    [SerializeField] private LayerMask selectionLayer;
    [SerializeField] private Tilemap movementOverlay;
    [SerializeField] private Tilemap cursorOverlay;
    [SerializeField] private TileBase possibleMoveTile;
    [SerializeField] private TileBase validMoveTile;
    [SerializeField] private Tilemap buttonMap;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private GameObject slimePrefab;

    //internal properties
    private Camera mainCamera;
    private Slime currentSlime;
    private GameObject currentSlimeObject;
    private Vector3Int currentSlimeTileLocation;
    private HashSet<Vector3Int> slimeTiles;
    private GameObject mergeWith;
    private Dictionary<Vector3Int, GameObject> slimes;
    private Dictionary<Vector3Int, Vector3Int> mouseOverTilePositions;
    private Dictionary<Vector3Int, Vector3Int> splitTilePositions;
    private bool getMoves;
    private bool performSplit;

    void Start()
    {
        slimes = new Dictionary<Vector3Int, GameObject>();
        mainCamera = Camera.main;
        mergeWith = null;
        getMoves = true;
        performSplit = false;

        if (currentSlime == null)
        {
            currentSlimeObject = GameObject.FindGameObjectWithTag("slime");
            currentSlime = currentSlimeObject?.GetComponent<Slime>();
            currentSlimeTileLocation = tilemap.WorldToCell(new Vector3(currentSlimeObject.transform.position.x, currentSlimeObject.transform.position.y, 0));
            GetValidMoves();
        }
    }

    private GameObject SelectSlime(Vector3 mouseWorldPos)
    {
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector3.zero, Mathf.Infinity, selectionLayer);

        if (hit.collider != null)
        {
            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

    private void AddSlime(GameObject slime)
    {
        Vector3Int gridPos = tilemap.WorldToCell(new Vector3(slime.transform.position.x, slime.transform.position.y, 0));
        slimes.Add(gridPos, slime);

        //activate button if this slime is standing on one
        SetButtonActive(gridPos, true);
    }

    private GameObject GetSlime(Vector3 worldPos)
    {
        Vector3Int gridPos = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
        if (!slimes.ContainsKey(gridPos)) return null;

        return slimes[gridPos];
    }

    private bool RemoveSlime(Vector3 worldPos)
    {
        Vector3Int gridPos = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));

        bool res = slimes.Remove(gridPos);


        if (res) {
            //deactivate button if this slime is standing on one
            SetButtonActive(gridPos, false);
        }

        return res;
    }

    private Vector3 roundToGrid(Vector3 pos)
    {
        Vector3Int cellPos = tilemap.WorldToCell(new Vector3(pos.x, pos.y, 0));
        Vector3 gridWorldPos = tilemap.CellToWorld(cellPos); //makes pos on grid
        return gridWorldPos;
    }

    private void PlaceSlime(Vector3 mouseWorldPos, GameObject slime)
    {
        Vector3 gridWorldPos = roundToGrid(mouseWorldPos);
        slime.transform.position = new Vector2(gridWorldPos.x, gridWorldPos.y);
    }

    private void GetValidMoves()
    {
        movementOverlay.ClearAllTiles();
        mouseOverTilePositions = new Dictionary<Vector3Int, Vector3Int>();
        splitTilePositions = new Dictionary<Vector3Int, Vector3Int>();
        int width = currentSlime.Scale + 1;

        Debug.Log(width);

        Vector3Int testPos;
        Vector3Int splitPos = Vector3Int.zero;
        HashSet<Vector3Int> validTiles = new HashSet<Vector3Int>();

        // get the tiles taken up by the slime
        slimeTiles = new HashSet<Vector3Int>();
        for(int h = 0; h < width; h++)
        {
            for(int k = 0; k < width; k++)
            {
                slimeTiles.Add(new Vector3Int(currentSlimeTileLocation.x + h, currentSlimeTileLocation.y + k, currentSlimeTileLocation.z));
            }
        }

        Vector3Int[] directions = {
            new Vector3Int( 1,  0, 0),
            new Vector3Int(-1,  0, 0),
            new Vector3Int( 0,  1, 0),
            new Vector3Int( 0, -1, 0)
        };

        foreach(Vector3Int direction in directions)
        {
            if(performSplit)
            {
                testPos = currentSlimeTileLocation + direction * (direction.x > 0 || direction.y > 0 ? width : width - 1);
                splitPos = currentSlimeTileLocation + direction * (direction.x > 0 || direction.y > 0 ? 1 : 0);
            }
            else
            {
                testPos = new Vector3Int(currentSlimeTileLocation.x, currentSlimeTileLocation.y, currentSlimeTileLocation.z);
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
                        if (!tilemap.HasTile(slimeTestPos))
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                possibleValidTiles.ExceptWith(slimeTiles);

                if(valid)
                {
                    // map the tiles the the slime will take up to the actual slime position the move would put you at
                    foreach(Vector3Int tilePos in possibleValidTiles)
                    {
                        if(!mouseOverTilePositions.ContainsKey(tilePos))
                        {
                            mouseOverTilePositions.Add(tilePos, testPos);
                        }
                        if(performSplit && !splitTilePositions.ContainsKey(tilePos))
                        {
                            splitTilePositions.Add(tilePos, splitPos + (piss ? perpendicular : Vector3Int.zero));
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
        }

        foreach(Vector3Int tilePos in validTiles)
        {
            movementOverlay.SetTile(tilePos, possibleMoveTile);
        }
    }

    private void MoveSelectedSlime(Vector3 worldPos)
    {
        GameObject temp = GetSlime(worldPos);

        movementOverlay.ClearAllTiles();
        cursorOverlay.ClearAllTiles();

        if (temp != null)
        {
            Slime slime = temp.GetComponent<Slime>();

            if (currentSlime.CanMerge() && slime.Scale == currentSlime.Scale)
            {
                Vector3 pos = roundToGrid(worldPos);
                currentSlime.SetDestination(new Vector3(pos.x, pos.y, currentSlimeObject.transform.position.z));
                currentSlimeTileLocation = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
                mergeWith = temp;
                RemoveSlime(worldPos);
            }

        }
        else
        {
            Vector3 pos = roundToGrid(worldPos);
            currentSlime.SetDestination(new Vector3(pos.x, pos.y, currentSlimeObject.transform.position.z));
            currentSlimeTileLocation = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
        }
    }

    private void SetButtonActive(Vector3Int slimeTilePos, bool isActive) {
        if (buttonMap.HasTile(slimeTilePos)) {
            TileBase tile = buttonMap.GetTile(slimeTilePos);
            if (isActive) {
                EventSystem.Current.FireEvent(new ActivateButtonContext(tile));
            }else{
                EventSystem.Current.FireEvent(new DeactivateButtonContext(tile));
            }
        }
    }

    private void SetButtonActive(Vector3 slimePos, bool isActive) {
        Vector3Int slimeTilePos = buttonMap.WorldToCell(new Vector3(slimePos.x, slimePos.y, 0));
        SetButtonActive(slimeTilePos, isActive);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mouseOverTileLocation = tilemap.WorldToCell(new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0));

        bool isValidSelectionTile = movementOverlay.HasTile(mouseOverTileLocation);

        if(!currentSlime.IsMoving)
        {
            // this is just run every time the slime reaches its dest
            if(getMoves) {
                GetValidMoves();
                SetButtonActive(currentSlime.transform.position, true);
                getMoves = false;
            }

            if (mergeWith != null && !currentSlime.IsMoving)
            {
                currentSlime.Merge();
                Destroy(mergeWith);
                mergeWith = null;
                getMoves = true;
            }

            cursorOverlay.ClearAllTiles();
            if (isValidSelectionTile)
            {
                int highlightSize = performSplit ? currentSlime.Scale : currentSlime.Scale + 1;
                for(int h = 0; h < highlightSize; h++)
                {
                    for(int k = 0; k < highlightSize; k++)
                    {
                        if(mouseOverTilePositions.ContainsKey(mouseOverTileLocation))
                        {
                            Vector3Int moveTilePos = mouseOverTilePositions[mouseOverTileLocation];
                            cursorOverlay.SetTile(new Vector3Int(moveTilePos.x + h, moveTilePos.y + k, moveTilePos.z), validMoveTile);
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (currentSlimeObject != null && isValidSelectionTile)
                {
                    if(performSplit)
                    {
                        // split the slime to the selected location

                        //deactivate button slime was on
                        SetButtonActive(currentSlime.transform.position, false);

                        int newScale = currentSlime.Scale - 1;
                        currentSlime.SetScale(newScale);
                        currentSlimeObject.transform.position = tilemap.CellToWorld(splitTilePositions[mouseOverTileLocation]);
                        AddSlime(currentSlimeObject);


                        currentSlimeObject = Instantiate(slimePrefab, currentSlimeObject.transform.position, Quaternion.Euler(0, 0, 0));
                        currentSlime = currentSlimeObject.GetComponent<Slime>();
                        currentSlime.SetScale(newScale);
                        performSplit = false;

                        //activate button active slime is one
                        SetButtonActive(currentSlime.transform.position, true);

                        // move the slime to the split location
                        currentSlimeTileLocation = mouseOverTilePositions[mouseOverTileLocation];
                        currentSlime.SetDestination(tilemap.CellToWorld(currentSlimeTileLocation));
                    }
                    else
                    {
                        //deactivate button slime was on
                        SetButtonActive(currentSlime.transform.position, false);

                        // move slime to the selected location
                        MoveSelectedSlime(tilemap.CellToWorld(mouseOverTilePositions[mouseOverTileLocation]));
                    }

                    getMoves = true;
                }
                else if(slimeTiles.Contains(mouseOverTileLocation) && currentSlime.Scale > 0)
                {
                    // initiate split
                    performSplit = true;
                    getMoves = true;
                }
            }
        }
    }
}
