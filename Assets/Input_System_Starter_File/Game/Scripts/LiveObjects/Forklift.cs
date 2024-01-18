using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.Windows;

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        [SerializeField]
        private GameObject _playerModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;
         private PlayerControls _input;
        [SerializeField] private GameObject _playerBox;

        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
            _input = new PlayerControls();
            _input.Forklift.Enable();
            _input.Forklift.Exit.performed += Exit_performed;
        }



        private void Exit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (_inDriveMode == true)
            {
                _playerModel.transform.position = _playerBox.transform.position;
                    ExitDriveMode();
            }
        }

    

   

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _playerModel.SetActive(false);
                _interactableZone.CompleteTask(5);
            }
        }

        private void ExitDriveMode()
        {
            _playerModel.SetActive(true);
            _inDriveMode = false;
            _forkliftCam.Priority = 9;            
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();
            
        }

        private void Update()
        {
            if (_inDriveMode == true)
            {
                CalcutateMovement();
                LiftControls();

            }

        }
      
        
       
        private void CalcutateMovement()
        {

            var move = _input.Forklift.movement.ReadValue<Vector2>();
            var direction = new Vector3(0, 0, move.y);
            var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            if (Mathf.Abs(move.y) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += move.x * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }

        private void LiftControls()
        {
           
                LiftUpRoutine();
            /*else if (Input.GetKey(KeyCode.T))
                LiftDownRoutine();*/
        }
        /*private void LiftControls()
       {
           var liftForks = _input.Forklift.LiftLow.ReadValue<float>();
           Vector3 yPos = _forks.transform.localPosition;
           yPos.y = Mathf.Clamp(yPos.y, _lowerLiftLimit, _raiseLiftLimit);
           _forks.transform.localPosition = yPos;
           _forks.transform.Translate((Vector3.up * Time.deltaTime * 3.0f * liftForks), transform.parent);
       }*/
        private void LiftUpRoutine()
        {

            var liftForks = _input.Forklift.LiftForks.ReadValue<float>();
            _lift.transform.Translate((Vector3.up * Time.deltaTime * 3.0f * liftForks), transform.parent);

            if (_lift.transform.localPosition.y < _liftUpperLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftUpperLimit;

            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }

        private void LiftDownRoutine()
        {

            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
        }

    }
}