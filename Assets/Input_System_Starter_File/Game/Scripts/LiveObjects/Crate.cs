using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Windows;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        [SerializeField] private PlayerControls _input;
        private bool _isReadyToBreak = false;

        private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            _input = new PlayerControls();
            _input.Player.TapHold.Enable();
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            
            if (_isReadyToBreak == false && _brakeOff.Count >0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone            
            {
                _input.Player.TapHold.performed +=
                context =>
                {
                    if (context.interaction is HoldInteraction)
                    {
                        if (_brakeOff.Count > 0)
                        {
                            BreakPart();
                            BreakPart();

                            StartCoroutine(PunchDelay());
                        }
                        else if (_brakeOff.Count == 0)
                        {
                            _isReadyToBreak = false;
                            _crateCollider.enabled = false;
                            _interactableZone.CompleteTask(6);
                            Debug.Log("Completely Busted");
                        }

                    }
                    else
                    {
                        if (_brakeOff.Count > 0)
                        {
                            BreakPart();
                            StartCoroutine(PunchDelay());
                        }
                        else if (_brakeOff.Count == 0)
                        {
                            _isReadyToBreak = false;
                            _crateCollider.enabled = false;
                            _interactableZone.CompleteTask(6);
                            Debug.Log("Completely Busted");
                        }

                    }
                };

                _input.Player.TapHold.canceled +=
                    _ =>
                    {

                    };
            }
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);
            
        }



        public void BreakPart()
        {
            if( _brakeOff.Count == 0)
            {

                return;
            }
            int rng = Random.Range(0, _brakeOff.Count);
            _brakeOff[rng].constraints = RigidbodyConstraints.None;
            _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
            _brakeOff.Remove(_brakeOff[rng]);            
        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
        }
    }
}
