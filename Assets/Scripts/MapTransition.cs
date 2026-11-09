using System;
using System.Collections;
using System.Threading;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Embree;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapTransition : MonoBehaviour {
    [SerializeField] PolygonCollider2D mapBoundry;
    CinemachineConfiner2D confiner;
    [SerializeField] float moveAmount = 2f;
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportTargetPosition;

    enum Direction { Up, Down, Left, Right, Teleport }
    private void Awake() {
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            confiner.BoundingShape2D = mapBoundry;
            UpdatePlayerPosition(collision.gameObject);

            MapController_man.Instance?.HighlightArea(mapBoundry.name);
        }
    }
    private void UpdatePlayerPosition(GameObject player) {
        if (direction == Direction.Teleport) {
            player.transform.position = teleportTargetPosition.position;
            return;
        }

        Vector3 newPos = player.transform.position;

        switch (direction) {
            case Direction.Up:
                newPos.y += moveAmount;
                break;
            case Direction.Down:
                newPos.y -= moveAmount;
                break;
            case Direction.Left:
                newPos.x -= moveAmount;
                break;
            case Direction.Right:
                newPos.x += moveAmount;
                break;
        }
        player.transform.position = newPos;
    }
}
