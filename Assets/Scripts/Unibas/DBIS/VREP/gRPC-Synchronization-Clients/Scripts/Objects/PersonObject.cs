using TreeEditor;
using UnityEditor.Build.Content;
using UnityEngine;

namespace Unibas.DBIS.VREP.Scripts.Objects
{
    public class PersonObject
    {
        private User user;
        private bool isPresent;
        private bool isInstantiated;

        public PersonObject()
        {
            this.user = new User();
            this.isPresent = false;
            this.isInstantiated = false;
        }

        public PersonObject(int objectId, Vector3 physicalPosition, Vector3 vrPosition, Quaternion rotation)
        {
            this.user.Id = objectId;
            this.user.UserPhysicalPosition = SetVector(physicalPosition);
            this.user.UserVRPosition = SetVector(vrPosition);
            this.user.UserRotation = SetQuadrublet(rotation);
            this.isPresent = false;
            this.isInstantiated = false;
        }

        public User GetPerson()
        {
            return this.user;
        }

        public void SetPerson(User user)
        {
            this.user = user;
        }


        public int GetObjectId()
        {
            return this.user.Id;
        }

        public void SetObjectId(int objectId)
        {
            this.user.Id= objectId;
        }

        public Vector3 GetPhysicalPosition()
        {
            Vector3 newVector = new Vector3
            {
                x = this.user.UserPhysicalPosition.X,
                y = this.user.UserPhysicalPosition.Y,
                z = this.user.UserPhysicalPosition.Z
            };


            return newVector;
        }

        public void SetPhysicalPosition(Vector3 physicalPosition)
        {
            this.user.UserPhysicalPosition = SetVector(physicalPosition);
        }

        public Vector3 GetVrPosition()
        {
            Vector3 newVector = new Vector3
            {
                x = this.user.UserVRPosition.X, 
                y = this.user.UserVRPosition.Y, 
                z = this.user.UserVRPosition.Z
            };


            return newVector;
        }

        public void SetVrPosition(Vector3 vrPosition)
        {
            this.user.UserVRPosition = SetVector(vrPosition);
        }

        public Quaternion GetRotation()
        {
            Quaternion newQuaternion = new Quaternion
            {
                x = this.user.UserRotation.X,
                y = this.user.UserRotation.Y,
                z = this.user.UserRotation.Z,
                w = this.user.UserRotation.W
            };


            return newQuaternion;
        }

        public void SetRotation(Quaternion rotation)
        {
            this.user.UserRotation = SetQuadrublet(rotation);
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