using System;
using System.Threading;
using Grpc.Core;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace Unibas.DBIS.VREP
{
	public class VREPClient : MonoBehaviour
	{
		private Thread connectionThread;
		public string host;
		public int port;
		public GameObject player1;
		public GameObject player2;
		private GameObject cowboy;
		private multiUserSync.multiUserSyncClient client;
		private User firstUser;
		private User secondUser;
		private Channel channel;
		private int firstUserId;
		private Vector3 firstUserPhysicalPosition;
		private Quaternion firstUserPhysicalRotation;
		private Vector3 firstUserVRPosition;
		private Quaternion firstUserVRRotation;
		private int secondUserId;
		private Vector3 secondUserOriginVRPosition;
		private Quaternion secondUserOriginVRRotation;
		private Vector3 secondUserVRPosition;
		private Quaternion secondUserVRRotation;
		private Vector3 secondUserPhysicalPosition;
		private Quaternion secondUserPhysicalRotation;
		private bool stop;
		private bool secondUserIsPresent;
		private bool secondUserIsInstantiated;
		private Vector3 translation;
		private bool playersHaveTeleportedInOtherVR;
		private bool playersHaveTeleportedHere;
		private Vector3 distanceTeleport;
		private Vector3 distanceTeleportHere;

		// Use this for initialization
		void Start ()
		{
			firstUserId = player1.GetInstanceID();
			firstUserPhysicalPosition = InputTracking.GetLocalPosition(XRNode.Head);
			firstUserPhysicalRotation = InputTracking.GetLocalRotation(XRNode.Head);
			firstUserVRPosition = player1.transform.position;
			firstUserVRRotation = player1.transform.rotation;


			firstUser = new User
			{
				Id = firstUserId,
				
				UserPhysicalPosition = new Vector()
				{
					X = firstUserPhysicalPosition.x,
					Y = firstUserPhysicalPosition.y,
					Z = firstUserPhysicalPosition.z,
				},
			
				UserPhysicalRotation = new Quadrublet()
				{
					X = firstUserPhysicalRotation.x,
					Y = firstUserPhysicalRotation.y,
					Z = firstUserPhysicalRotation.z,
					W = firstUserPhysicalRotation.w
				},
				
				UserVRPosition = new Vector()
				{
					X = firstUserVRPosition.x,
					Y = firstUserVRPosition.y,
					Z = firstUserVRPosition.z,
				},
			
				UserVRRotation = new Quadrublet()
				{
					X = firstUserVRRotation.x,
					Y = firstUserVRRotation.y,
					Z = firstUserVRRotation.z,
					W = firstUserVRRotation.w
				}
				
			};
			
			secondUser = new User();
			secondUserId = 0;
			secondUserOriginVRPosition = new Vector3();
			secondUserOriginVRRotation = new Quaternion();
			secondUserVRPosition = new Vector3();
			secondUserVRRotation = new Quaternion();

			secondUserIsPresent = false;
			secondUserIsInstantiated = false;

			playersHaveTeleportedInOtherVR = false;
			playersHaveTeleportedHere = false;
			
			cowboy = new GameObject();
			
			connectionThread = new Thread(Run);
			connectionThread.Start();

			translation = new Vector3();
			translation.x = player1.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x;
			translation.y = player1.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y;
			translation.z = player1.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z;

			distanceTeleport = new Vector3();
			distanceTeleportHere = new Vector3();
		}
	
		void Update ()
		{
			
			firstUserPhysicalPosition = InputTracking.GetLocalPosition(XRNode.Head);
			firstUserPhysicalRotation = InputTracking.GetLocalRotation(XRNode.Head);
			firstUserVRPosition = player1.transform.position;
			firstUserVRRotation = player1.transform.rotation;

			Vector3 tempTranslate = translation;
			
			translation.x = player1.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x;
			translation.y = player1.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y;
			translation.z = player1.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z;

			if (translation != tempTranslate)
				playersHaveTeleportedHere = true;
			
			
			UpdateUser(firstUser, firstUserId, firstUserVRPosition, firstUserVRRotation, firstUserPhysicalPosition, firstUserPhysicalRotation);

			if (secondUserIsPresent && secondUserIsInstantiated == false)
			{
				secondUserIsInstantiated = true;
				cowboy = Instantiate(player2, secondUserVRPosition, secondUserVRRotation);
			} 

			if (secondUserIsPresent && secondUserIsInstantiated)
				cowboy.transform.SetPositionAndRotation(secondUserVRPosition, secondUserVRRotation);

			if (playersHaveTeleportedInOtherVR)
			{
				secondUserVRPosition.x += distanceTeleport.x;
				secondUserVRPosition.y += distanceTeleport.y;
				secondUserVRPosition.z += distanceTeleport.z;
				
				Vector3 newPosFirstUser = new Vector3()
				{
					x = firstUserVRPosition.x + distanceTeleport.x,
					y = firstUserVRPosition.y + distanceTeleport.y,
					z = firstUserVRPosition.z + distanceTeleport.z
				};

		
				cowboy.transform.SetPositionAndRotation(secondUserVRPosition, secondUserVRRotation);
				GameObject.Find("VRCamera").transform.SetPositionAndRotation(newPosFirstUser, firstUserVRRotation);

				playersHaveTeleportedInOtherVR = false;
			}
			
			translation.x = player1.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x;
			translation.y = player1.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y;
			translation.z = player1.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z;
			
		}


		private void Run()
		{
		
			channel = new Channel(host, port, ChannelCredentials.Insecure);
			client = new multiUserSync.multiUserSyncClient(channel);
			DateTime time = DateTime.Now;
		
			while (!stop || channel.State != ChannelState.Shutdown) //The synchronization happens in the while loop
			{
				var now = DateTime.Now;
				var deltaTime = now - time;
				time = now;
				SetUser(firstUser);
				GetUser(firstUserId); //Trick to get user which is NOT equal firstUserId, details see implementation on server

				if (playersHaveTeleportedHere)
				{
					SetDistanceTeleport(distanceTeleportHere);
					playersHaveTeleportedHere = false;
				}
				
				GetDistanceTeleport();
				
			}

			channel.ShutdownAsync().Wait();
		}

		private void OnApplicationQuit()
		{
			stop = true;
			connectionThread.Abort();
		}

		private void OnDestroy()
		{
			stop = true;
			connectionThread.Abort();
		}


		private void SetUser(User user)
		{
			try
			{
				Confirmation serverResponse = client.setUser(user);
				Debug.Log(serverResponse);		
			}
			catch (RpcException e)
			{
				Debug.Log("RPC failed in method \"setUser\"" + e);
			}
		
		}

		private void GetUser(int userId)
		{
			try
			{
				RequestUser requestUser = new RequestUser()
				{
					RequestUserID = userId
				};
				
				var responseUser = client.getUser(requestUser);

				if (responseUser.Id != 0)
					secondUserIsPresent = true;
				
				secondUserId = responseUser.Id;
				secondUserVRPosition.x = responseUser.UserPhysicalPosition.X + translation.x;
				secondUserVRPosition.y = 0.0f;
				secondUserVRPosition.z = responseUser.UserPhysicalPosition.Z + translation.z;
				
				secondUserVRRotation.x = responseUser.UserPhysicalRotation.X;
				secondUserVRRotation.y = responseUser.UserPhysicalRotation.Y;
				secondUserVRRotation.z = responseUser.UserPhysicalRotation.Z;
				secondUserVRRotation.w = responseUser.UserPhysicalRotation.W;
				
				secondUserOriginVRPosition.x = responseUser.UserVRPosition.X;
				secondUserOriginVRPosition.y = responseUser.UserVRPosition.Y;
				secondUserOriginVRPosition.z = responseUser.UserVRPosition.Z;

				secondUserOriginVRRotation.x = responseUser.UserVRRotation.X;
				secondUserOriginVRRotation.y = responseUser.UserVRRotation.Y;
				secondUserOriginVRRotation.z = responseUser.UserVRRotation.Z;
				secondUserOriginVRRotation.w = responseUser.UserVRRotation.W;
					
			}
			catch (RpcException e)
			{
				Debug.Log("RPC failed in method \"getUser\" " + e);
			}
		}

		private void GetDistanceTeleport()
		{
			try
			{
				RequestUser requestUser = new RequestUser()
				{
					RequestUserID = 0
				};

				var responseDistance = client.getDistanceTeleport(requestUser);

				if (responseDistance != null)
				{
					distanceTeleport.x = responseDistance.X;
					distanceTeleport.y = responseDistance.Y;
					distanceTeleport.z = responseDistance.Z;

					playersHaveTeleportedInOtherVR = true;
				}
				
			}
			catch (RpcException e)
			{
				Debug.Log("RPC failed in method \"getDistanceTeleport\" " + e);
			}
		}

		private void SetDistanceTeleport(Vector3 distanceTeleport)
		{
			try
			{
				Vector distanceTeleportHere = new Vector()
				{
					X = distanceTeleport.x,
					Y = distanceTeleport.y,
					Z = distanceTeleport.z
				};
					
				Confirmation responseServer = client.setDistanceTeleport(distanceTeleportHere);
				Debug.Log(responseServer);
			}
			catch (RpcException e)
			{
				Debug.Log("RPC failed in method \"setDistanceTeleport\" " + e);
			}
			
		}


		private void UpdateUser(User user, int userId, Vector3 worldPosition, Quaternion worldRotation, Vector3 physicalPosition, Quaternion physicalRotation)
		{
			user.Id = userId;
			user.UserVRPosition.X = worldPosition.x;
			user.UserVRPosition.Y = worldPosition.y;
			user.UserVRPosition.Z = worldPosition.z;
			user.UserVRRotation.X = worldRotation.x;
			user.UserVRRotation.Y = worldRotation.y;
			user.UserVRRotation.Z = worldRotation.z;
			user.UserVRRotation.W = worldRotation.w;
			
			user.UserPhysicalPosition.X = physicalPosition.x;
			user.UserPhysicalPosition.Y = physicalPosition.y;
			user.UserPhysicalPosition.Z = physicalPosition.z;
			user.UserPhysicalRotation.X = physicalRotation.x;
			user.UserPhysicalRotation.Y = physicalRotation.y;
			user.UserPhysicalRotation.Z = physicalRotation.z;
			user.UserPhysicalRotation.W = physicalRotation.w;
			
			
		}

		private bool checkSecondUserHasTeleported(Vector3 secondUserPosition, Vector3 secondUserOriginPosition, 
			Vector3 differencePosToOrigin)
		{
			var diffPosTempX = Math.Abs(secondUserPosition.x - secondUserOriginPosition.x);
			var diffPosTempY = Math.Abs(secondUserPosition.y - secondUserOriginPosition.y);
			var diffPosTempZ = Math.Abs(secondUserPosition.z - secondUserOriginPosition.z);

			if (differencePosToOrigin.x != diffPosTempX || differencePosToOrigin.y != diffPosTempY ||
			    differencePosToOrigin.z != diffPosTempZ)
				return true;
			
			return false;
		}

	}
}