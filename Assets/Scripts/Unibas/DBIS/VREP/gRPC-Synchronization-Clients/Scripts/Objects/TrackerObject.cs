using TreeEditor;
using UnityEditor.Build.Content;
using UnityEngine;

namespace Unibas.DBIS.VREP.Scripts.Objects
{
    public class TrackerObject
    {
        private Tracker _tracker;
        private bool isPresent;
        private bool isInstantiated;

        public TrackerObject()
        {
            this._tracker = new Tracker();
            this.isPresent = false;
            this.isInstantiated = false;
        }

        public TrackerObject(int objectId, Vector3 physicalPosition, Vector3 vrPosition, Quaternion rotation)
        {
            this._tracker = new Tracker()
            {
                Id = objectId,
                TrackerPhysicalPosition = SetVector(physicalPosition),
                TrackerVRPositon = SetVector(vrPosition),
                TrackerRotation = SetQuadrublet(rotation)

            };
            
            this.isPresent = false;
            this.isInstantiated = false;
        }

        public Tracker GetTracker()
        {
            return this._tracker;
        }

        public void SetTracker(Tracker tracker)
        {
            this._tracker = tracker;
        }


        public int GetObjectId()
        {
            return this._tracker.Id;
        }

        public void SetObjectId(int objectId)
        {
            this._tracker.Id= objectId;
        }

        public Vector3 GetPhysicalPosition()
        {
            Vector3 newVector = new Vector3
            {
                x = this._tracker.TrackerPhysicalPosition.X,
                y = this._tracker.TrackerPhysicalPosition.Y,
                z = this._tracker.TrackerPhysicalPosition.Z
            };


            return newVector;
        }

        public void SetPhysicalPosition(Vector3 physicalPosition)
        {
            this._tracker.TrackerPhysicalPosition = SetVector(physicalPosition);
        }

        public Vector3 GetVrPosition()
        {
            Vector3 newVector = new Vector3
            {
                x = this._tracker.TrackerVRPositon .X, 
                y = this._tracker.TrackerVRPositon .Y, 
                z = this._tracker.TrackerVRPositon .Z
            };


            return newVector;
        }

        public void SetVrPosition(Vector3 vrPosition)
        {
            this._tracker.TrackerVRPositon = SetVector(vrPosition);
        }

        public Quaternion GetRotation()
        {
            Quaternion newQuaternion = new Quaternion
            {
                x = this._tracker.TrackerRotation.X,
                y = this._tracker.TrackerRotation.Y,
                z = this._tracker.TrackerRotation.Z,
                w = this._tracker.TrackerRotation.W
            };


            return newQuaternion;
        }

        public void SetRotation(Quaternion rotation)
        {
            this._tracker.TrackerRotation = SetQuadrublet(rotation);
        }

        public bool TrackerIsPresent()
        {
            return this.isPresent;
        }

        public void SetTrackerIsPresent(bool isPresent)
        {
            this.isPresent = isPresent;
        }

        public bool TrackerIsInstantiated()
        {
            return this.isInstantiated;
        }

        public void TrackerIsInstantiated(bool isInstantiated)
        {
            this.isInstantiated = isInstantiated;
        }

        private Vector SetVector(Vector3 vector3)
        {
            Vector newVector = new Vector()
            {
                X = vector3.x,
                Y = vector3.y,
                Z = vector3.z
            };

            return newVector;
        }

        private Quadrublet SetQuadrublet(Quaternion quaternion)
        {
            Quadrublet newQuadrublet = new Quadrublet()
            {
                X = quaternion.x,
                Y = quaternion.y,
                Z = quaternion.z,
                W = quaternion.w
            };

            return newQuadrublet;
        }
        
    }
}