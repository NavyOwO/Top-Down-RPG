using System.Threading;
using Unity.Cinemachine;
using UnityEditor.Embree;
using UnityEngine;

public class MapTransition : MonoBehaviour {
    [SerializeField] PolygonCollider2D mapBoundry;
    CinemachineConfiner2D confiner;
    [SerializeField] float moveAmount = 2f;
    [SerializeField] Direction direction;
    //[SerializeField] float timer = 0.5f;

    enum Direction { Up, Down, Left, Right }
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

        /* 
        BoxCollider2D playerCollider = player.GetComponent<BoxCollider2D>(); 
        while (timer > Time.deltaTime) { 
            playerCollider.enabled = false;
            timer += Time.deltaTime;
        }
        playerCollider.enabled = true;
        */ //this code is evil and breaks stuff -- find a way to tp and disable/ignore collisions for x amt time

        player.transform.position = newPos;
    }
}
