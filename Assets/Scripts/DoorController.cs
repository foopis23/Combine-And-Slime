using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

// for storing door asc in editor
[Serializable]
public struct SerializedDoorData
{
    public TileBase[] doorTile;
}

// storing runtime door data
public struct DoorData
{
    public Vector3Int location;
    public TileBase activated;
    public TileBase deactivated;
    public TileBase groundTile;
}

// for storing data about doors asc to specific button type
public class ButtonDoorData
{
    public List<DoorData> doors;
    public int activation;

    public ButtonDoorData()
    {
        doors = new List<DoorData>();
        activation = 0;
    }

    public void Activate(Tilemap doorMap, Tilemap groundMap)
    {
        ++activation;

        if (activation == 1)
        {
            foreach (DoorData door in doors)
            {
                doorMap.SetTile(door.location, door.activated);
                groundMap.SetTile(new Vector3Int(door.location.x, door.location.y, 0), door.groundTile);
            }
        }
    }

    public void Deactivate(Tilemap doorMap, Tilemap groundMap)
    {
        --activation;
        if (activation == 0)
        {
            foreach (DoorData door in doors)
            {
                doorMap.SetTile(door.location, door.deactivated);
                groundMap.SetTile(new Vector3Int(door.location.x, door.location.y, 0), null);
            }
        }
        activation = Mathf.Max(activation, 0);
    }
}

public class DoorController : MonoBehaviour
{
    //editor properties
    [SerializeField] private Tilemap doorMap;
    [SerializeField] private Tilemap groundMap;
    [SerializeField] private SerializedDoorData[] doorAssociationData;

    //internal properties
    private Dictionary<ButtonType, ButtonDoorData> DoorLookUp;

    private void Start()
    {
        DoorLookUp = new Dictionary<ButtonType, ButtonDoorData>();
        GetAllDoors();

        EventSystem.Current.RegisterEventListener<ActivateButtonContext>(OnActivateButton);
        EventSystem.Current.RegisterEventListener<DeactivateButtonContext>(OnDeactiveButton);
        
    }

    private ButtonType LookUpAssociation(TileBase door) {
        for(int i=0; i<doorAssociationData.Length; i++) {
            foreach(TileBase tile in doorAssociationData[i].doorTile) {
                if (tile.Equals(door)) return (ButtonType)i;
            }
        }
        
        throw new Exception();
    }

    private void GetAllDoors()
    {
        // THIS CODE IS KIND OF SHIT, BUT I AM NEW TO TILE MAPS AND I COULDN'T FIGURE OUT A BETTER WAY
        foreach (var pos in doorMap.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            if (doorMap.HasTile(localPlace))
            {
                TileBase doorTile = doorMap.GetTile(localPlace);
                ButtonType buttonType;

                try{
                    buttonType = LookUpAssociation(doorTile);
                }catch(Exception e) {
                    continue;
                }

                if (!DoorLookUp.ContainsKey(buttonType))
                {
                    DoorLookUp[buttonType] = new ButtonDoorData();
                }

                DoorData doorData = new DoorData();
                doorData.activated = null;
                doorData.deactivated = doorTile;
                doorData.location = localPlace;
                Vector3Int groundTileLoc = new Vector3Int(localPlace.x, localPlace.y, 0);
                doorData.groundTile = groundMap.GetTile(groundTileLoc);
                groundMap.SetTile(groundTileLoc, null);
                DoorLookUp[buttonType].doors.Add(doorData);
            }
        }
    }

    public void OnActivateButton(ActivateButtonContext ctx)
    {
        if (!DoorLookUp.ContainsKey(ctx.ButtonType)) return;

        DoorLookUp[ctx.ButtonType].Activate(doorMap, groundMap);
    }

    public void OnDeactiveButton(DeactivateButtonContext ctx)
    {
        if (!DoorLookUp.ContainsKey(ctx.ButtonType)) return;

        DoorLookUp[ctx.ButtonType].Deactivate(doorMap, groundMap);
    }
}
