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
    [SerializeField] bool ignoreCollisions = false;
    [SerializeField] float ignoreTimer = 0.5f;
    [SerializeField] TilemapCollider2D tilemapColLayer;

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
        player.transform.position = newPos;

        BoxCollider2D playerCollider = player.GetComponent<BoxCollider2D>();

        StartCoroutine(IgnorePlayerCollisionTime(playerCollider, tilemapColLayer));
    }
    private IEnumerator IgnorePlayerCollisionTime(BoxCollider2D playerCollider, TilemapCollider2D tileCollider) {
        if (ignoreCollisions) {
            Debug.Log("ignoring collisions");
            Physics2D.IgnoreCollision(playerCollider, tileCollider, true);

            Debug.Log("waiting for " + ignoreTimer + " second(s)");
            yield return new WaitForSeconds(ignoreTimer);

            Debug.Log("enabling collisions");
            Physics2D.IgnoreCollision(playerCollider, tileCollider, false);
        }
    }
}
