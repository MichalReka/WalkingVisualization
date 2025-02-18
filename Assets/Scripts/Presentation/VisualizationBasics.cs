﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizationBasics : MonoBehaviour
{
    float moveSpeed = 5f;
    float oldMoveSpeed;
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    public static bool ifPaused { get; private set; }
    float rotationY = 0F;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        oldMoveSpeed = moveSpeed;
        ifPaused = false;

    }
    void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        ifPaused = true;
    }
    public static void ResumeGame()
    {
        ifPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
    }
    private void Update()
    {
        if (ifPaused == false)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                moveSpeed = oldMoveSpeed * 9f;
            }
            else
            {
                moveSpeed = oldMoveSpeed;
            }
            transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime);
            transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime);
            if (axes == RotationAxes.MouseXAndY)
            {
                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
            }
            else if (axes == RotationAxes.MouseX)
            {
                transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
            }
            else
            {
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Escape))
        {
            if (ifPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
}