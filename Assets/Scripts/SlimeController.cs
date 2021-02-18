using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SlimeController : MonoBehaviour
{
    //editor properties
    [SerializeField] private float dragDeadZone = 10.0f;
    [SerializeField] private LayerMask selectionLayer;
    [SerializeField] private Tilemap overlay;
    [SerializeField] private Tilemap overlay2;
    [SerializeField] private TileBase possibleMoveTile;
    [SerializeField] private TileBase validMoveTile;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private GameObject slimePrefab;

    //internal properties
    private Camera mainCamera;
    private Slime currentSlime;
    private Vector3Int currentSlimeTileLocation;
    private GameObject currentSlimeObject;
    private GameObject mergeWith;
    private Dictionary<Vector3Int, GameObject> slimes;
    private Vector3 dragStartPos;
    private HashSet<Vector3Int> validMoves;
    private Dictionary<Vector3Int, Vector3Int> mouseOverTilePositions;
    private bool isDragging;
    private bool hasSpawnedSplitFromDrag;


    void Start()
    {
        slimes = new Dictionary<Vector3Int, GameObject>();
        mainCamera = Camera.main;
        mergeWith = null;

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
        Debug.Log($"Add: {gridPos}");
        slimes.Add(gridPos, slime);
    }

    private GameObject GetSlime(Vector3 worldPos)
    {
        Vector3Int gridPos = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
        Debug.Log($"Get: {gridPos}");
        if (!slimes.ContainsKey(gridPos)) return null;

        return slimes[gridPos];
    }

    private bool RemoveSlime(Vector3 worldPos)
    {
        Vector3Int gridPos = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
        Debug.Log($"Add: {gridPos}");
        return slimes.Remove(gridPos);
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
        overlay.ClearAllTiles();
        validMoves = new HashSet<Vector3Int>();
        mouseOverTilePositions = new Dictionary<Vector3Int, Vector3Int>();
        int width = currentSlime.Scale + 1;

        Debug.Log(width);

        Vector3Int testPos;
        HashSet<Vector3Int> validTiles = new HashSet<Vector3Int>();

        // get the tiles taken up by the slime
        HashSet<Vector3Int> slimeTiles = new HashSet<Vector3Int>();
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
            testPos = new Vector3Int(currentSlimeTileLocation.x, currentSlimeTileLocation.y, currentSlimeTileLocation.z);
            HashSet<Vector3Int> possibleValidTiles;
            bool valid;
            do
            {
                testPos = testPos + direction;
                possibleValidTiles = new HashSet<Vector3Int>();
                valid = true;
                //check if every tile in the area covered by the slime is valid
                for(int h = 0; h < width; h++)
                {
                    for(int k = 0; k < width; k++)
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

                    }

                    // add the move to valid moves
                    validMoves.Add(testPos);

                    // add the tiles to valid tiles
                    validTiles.UnionWith(possibleValidTiles);
                }
            }
            while(valid);
        }

        foreach(Vector3Int tilePos in validTiles)
        {
            overlay.SetTile(tilePos, possibleMoveTile);
        }
    }

    private bool IsValidMove(Vector3Int tilePos) => validMoves.Contains(tilePos);

    private void MoveSelectedSlime(Vector3 worldPos)
    {
        GameObject temp = GetSlime(worldPos);

        if (temp != null)
        {
            Slime slime = temp.GetComponent<Slime>();

            if (currentSlime.CanMerge() && slime.Scale == currentSlime.Scale)
            {
                Vector3 pos = roundToGrid(worldPos);
                currentSlime.SetDestination(new Vector3(pos.x, pos.y, currentSlimeObject.transform.position.z));
                currentSlimeTileLocation = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
                GetValidMoves();
                mergeWith = temp;
                RemoveSlime(worldPos);
            }

        }
        else
        {
            Vector3 pos = roundToGrid(worldPos);
            currentSlime.SetDestination(new Vector3(pos.x, pos.y, currentSlimeObject.transform.position.z));
            currentSlimeTileLocation = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
            GetValidMoves();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mouseOverTileLocation = tilemap.WorldToCell(new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0));

        bool isValidSelectionTile = overlay.HasTile(mouseOverTileLocation);

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                if (hasSpawnedSplitFromDrag && isValidSelectionTile)
                {
                    MoveSelectedSlime(tilemap.CellToWorld(mouseOverTilePositions[mouseOverTileLocation]));
                }

                hasSpawnedSplitFromDrag = false;

            }
            else if (currentSlimeObject != null && isValidSelectionTile)
            {
                MoveSelectedSlime(tilemap.CellToWorld(mouseOverTilePositions[mouseOverTileLocation]));
            }

            isDragging = false;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            if (Vector3.Distance(dragStartPos, Input.mousePosition) > dragDeadZone)
            {
                isDragging = true;
            }
        }

        if (isDragging && !hasSpawnedSplitFromDrag)
        {
            GameObject slime = SelectSlime(mainCamera.ScreenToWorldPoint(dragStartPos));
            if (slime != null && slime.Equals(currentSlimeObject) && currentSlime.Split())
            {
                int newScale = currentSlime.Scale;
                AddSlime(currentSlimeObject);
                currentSlimeObject = Instantiate(slimePrefab, slime.transform.position, Quaternion.Euler(0, 0, 0));
                currentSlime = currentSlimeObject.GetComponent<Slime>();
                currentSlime.SetScale(newScale);
                hasSpawnedSplitFromDrag = true;
            }
        }

        if (mergeWith != null && !currentSlime.IsMoving)
        {
            currentSlime.Merge();
            Destroy(mergeWith);
            mergeWith = null;
        }

        overlay2.ClearAllTiles();
        if (isValidSelectionTile)
        {
            for(int h = 0; h < currentSlime.Scale + 1; h++)
            {
                for(int k = 0; k < currentSlime.Scale + 1; k++)
                {
                    Vector3Int moveTilePos = mouseOverTilePositions[mouseOverTileLocation];
                    overlay2.SetTile(new Vector3Int(moveTilePos.x + h, moveTilePos.y + k, moveTilePos.z), validMoveTile);
                }
            }
        }
    }
}
