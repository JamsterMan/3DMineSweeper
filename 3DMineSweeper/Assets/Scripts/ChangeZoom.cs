using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeZoom : MonoBehaviour
{
    public Game game;
    public int expectedVal = 10;
    private readonly float defaultSize = 6f;
    public float offsetChange = 0.5f;
    public float sizeChange = 0.6f;

    public Vector3 defaultPosition;
    public Camera mainCamera;

    void Start()
    {
        ZoomCameraOnBoard();
    }

    public void ZoomCameraOnBoard()
    {
        float sizeVal;
        Vector3 offset;
        offset.x = 0f;//width
        offset.y = 0f;//hieght
        offset.z = 0f;

        offset.x = offsetChange * (game.width - expectedVal);//change x position of camera to fit the minefield
        offset.y = offsetChange * (game.height - expectedVal);//change y position of camera to fit the minefield
        transform.position = defaultPosition + offset;

        if (game.width >= game.height) {
            sizeVal = sizeChange * (game.width - expectedVal);
        } else {
            sizeVal = sizeChange * (game.height - expectedVal);
        }

        mainCamera.orthographicSize = defaultSize + sizeVal;
    }

}
