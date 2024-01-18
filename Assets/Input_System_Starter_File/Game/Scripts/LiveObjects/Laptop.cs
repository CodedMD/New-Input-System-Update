using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using TMPro;
using System.Security.Policy;
using static UnityEditor.Experimental.GraphView.GraphView;
using Game.Scripts.Player;

namespace Game.Scripts.LiveObjects
{
    public class Laptop : MonoBehaviour
    {
        [SerializeField]
        private Slider _progressBar;
        [SerializeField]
        private int _hackTime = 5;
        private bool _hacked = false;
        [SerializeField]
        private CinemachineVirtualCamera[] _cameras;
        private int _activeCamera = 0;
        [SerializeField]
        private InteractableZone _interactableZone;
        private PlayerControls _input;
        [SerializeField] private PlayerMovement _player;

        public static event Action onHackComplete;
        public static event Action onHackEnded;

        private void OnEnable()
        {
            InteractableZone.onHoldStarted += InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded += InteractableZone_onHoldEnded;
            _input = new PlayerControls();
            _input.Player.Enable();
            _input.Player.InteractionZone.performed += InteractionZone_started;
            _input.Player.Exit.performed += Exit_performed;
        }

        private void Exit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            

            if (_hacked == true)
            {
                

                _hacked = false;
                onHackEnded?.Invoke();
                ResetCameras();
            }
        }

        private void InteractionZone_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            if (_hacked == true)
            {
                var previous = _activeCamera;
                _activeCamera++;


                if (_activeCamera >= _cameras.Length)
                    _activeCamera = 0;


                _cameras[_activeCamera].Priority = 11;
                _cameras[previous].Priority = 9;
            }
        }

        private void Update()
        {
            
           
        }

        void ResetCameras()
        {
            foreach (var cam in _cameras)
            {
                cam.Priority = 9;
            }
            _player.enabled = true;
        }

        private void InteractableZone_onHoldStarted(int zoneID)
        {
            if (zoneID == 3 && _hacked == false) //Hacking terminal
            {
                _progressBar.gameObject.SetActive(true);
                StartCoroutine(HackingRoutine());
                onHackComplete?.Invoke();
            }
        }

        private void InteractableZone_onHoldEnded(int zoneID)
        {
            if (zoneID == 3) //Hacking terminal
            {
                if (_hacked == true)
                    return;

                StopAllCoroutines();
                _progressBar.gameObject.SetActive(false);
                _progressBar.value = 0;
                onHackEnded?.Invoke();
            }
        }

        
        IEnumerator HackingRoutine()
        {
            while (_progressBar.value < 1)
            {
                _progressBar.value += Time.deltaTime / _hackTime;
                yield return new WaitForEndOfFrame();
            }

            //successfully hacked
            _hacked = true;
            _interactableZone.CompleteTask(3);

            //hide progress bar
            _progressBar.gameObject.SetActive(false);

            //enable Vcam1
            _cameras[0].Priority = 11;
        }
        
        private void OnDisable()
        {
            InteractableZone.onHoldStarted -= InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded -= InteractableZone_onHoldEnded;
        }
    }

}

