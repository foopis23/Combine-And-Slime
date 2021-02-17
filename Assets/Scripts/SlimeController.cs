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

    //internal properties
    private Camera mainCamera;
    private GameObject selected;
    private GameObject selectedShadow; //TODO: selectedShadow is going to be hacky but just fix it later?
    private Vector3 dragStartPos;
    private bool isDragging;
    private bool hasSpawnedNewGuyLmao;


    void Start()
    {
        mainCamera = Camera.main;
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
        SpriteRenderer spriteRenderer = selected.GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1.0f);
    }

    private void PlaceSelectedSlime(Vector3 mouseWorldPos)
    {
        PlaceSlime(mouseWorldPos, selected);
        if (selectedShadow != null)
            selectedShadow.GetComponent<SpriteRenderer>().enabled = false;
        selectedShadow = null;
    }

    private SpriteRenderer GetSlimeShadow(GameObject slime) {
        SpriteRenderer[] renderers = slime.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            if (!renderer.gameObject.Equals(selected))
            {
                return renderer;
            }
        }

        return null;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                hasSpawnedNewGuyLmao = false;
            }

            if (selected != null) {
                PlaceSelectedSlime(mouseWorldPos);
                selected = null;
            }else if (!isDragging) {
                selected = SelectSlime(mouseWorldPos);
                if (selected != null) {
                    SpriteRenderer renderer = GetSlimeShadow(selected);
                    selectedShadow = renderer.gameObject;
                    renderer.enabled = true;
                }
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

        if (isDragging && !hasSpawnedNewGuyLmao)
        {
            GameObject slime = SelectSlime(mainCamera.ScreenToWorldPoint(dragStartPos));
            if (slime != null)
            {
                slime.transform.localScale = new Vector3(1, 1, 1);
                selected = Instantiate(slimePrefab);
                SpriteRenderer renderer = GetSlimeShadow(selected);
                selectedShadow = renderer.gameObject;
                renderer.enabled = true;
                hasSpawnedNewGuyLmao = true;
            }
        }

        if (selected != null)
            selected.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y);

        if (selectedShadow != null)
        {
            Vector3 gridWorldPos = roundToGrid(mouseWorldPos);
            selectedShadow.transform.position = new Vector3(gridWorldPos.x, gridWorldPos.y, 0);
        }
    }
}
