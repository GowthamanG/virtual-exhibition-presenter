﻿using System;
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
		private GameObject avatarSecondPlayer;
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
		private bool stop;
		private bool secondUserIsPresent;
		private bool secondUserIsInstantiated;
		private Vector3 v1;

		private Vector3 v2;
		private bool v2IsSet;
		private bool playerHasTeleported;
		private Vector3 distanceTeleporting;

		// Use this for initialization
		void Start ()
		{
			firstUserId = player1.GetInstanceID();
			firstUserPosition = InputTracking.GetLocalPosition(XRNode.Head);
			firstUserRotation = InputTracking.GetLocalRotation(XRNode.Head);

			firstUser = new User
			{
				Id = firstUserId,
			
				UserPhysicalPosition = new Vector()
				{
					X = firstUserPosition.x,
					Y = firstUserPosition.y,
					Z = firstUserPosition.z,
				},
				
				UserVRPosition = new Vector()
				{
					X = player1.transform.position.x,
					Y = player1.transform.position.y,
					Z = player1.transform.position.z
				},
			
				UserRotation = new Quadrublet()
				{
					X = firstUserRotation.x,
					Y = firstUserRotation.y,
					Z = firstUserRotation.z,
					W = firstUserRotation.w
				}
			};
			
			secondUser = new User();
			secondUserId = 0;
			secondUserPosition = new Vector3();
			secondUserRotation = new Quaternion();

			secondUserIsPresent = false;
			secondUserIsInstantiated = false;
			
			avatarSecondPlayer = new GameObject();
			
			connectionThread = new Thread(Run);
			connectionThread.Start();

			v1 = new Vector3()
			{
				x = player1.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x,
				y = player1.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y,
				z = player1.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z
			};
			
			v2 = new Vector3(0.0f, 0.0f, 0.0f);
			v2IsSet = false;
			playerHasTeleported = false;
			distanceTeleporting = new Vector3();
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
				avatarSecondPlayer = Instantiate(player2, secondUserPosition, new Quaternion(0, 0, 0, 0));
			} 

			if (secondUserIsPresent && secondUserIsInstantiated)
				avatarSecondPlayer.transform.SetPositionAndRotation(secondUserPosition, new Quaternion(0, 0, 0, 0));

			if (playerHasTeleported)
			{
				
				Vector3 newPosFirstUser = new Vector3()
				{
					x = firstUserPosition.x + distanceTeleporting.x,
					y = firstUserPosition.y + distanceTeleporting.y,
					z = firstUserPosition.z + distanceTeleporting.z

				};
			
				Vector3 newPosSecondUser = new Vector3()
				{
					x = secondUserPosition.x + distanceTeleporting.x,
					y = secondUserPosition.y + distanceTeleporting.y,
					z = secondUserPosition.z + distanceTeleporting.z

				};
				
				avatarSecondPlayer.transform.SetPositionAndRotation(newPosSecondUser, secondUserRotation);

				GameObject.Find("VRCamera").transform.position = newPosFirstUser;
			}
			
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
				secondUserPosition.x = responseUser.UserPhysicalPosition.X + v1.x;
				secondUserPosition.y = 0.0f;
				secondUserPosition.z = responseUser.UserPhysicalPosition.Z + v1.z;
				secondUserRotation.x = responseUser.UserRotation.X;
				secondUserRotation.y = responseUser.UserRotation.Y;
				secondUserRotation.z = responseUser.UserRotation.Z;
				secondUserRotation.w = responseUser.UserRotation.W;

				Vector3 tempV2 = v2;
				v2.x = responseUser.UserVRPosition.X - secondUserPosition.x;
				v2.y = responseUser.UserVRPosition.Y - secondUserPosition.y;
				v2.z = responseUser.UserVRPosition.Z - secondUserPosition.z;

				if (tempV2 != v2)
				{
					playerHasTeleported = true;
					distanceTeleporting.x = tempV2.x - v2.x;
					distanceTeleporting.y = tempV2.y - v2.y;
					distanceTeleporting.z = tempV2.y - v2.z;
				}

			}
			catch (RpcException e)
			{
				Debug.Log("RPC failed in method \"getUser\" " + e);
			}
		}


		private void UpdateUser(User user, int userId, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			user.Id = userId;
			user.UserPhysicalPosition.X = position.x;
			user.UserPhysicalPosition.Y = position.y;
			user.UserPhysicalPosition.Z = position.z;
			user.UserRotation.X = rotation.x;
			user.UserRotation.Y = rotation.y;
			user.UserRotation.Z = rotation.z;
			user.UserRotation.W = rotation.w;
		}

	}
}