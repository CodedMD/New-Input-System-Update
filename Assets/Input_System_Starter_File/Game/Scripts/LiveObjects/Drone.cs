using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using UnityEngine.Windows;
using UnityEngine.Animations;
using Unity.Mathematics;
using Game.Scripts.Player;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        [SerializeField] private PlayerControls _input;
        [SerializeField] private PlayerMovement _player;

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;

            _input = new PlayerControls();
            _input.Drone.Enable();
            _input.Player.Disable();
            _input.Drone.Exit.performed += Exit_performed;

        }

        private void Exit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (_inFlightMode)
            {
                _inFlightMode = false;
                _player.enabled = true;

                onExitFlightmode?.Invoke();
                ExitFlightMode();
            }
        }
           

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _player.enabled = true;

                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);            
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                _player.enabled = false;

                CalculateTilt();
                CalculateMovementUpdate();

                
            }
        }

        private void FixedUpdate()
        {


            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            
           


            var moveRotate = _input.Drone.Rotation.ReadValue<float>();
            var tempRot = transform.localRotation.eulerAngles;
            tempRot.y -= _speed  * moveRotate;
            transform.localRotation = Quaternion.Euler(tempRot);
          
        }

        private void CalculateMovementFixedUpdate()
        {
            var moveUp = _input.Drone.ThrustUpDown.ReadValue<float>();
            _rigidbody.AddForce(transform.up * moveUp * _speed, ForceMode.Acceleration);

            
        }

        private void CalculateTilt()
        {
           var move = _input.Drone.Movement.ReadValue<Vector2>();
            var velocity = transform.forward;
            var moveRotate = new Vector3(0, 0,move.y);
            transform.Translate(  moveRotate* Time.deltaTime );
            transform.Rotate(0, move.x, 0);
           

            //controls the tilt
            if (move.x < 0) 
                   transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 30);
               else if (move.x > 0)
                   transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
               else if (move.y > 0)
                   transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
               else if (move.y < 0)
                   transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
               else
                 transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 0);

        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }
    }
}
