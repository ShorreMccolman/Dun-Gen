using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSPlayer : MonoBehaviour
{
    [SerializeField] float _walkSpeed;
    [SerializeField] float _runSpeed;
    [SerializeField] float _jumpSpeed;
    [SerializeField] float _gravity;
    [SerializeField] float _lookSpeed;
    [SerializeField] float _maxRotation;

    [SerializeField] Camera _camera;

    CharacterController _controller;
    Transform _transform;
    Vector3 _moveDirection;
    float _rotation;

    bool _canMove = true;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _transform = transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector3 forward = _transform.TransformDirection(Vector3.forward);
        Vector3 right = _transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float xSpeed = _canMove ? (isRunning ? _runSpeed : _walkSpeed) * Input.GetAxis("Vertical") : 0;
        float ySpeed = _canMove ? (isRunning ? _runSpeed : _walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float yDir = _moveDirection.y;

        _moveDirection = (forward * xSpeed) + (right * ySpeed);

        if (Input.GetButtonDown("Jump") && _canMove && _controller.isGrounded)
        {
            _moveDirection.y = _jumpSpeed;
        }
        else
        {
            _moveDirection.y = yDir;
        }

        if(!_controller.isGrounded)
        {
            _moveDirection.y -= _gravity * Time.deltaTime;
        }

        _controller.Move(_moveDirection * Time.deltaTime);

        if(_canMove)
        {
            _rotation += -Input.GetAxis("Mouse Y") * _lookSpeed;
            _rotation = Mathf.Clamp(_rotation, -_maxRotation, _maxRotation);
            _camera.transform.localRotation = Quaternion.Euler(_rotation, 0, 0);

            _transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * _lookSpeed, 0);
        }
    }
}