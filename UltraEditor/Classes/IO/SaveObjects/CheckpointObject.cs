namespace UltraEditor.Classes.IO.SaveObjects;

using System.Collections;
using System.Collections.Generic;
using UltraEditor.Classes.Editor;
using UltraEditor.Libraries;
using UnityEngine;

public class CheckpointObject : SavableObject
{
    public List<string> rooms = new List<string>();
    public List<string> roomsToInherit = new List<string>();
    public GameObject[] checkpointRooms = [];
    public List<GameObject> checkpointRoomsToInherit = [];

    public static CheckpointObject Create(GameObject target)
    {
        CheckpointObject checkpointObject = target.AddComponent<CheckpointObject>();
        if (checkpointObject.GetComponent<Collider>() != null)
            checkpointObject.GetComponent<Collider>().isTrigger = true;
        return checkpointObject;
    }

    public void addRoomId(string id)
    {
        rooms.Add(id);
    }

    public void addRoomToInheritId(string id)
    {
        roomsToInherit.Add(id);
    }

    public override void Create()
    {
        checkpointRooms = LoadingHelper.GetObjectsWithIds(rooms);
        checkpointRoomsToInherit = LoadingHelper.GetObjectsWithIdsList(roomsToInherit);

        StartCoroutine(waitTillPlayer());
    }

    IEnumerator waitTillPlayer()
    {
        while (!NewMovement.Instance.gameObject.activeInHierarchy)
        {
            yield return new WaitForEndOfFrame();
        }
        GameObject so = Instantiate(AddressablesHelper.Ass<GameObject>("Assets/Prefabs/Levels/Checkpoint.prefab"), transform);
        so.transform.localPosition = Vector3.zero;
        so.transform.localEulerAngles = Vector3.zero;
        so.transform.localScale = Vector3.one;

        CheckPoint checkpoint = so.GetComponent<CheckPoint>();
        checkpoint.rooms = checkpointRooms;
        checkpoint.roomsToInherit = checkpointRoomsToInherit;
    }
}
