using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SlimeController : MonoBehaviour
{
    //editor properties
    [SerializeField] private float dragDeadZone = 10.0f;
    [SerializeField] private LayerMask selectionLayer;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private GameObject slimePrefab;
    [SerializeField] private GameObject tileSelectionCursor;

    //internal properties
    private Camera mainCamera;
    private Slime currentSlime;
    private Vector3Int currentSlimeTileLocation;
    private GameObject currentSlimeObject;
    private GameObject mergeWith;
    private Dictionary<Vector3Int, GameObject> slimes;
    private Vector3 dragStartPos;
    private bool isDragging;
    private bool hasSpawnedSplitFromDrag;
    private bool isValidTile;


    void Start()
    {
        slimes = new Dictionary<Vector3Int, GameObject>();
        mainCamera = Camera.main;
        mergeWith = null;
        isValidTile = false;

        if (currentSlime == null) 
        {
            currentSlimeObject = GameObject.FindGameObjectWithTag("slime");
            currentSlime = currentSlimeObject?.GetComponent<Slime>();
            currentSlimeTileLocation = tilemap.WorldToCell(new Vector3(currentSlimeObject.transform.position.x, currentSlimeObject.transform.position.y, 0));
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

    private void AddSlime(GameObject slime) {
        Vector3Int gridPos = tilemap.WorldToCell(new Vector3(slime.transform.position.x, slime.transform.position.y, 0));
        Debug.Log($"Add: {gridPos}");
        slimes.Add(gridPos, slime);
    }

    private GameObject GetSlime(Vector3 worldPos) {
        Vector3Int gridPos = tilemap.WorldToCell(new Vector3(worldPos.x,worldPos.y, 0));
        Debug.Log($"Get: {gridPos}");
        if (!slimes.ContainsKey(gridPos)) return null;

        return slimes[gridPos];
    }

    private bool RemoveSlime(Vector3 worldPos) {
        Vector3Int gridPos = tilemap.WorldToCell(new Vector3(worldPos.x,worldPos.y, 0));
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

    private void MoveSelectedSlime(Vector3 worldPos) {
        GameObject temp = GetSlime(worldPos);
        
        if (temp != null) {
            Slime slime = temp.GetComponent<Slime>();
            
            if (currentSlime.CanMerge() && slime.Scale == currentSlime.Scale) {
                currentSlime.SetDestination(roundToGrid(worldPos));
                currentSlimeTileLocation = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
                mergeWith = temp;
                RemoveSlime(worldPos);
            }

        }else{
            currentSlimeTileLocation = tilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));
            currentSlime.SetDestination(roundToGrid(worldPos));
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mapTileLocation = tilemap.WorldToCell(new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0));

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                if (hasSpawnedSplitFromDrag && isValidTile) {
                    MoveSelectedSlime(mouseWorldPos);
                }

                hasSpawnedSplitFromDrag = false;

            }else if (currentSlimeObject != null && isValidTile) {
                MoveSelectedSlime(mouseWorldPos);
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

        if (mergeWith != null && !currentSlime.IsMoving) {
            currentSlime.Merge();
            Destroy(mergeWith);
            mergeWith = null;
        }

        tileSelectionCursor.transform.position = roundToGrid(mouseWorldPos);
        
        isValidTile = (mapTileLocation.x == currentSlimeTileLocation.x || mapTileLocation.y == currentSlimeTileLocation.y) && tilemap.HasTile(mapTileLocation);
        if (isValidTile) {
            tileSelectionCursor.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 0.5f);
        }else{
           tileSelectionCursor.GetComponent<SpriteRenderer>().color = new Color(255, 0, 0, 0.5f); 
        }
    }
}
