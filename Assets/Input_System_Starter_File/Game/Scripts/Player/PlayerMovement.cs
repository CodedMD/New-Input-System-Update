using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.LiveObjects;
using Cinemachine;
using UnityEngine.Windows;

namespace Game.Scripts.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        private CharacterController _controller;
        private Animator _anim;
        [SerializeField]
        private float _speed = 5.0f;
        private bool _playerGrounded;
        [SerializeField]
        private Detonator _detonator;
        private bool _canMove = true;
        [SerializeField]
        private CinemachineVirtualCamera _followCam;
        [SerializeField]
        private GameObject _model;
        [SerializeField]
        private PlayerControls _input;


        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete += ReleasePlayerControl;
            Laptop.onHackEnded += ReturnPlayerControl;
            Forklift.onDriveModeEntered += ReleasePlayerControl;
            Forklift.onDriveModeExited += ReturnPlayerControl;
            Forklift.onDriveModeEntered += HidePlayer;
            Drone.OnEnterFlightMode += ReleasePlayerControl;
            Drone.onExitFlightmode += ReturnPlayerControl;
            _input = new PlayerControls();
            _input.Player.Enable();
            _input.Player.movement.started += Move_started;
            _input.Player.movement.canceled += Move_canceled;
        }

   

        private void Move_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _anim.SetFloat("Speed", 0);

        }

        private void Move_started(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if(_canMove == true)
            {
                var velocity = transform.forward;
                _anim.SetFloat("Speed", Mathf.Abs(velocity.magnitude));
                _playerGrounded = _controller.isGrounded;

                if (_playerGrounded)
                {
                    velocity.y = 0f;
                }
                if (!_playerGrounded)
                {
                    velocity.y += -20f * Time.deltaTime;
                }

                _controller.Move(velocity * Time.deltaTime);
            }
           
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();

            if (_controller == null)
                Debug.LogError("No Character Controller Present");

            _anim = GetComponentInChildren<Animator>();

            if (_anim == null)
                Debug.Log("Failed to connect the Animator");
        }

        private void Update()
        {
            if (_canMove == true)
                CalcutateMovement();

        }

        private void CalcutateMovement()
        {
            _playerGrounded = _controller.isGrounded;
            var move = _input.Player.movement.ReadValue<Vector2>();
            var velocity = transform.forward;
            var moveRotate = new Vector3(0, 0, move.y);
            transform.Translate(moveRotate * Time.deltaTime * _speed);
            transform.Rotate(0, move.x, 0);                

        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            switch(zone.GetZoneID())
            {
                case 1: //place c4
                    _detonator.Show();
                    break;
                case 2: //Trigger Explosion
                    TriggerExplosive();
                    break;
            }
        }

        private void ReleasePlayerControl()
        {
            
            _canMove = false;
            _followCam.Priority = 9;
        }

        private void ReturnPlayerControl()
        {
            _model.SetActive(true);
            _canMove = true;
            _followCam.Priority = 10;
        }

        private void HidePlayer()
        {
            _model.SetActive(false);
        }
               
        private void TriggerExplosive()
        {
            _detonator.TriggerExplosion();
        }

        private void OnDisable()
        {

            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete -= ReleasePlayerControl;
            Laptop.onHackEnded -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= ReleasePlayerControl;
            Forklift.onDriveModeExited -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= HidePlayer;
            Drone.OnEnterFlightMode -= ReleasePlayerControl;
            Drone.onExitFlightmode -= ReturnPlayerControl;
        }

    }
}

