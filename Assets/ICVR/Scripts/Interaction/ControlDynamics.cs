/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace ICVR
{

    [Serializable]
    public class ControlEvent
    {
        public string state;
        public Collider sensor;
        public UnityEvent controlEffect;
    }


    /// <summary>
    /// This component is used to control objects with user interaction. An attached collider, when contacting 
    /// any of the control event sensors, will emit the relevant control effect. 
    /// e.g. for user interfaces, switches or other jointed objects. For more information 
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Interaction/ControlDynamics.md"/>
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ControlDynamics : MonoBehaviour
    {
        public string State { get; private set; }

        [SerializeField] private Transform resetReference;
        [SerializeField] private bool stickySensors;
        
        [SerializeField] private List<ControlEvent> controlEvents;

        private List<string> controls = new List<string>();
        private List<Transform> contacts = new List<Transform>();

        public delegate void ControlEventTrigger(ControlEvent cEvent);
        public event ControlEventTrigger onControlAction;

        private void Awake()
        {
            InitialiseControlEvents();
            onControlAction += _onControlAction;
        }

        private void OnDestroy()
        {
            onControlAction -= _onControlAction;
        }

        private void InitialiseControlEvents()
        {
            controls.Clear();

            // checking event integrity and isTrigger state
            foreach (ControlEvent controlEvent in controlEvents)
            {
                controlEvent.sensor.isTrigger = true;

                if (controlEvent.controlEffect != null)
                {
                    controls.Add(controlEvent.sensor.name);
                }
                else
                {
                    Debug.LogWarning("Control '" + gameObject.name + "-" + controlEvent.state + "' " +
                        "has no effects assigned to it. It won't do anything.");
                }
            }
        }

        public int AddControlEvent(ControlEvent newEvent)
        {
            if (!controls.Contains(newEvent.state))
            {
                controls.Add(newEvent.state);
                controlEvents.Add(newEvent);
            }
            return controls.Count;
        }

        private void OnTriggerEnter(Collider other)
        {
            contacts.Add(other.transform);
            if (controls.Contains(other.gameObject.name))
            {
                ProcessControlEvent(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            contacts.Remove(other.transform);
        }

        public void StickToTarget(Transform target)
        {
            if (contacts.Count >= 1 && contacts.Contains(target))
            {
                resetReference = target;
            }
        }


        #region Controller interface

        public void StartInteraction(GameObject target)
        {
            // nothing yet
        }

        public void FinishInteraction()
        {
            if (gameObject.TryGetComponent(out ObjectInterface objectInterface))
            {
                objectInterface.ToggleActivation();
            }

            ResetPose();
        }

        public void ResetPose()
        {
            StartCoroutine(ResetGripOrigin());
        }

        private void ProcessControlEvent(GameObject go)
        {
            if (stickySensors)
            {
                resetReference = go.transform;
            }

            int controlIndex = controls.IndexOf(go.name);
            ControlEvent cEvent = controlEvents[controlIndex];
            if (cEvent.state != State)
            {
                State = cEvent.state;
                onControlAction?.Invoke(cEvent);
            }

        }

        private IEnumerator ResetGripOrigin()
        {
            yield return new WaitForSeconds(0.2f);
            transform.position = resetReference.position;
            transform.rotation = resetReference.rotation;
        }

        private void _onControlAction(ControlEvent cEvent)
        {
            // actions to perform when any control event is fired
            //...
            //DebugControlEvent(cEvent);

            // finally, fire specific control effect for this state transition
            cEvent.controlEffect?.Invoke();
        }

        private void DebugControlEvent(ControlEvent c)
        {
            Debug.Log(gameObject.name + " triggered a control event:\n " +
                "State=" + c.state + ", " +
                "Sensor=" + c.sensor.name + ", " +
                "Effect=" + c.controlEffect.GetPersistentMethodName(0));
        }

        #endregion Controller interface

    }

}
