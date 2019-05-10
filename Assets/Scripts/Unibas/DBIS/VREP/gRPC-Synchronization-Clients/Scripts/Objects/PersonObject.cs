using TreeEditor;
using UnityEditor.Build.Content;
using UnityEngine;

namespace Unibas.DBIS.VREP.Scripts.Objects
{
    public class PersonObject
    {
        private User _user;
        private bool isPresent;
        private bool isInstantiated;

        public PersonObject()
        {
            this._user = new User();
            this.isPresent = false;
            this.isInstantiated = false;
        }

        public PersonObject(int objectId, Vector3 physicalPosition, Vector3 vrPosition, Quaternion rotation)
        {
            this._user = new User()
            {
                Id = objectId,
                UserPhysicalPosition = SetVector(physicalPosition),
                UserVRPosition = SetVector(vrPosition),
                UserRotation = SetQuadrublet(rotation)
            };
       
            this.isPresent = false;
            this.isInstantiated = false;
        }

        public User GetPerson()
        {
            return this._user;
        }

        public void SetPerson(User user)
        {
            this._user = user;
        }


        public int GetObjectId()
        {
            return this._user.Id;
        }

        public void SetObjectId(int objectId)
        {
            this._user.Id= objectId;
        }

        public Vector3 GetPhysicalPosition()
        {
            Vector3 newVector = new Vector3
            {
                x = this._user.UserPhysicalPosition.X,
                y = this._user.UserPhysicalPosition.Y,
                z = this._user.UserPhysicalPosition.Z
            };


            return newVector;
        }

        public void SetPhysicalPosition(Vector3 physicalPosition)
        {
            this._user.UserPhysicalPosition = SetVector(physicalPosition);
        }

        public Vector3 GetVrPosition()
        {
            Vector3 newVector = new Vector3
            {
                x = this._user.UserVRPosition.X, 
                y = this._user.UserVRPosition.Y, 
                z = this._user.UserVRPosition.Z
            };


            return newVector;
        }

        public void SetVrPosition(Vector3 vrPosition)
        {
            this._user.UserVRPosition = SetVector(vrPosition);
        }

        public Quaternion GetRotation()
        {
            Quaternion newQuaternion = new Quaternion
            {
                x = this._user.UserRotation.X,
                y = this._user.UserRotation.Y,
                z = this._user.UserRotation.Z,
                w = this._user.UserRotation.W
            };


            return newQuaternion;
        }

        public void SetRotation(Quaternion rotation)
        {
            this._user.UserRotation = SetQuadrublet(rotation);
        }

        public bool PersonIsPresent()
        {
            return this.isPresent;
        }

        public void SetPersonIsPresent(bool isPresent)
        {
            this.isPresent = isPresent;
        }

        public bool PersonIsInstantiated()
        {
            return this.isInstantiated;
        }

        public void SetPersonIsInstantiated(bool isInstantiated)
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