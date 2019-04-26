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
		private GameObject cylinder;
		private multiUserSync.multiUserSyncClient client;
		private User firstUser;
		private User secondUser;
		private Channel channel;
		private int firstUserId;
		private Vector3 firstUserPosition;
		private Quaternion firstUserRotation;
		private Vector3 firstUserScale;
		private int secondUserId;
		private Vector3 secondUserPosition;
		private Quaternion secondUserRotation;
		private Vector3 secondUserScale;
		private bool stop;
		private bool secondUserIsPresent;
		private bool secondUserIsInstantiated;
		private float translateX, translateY, translateZ;

		// Use this for initialization
		void Start ()
		{
			firstUserId = player1.GetInstanceID();
			firstUserPosition = InputTracking.GetLocalPosition(XRNode.Head);
			firstUserRotation = InputTracking.GetLocalRotation(XRNode.Head);
			firstUserScale = player1.transform.lossyScale;

			firstUser = new User
			{
				Id = firstUserId,
			
				UserPosition = new Vector()
				{
					X = firstUserPosition.x,
					Y = firstUserPosition.y,
					Z = firstUserPosition.z,
				},
			
				UserRotation = new Quadrublet()
				{
					X = firstUserRotation.x,
					Y = firstUserRotation.y,
					Z = firstUserRotation.z,
					W = firstUserRotation.w
				},
			
				UserScale = new Vector()
				{
					X = firstUserScale.x,
					Y = firstUserScale.y,
					Z = firstUserScale.z,
				}
			};
			
			secondUser = new User();
			secondUserId = 0;
			secondUserPosition = new Vector3();
			secondUserRotation = new Quaternion();
			secondUserScale = new Vector3();

			secondUserIsPresent = false;
			secondUserIsInstantiated = false;
			
			cylinder = new GameObject();
			
			connectionThread = new Thread(Run);
			connectionThread.Start();

			translateX = player1.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x;
			translateY = player1.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y;
			translateZ = player1.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z;
		}
	
		void Update ()
		{

			firstUserPosition = InputTracking.GetLocalPosition(XRNode.Head);
			firstUserRotation = InputTracking.GetLocalRotation(XRNode.Head);
			firstUserScale = player1.transform.lossyScale;
			
			UpdateUser(firstUser, firstUserId, firstUserPosition, firstUserRotation, firstUserScale);

			if (secondUserIsPresent && secondUserIsInstantiated == false)
			{
				secondUserIsInstantiated = true;
				cylinder = Instantiate(player2, secondUserPosition, secondUserRotation);
			} 

			if (secondUserIsPresent && secondUserIsInstantiated)
				cylinder.transform.SetPositionAndRotation(secondUserPosition, secondUserRotation);
			
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
				secondUserPosition.x = responseUser.UserPosition.X + translateX;
				secondUserPosition.y = 0.0f;
				secondUserPosition.z = responseUser.UserPosition.Z + translateZ;
				secondUserRotation.x = responseUser.UserRotation.X;
				secondUserRotation.y = responseUser.UserRotation.Y;
				secondUserRotation.z = responseUser.UserRotation.Z;
				secondUserRotation.w = responseUser.UserRotation.W;
				secondUserScale.x = responseUser.UserScale.X;
				secondUserScale.y = responseUser.UserScale.Y;
				secondUserScale.z = responseUser.UserScale.Z;
				
			}
			catch (RpcException e)
			{
				Debug.Log("RPC failed in method \"getUser\" " + e);
			}
		}


		private void UpdateUser(User user, int userId, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			user.Id = userId;
			user.UserPosition.X = position.x;
			user.UserPosition.Y = position.y;
			user.UserPosition.Z = position.z;
			user.UserRotation.X = rotation.x;
			user.UserRotation.Y = rotation.y;
			user.UserRotation.Z = rotation.z;
			user.UserRotation.W = rotation.w;
			user.UserScale.X = scale.x;
			user.UserScale.Y = scale.y;
			user.UserScale.Z = scale.z;
		}

	}
}