/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using UnityEngine;

namespace ICVR
{

    public class ThrowData
    {
        public SVector3 LinearForce;
        public SVector3 AngularForce;
    }

    /// <summary>
    /// Adding this component to an object extends the physics system, allowing it to be thrown with a force 
    /// based on the hand velocity, as well as the weight of the object. Setting a new object density will 
    /// automatically update the object's mass and, when the mesh is readable, will calculate its volume. 
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Interaction/RigidDynamics.md"/>
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RigidDynamics : MonoBehaviour
    {
        [Tooltip("Units: g/cm3 [0.001 - 10.000] \nAssumes isentropy\nWater = 1.0, Air = 0.00123m Iron:7.874")]
        [SerializeField] private float Density = 1.0f;

        public float Mass
        {
            get { return GetNewMass(); }
            set { _mass = value; }
        }

        public float Volume
        {
            get { return GetVolume(); }
            set { _volume = value; }
        }

        public ThrowData Throw
        {
            get { return GetForcesOnRigidBody(); }
            set { _throw = value; }
        }

        public bool UsesGravity
        {
            get { return _usesGravity; }
            private set { _usesGravity = value; }
        }

        // properties
        private ThrowData _throw;
        private float _mass;
        private float _volume;
        private bool _usesGravity;

        // calculation vars
        private Vector3 locNow;
        private Vector3 locThn;

        private Quaternion rotNow;
        private Quaternion rotThn;

        private int trjWinSize = 12;
        private int rotWinSize = 8;

        private Vector3[] lastVels;
        private Vector3[] lastAngVels;
        private float[] degs;

        private int currVelEntry = 0;
        private int currRotEntry = 0;

        private bool isReadable = false;
        private Rigidbody myRB;

        private void OnEnable()
        {
            myRB = GetComponent<Rigidbody>();
            _usesGravity = myRB.useGravity;
        }

        void Start()
        {
            ResetComponent();

            if (TryGetComponent(out Rigidbody rb))
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

                _volume = GetVolume();
                _mass = GetNewMass();
                _usesGravity = rb.useGravity;
            }

            //Debug.Log(gameObject.name + "\nisReadable:" + isReadable + ";\tVolume:" + (_volume*1000000) + "cm^3;\tMass:" + (_mass*1000) + "g");
        }


        private void ResetComponent()
        {
            currVelEntry = 0;
            currRotEntry = 0;

            lastVels = new Vector3[trjWinSize];

            degs = new float[rotWinSize];
            lastAngVels = new Vector3[rotWinSize];

            locNow = gameObject.transform.position;
            locThn = locNow;

            rotNow = transform.rotation;
            rotThn = rotNow;
        }

        private float GetVolume()
        {
            MeshFilter myMesh;

            if (GetComponent<MeshFilter>())
            {
                myMesh = GetComponent<MeshFilter>();

            }
            else if (GetComponentInChildren<MeshFilter>())
            {
                myMesh = GetComponentInChildren<MeshFilter>().gameObject.GetComponent<MeshFilter>();
            }
            else
            {
                myMesh = null;
            }

            if (myMesh != null && myMesh.mesh.isReadable)
            {
                isReadable = myMesh.mesh.isReadable;
                Mesh mesh = GetMesh();
                VolumeSolver vs = new VolumeSolver(gameObject.transform.lossyScale);
                return (float)vs.GetMeshVolume(mesh);
            }
            else
            {
                return GetComponent<Rigidbody>().mass / (Density * 1000f);
            }
        }

        private Mesh GetMesh()
        {
            MeshFilter foundMesh;
            TryGetComponent(out foundMesh);
            if (foundMesh == null)
            {
                for (int t = 0; t < transform.childCount; t++)
                {
                    transform.GetChild(t).TryGetComponent(out foundMesh);
                }
            }

            if (foundMesh != null)
            {
                return foundMesh.sharedMesh;
            }
            else
            {
                Debug.Log("no mesh found");
                return null;
            }
        }

        private float GetNewMass()
        {
            float myMass = ((Density * 1000f) * (_volume));
            GetComponent<Rigidbody>().mass = myMass;
            if (myMass == 0f) myMass = 1f;
            return (float)myMass;
        }

        private void FixedUpdate()
        {
            if (myRB.IsSleeping()) return;

            locNow = transform.position;
            Vector3 trajectory = locNow - locThn;
            rotNow = transform.rotation;
            Quaternion angTraj = rotNow * Quaternion.Inverse(rotThn);

            lastVels[currVelEntry] = trajectory / Time.fixedDeltaTime;
            currVelEntry++;

            if (currVelEntry >= trjWinSize) currVelEntry = 0;
            angTraj.ToAngleAxis(out degs[currRotEntry], out lastAngVels[currRotEntry]);
            currRotEntry++;

            if (currRotEntry >= rotWinSize) currRotEntry = 0;
            locThn = locNow;
            rotThn = rotNow;
        }

        private Vector3 GetCurrentVelocity()
        {
            Vector3 sumVs = new Vector3(0f, 0f, 0f);
            foreach (Vector3 v in lastVels.ToArray())
            {
                sumVs.x += v.x;
                sumVs.y += v.y;
                sumVs.z += v.z;
            }
            return sumVs / (float)trjWinSize;
        }

        private Vector3 GetNormalisedAngVel(Vector3[] angleArr)
        {
            Vector3 sumTs = Vector3.zero;
            foreach (Vector3 v in angleArr.ToArray())
            {
                sumTs.x += v.x;
                sumTs.y += v.y;
                sumTs.z += v.z;
            }
            return (sumTs / (float)rotWinSize).normalized;
        }

        private Vector3 ToAngularVelocity(Quaternion q)
        {
            if (Math.Abs(q.w) > 1023.5f / 1024.0f)
                return new Vector3();
            var angle = Math.Acos(Math.Abs(q.w));
            var gain = Math.Sign(q.w) * 2.0f * angle / Math.Sin(angle);
            return new Vector3((float)(q.x * gain), (float)(q.y * gain), (float)(q.z * gain));
        }

        private ThrowData GetForcesOnRigidBody()
        {
            _throw = new ThrowData();

            // translational forces
            Vector3 currVel = GetCurrentVelocity();
            Vector3 forceDir = currVel.normalized;
            float vSq = new Vector3(currVel.x * currVel.x, currVel.y * currVel.y, currVel.z * currVel.z).magnitude;
            Vector3 force = 0.5f * _mass * (vSq * forceDir);
            _throw.LinearForce = force;

            // rotational forces
            Vector3 torqueAxis = GetNormalisedAngVel(lastAngVels);
            float torqueMag = degs.Sum() / rotWinSize;
            Quaternion targetRot = Quaternion.AngleAxis(torqueMag, torqueAxis);
            Vector3 torque = ToAngularVelocity(targetRot);
            _throw.AngularForce = torque;

            return _throw;
        }
    }
}