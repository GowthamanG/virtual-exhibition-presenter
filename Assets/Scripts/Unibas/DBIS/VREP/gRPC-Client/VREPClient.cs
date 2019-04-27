using System;
using System.Threading;
using Grpc.Core;
using UnityEngine;
using UnityEngine.XR;

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
		private Vector3 firstUserVRWorldPosition;
		private Quaternion firstUserVRWorldRotation;
		private int secondUserId;
		private Vector3 secondUserOriginVRPosition;
		private Quaternion secondUserOriginVRRotation;
		private Vector3 secondUserVRWorldPosition;
		private Quaternion secondUserVRWorldRotation;
		private bool stop;
		private bool secondUserIsPresent;
		private bool secondUserIsInstantiated;
		private float translateX, translateY, translateZ;

		// Use this for initialization
		void Start ()
		{
			firstUserId = player1.GetInstanceID();
			firstUserPhysicalPosition = InputTracking.GetLocalPosition(XRNode.Head);
			firstUserPhysicalRotation = InputTracking.GetLocalRotation(XRNode.Head);
			firstUserVRWorldPosition = player1.transform.position;
			firstUserVRWorldRotation = player1.transform.rotation;

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
				
				UserVRWorldPosition = new Vector()
				{
					X = firstUserVRWorldPosition.x,
					Y = firstUserVRWorldPosition.y,
					Z = firstUserVRWorldPosition.z,
				},
			
				UserVRWorldRotation = new Quadrublet()
				{
					X = firstUserVRWorldRotation.x,
					Y = firstUserVRWorldRotation.y,
					Z = firstUserVRWorldRotation.z,
					W = firstUserVRWorldRotation.w
				},
			
				
			};
			
			secondUser = new User();
			secondUserId = 0;
			secondUserOriginVRPosition = new Vector3();
			secondUserOriginVRRotation = new Quaternion();
			secondUserVRWorldPosition = new Vector3();
			secondUserVRWorldRotation = new Quaternion();

			secondUserIsPresent = false;
			secondUserIsInstantiated = false;
			
			cowboy = new GameObject();
			
			connectionThread = new Thread(Run);
			connectionThread.Start();

			translateX = player1.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x;
			translateY = player1.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y;
			translateZ = player1.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z;
		}
	
		void Update ()
		{
			

			firstUserPhysicalPosition = InputTracking.GetLocalPosition(XRNode.Head);
			firstUserPhysicalRotation = InputTracking.GetLocalRotation(XRNode.Head);
			firstUserVRWorldPosition = player1.transform.position;
			firstUserVRWorldRotation = player1.transform.rotation;
			
			translateX = player1.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x;
			translateY = player1.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y;
			translateZ = player1.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z;
			
			
			UpdateUser(firstUser, firstUserId, firstUserVRWorldPosition, firstUserVRWorldRotation, firstUserPhysicalPosition, firstUserPhysicalRotation);

			if (secondUserIsPresent && secondUserIsInstantiated == false)
			{
				secondUserIsInstantiated = true;
				cowboy = Instantiate(player2, secondUserVRWorldPosition, secondUserVRWorldRotation);
			} 

			if (secondUserIsPresent && secondUserIsInstantiated)
				cowboy.transform.SetPositionAndRotation(secondUserVRWorldPosition, secondUserVRWorldRotation);		
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
				Response serverResponse = client.setUser(user);
				Debug.Log("User is set: " + serverResponse.Response_);

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
				secondUserVRWorldPosition.x = responseUser.UserPhysicalPosition.X + translateX;
				secondUserVRWorldPosition.y = 0.0f;
				secondUserVRWorldPosition.z = responseUser.UserPhysicalPosition.Z + translateZ;
				
				secondUserVRWorldRotation.x = responseUser.UserPhysicalRotation.X;
				secondUserVRWorldRotation.y = responseUser.UserPhysicalRotation.Y;
				secondUserVRWorldRotation.z = responseUser.UserPhysicalRotation.Z;
				secondUserVRWorldRotation.w = responseUser.UserPhysicalRotation.W;
				
				secondUserOriginVRPosition.x = responseUser.UserVRWorldPosition.X;
				secondUserOriginVRPosition.y = responseUser.UserVRWorldPosition.Y;
				secondUserOriginVRPosition.z = responseUser.UserVRWorldPosition.Z;

				secondUserOriginVRRotation.x = responseUser.UserVRWorldRotation.X;
				secondUserOriginVRRotation.y = responseUser.UserVRWorldRotation.Y;
				secondUserOriginVRRotation.z = responseUser.UserVRWorldRotation.Z;
				secondUserOriginVRRotation.w = responseUser.UserVRWorldRotation.W;
			}
			catch (RpcException e)
			{
				Debug.Log("RPC failed in method \"getUser\" " + e);
			}
		}


		private void UpdateUser(User user, int userId, Vector3 worldPosition, Quaternion worldRotation, Vector3 physicalPosition, Quaternion physicalRotation)
		{
			user.Id = userId;
			user.UserVRWorldPosition.X = worldPosition.x;
			user.UserVRWorldPosition.Y = worldPosition.y;
			user.UserVRWorldPosition.Z = worldPosition.z;
			user.UserVRWorldRotation.X = worldRotation.x;
			user.UserVRWorldRotation.Y = worldRotation.y;
			user.UserVRWorldRotation.Z = worldRotation.z;
			user.UserVRWorldRotation.W = worldRotation.w;
			
			user.UserPhysicalPosition.X = physicalPosition.x;
			user.UserPhysicalPosition.Y = physicalPosition.y;
			user.UserPhysicalPosition.Z = physicalPosition.z;
			user.UserPhysicalRotation.X = physicalRotation.x;
			user.UserPhysicalRotation.Y = physicalRotation.y;
			user.UserPhysicalRotation.Z = physicalRotation.z;
			user.UserPhysicalRotation.W = physicalRotation.w;
			
			
		}

	}
}