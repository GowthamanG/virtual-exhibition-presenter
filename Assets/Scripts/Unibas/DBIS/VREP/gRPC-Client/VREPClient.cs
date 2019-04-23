using System;
using System.Threading;
using Grpc.Core;
using UnityEngine;
using UnityEngine.XR;

namespace Unibas.DBIS.VREP
{
	public class VREPClient : MonoBehaviour
	{
		//private AutoResetEvent resetEvent;
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
		private bool secondUserPresence;
		private bool secondUserInstantiated;
		private float translateX, translateY, translateZ;

		// Use this for initialization
		void Start ()
		{
			firstUserId = player1.GetInstanceID();
			/*firstUserPosition = player1.transform.position;
			firstUserRotation = player1.transform.rotation;*/
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
		
			//resetEvent = new AutoResetEvent(false);

			/*Instantiate(player2);

			secondUserId = player2.GetInstanceID();
			secondUserPosition = player2.transform.position;
			secondUserRotation = player2.transform.rotation;
			secondUserScale = player2.transform.lossyScale;
			
			secondUser = new User
			{
				Id = secondUserId,
			
				UserPosition = new Vector()
				{
					X = secondUserPosition.x,
					Y = secondUserPosition.y,
					Z = secondUserPosition.z,
				},
			
				UserRotation = new Quadrublet()
				{
					X = secondUserRotation.x,
					Y = secondUserRotation.y,
					Z = secondUserRotation.z,
					W = secondUserRotation.w
				},
			
				UserScale = new Vector()
				{
					X = secondUserScale.x,
					Y = secondUserScale.y,
					Z = secondUserScale.z,
				}
			};*/
			
			secondUser = new User();
			secondUserId = 0;
			secondUserPosition = new Vector3();
			secondUserRotation = new Quaternion();
			secondUserScale = new Vector3();

			secondUserPresence = false;
			secondUserInstantiated = false;
			
			cylinder = new GameObject();
			
			connectionThread = new Thread(Run);
			connectionThread.Start();

			translateX = player1.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x;
			translateY = player1.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y;
			translateZ = player1.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z;


			//client.setUser(user);
			//SetUser(user);


			/*task = SetUser();
		task.Wait();*/

			//Vector3 positionSecondUser = new Vector3(secondUser.PlayerPosition.X, secondUser.PlayerPosition.Y, secondUser.PlayerPosition.Z);

			//Instantiate(secondUserObject, positionSecondUser, Quaternion.Euler(0,0,0));

			//channel.ShutdownAsync().Wait();

		}
	
		// Update is called once per frame
		void Update ()
		{
			/*UpdateUser(user, player1);
		//updateUser(secondUser, secondUserObject);
		//task.Wait();
		//client.setUser(user);
		client.setUser(user);*/
			//resetEvent.Set();
	
			/*firstUserPosition = player1.transform.position;
			firstUserRotation = player1.transform.rotation;
			firstUserScale = player1.transform.lossyScale;*/

			firstUserPosition = InputTracking.GetLocalPosition(XRNode.Head);
			firstUserRotation = InputTracking.GetLocalRotation(XRNode.Head);
			firstUserScale = player1.transform.lossyScale;
			
			UpdateUser(firstUser, firstUserId, firstUserPosition, firstUserRotation, firstUserScale);
			//UpdateUser(secondUser, secondUserId, secondUserPosition, secondUserRotation, secondUserScale);

			if (secondUserPresence && secondUserInstantiated == false)
			{
				secondUserInstantiated = true;
				cylinder = Instantiate(player2, secondUserPosition, secondUserRotation);
			}

			//player2.transform.position = secondUserPosition;
			//player2.transform.rotation = secondUserRotation;
			if (secondUserPresence)
				cylinder.transform.SetPositionAndRotation(secondUserPosition, secondUserRotation);
			
		}



		//This method is to update the partners position in the same exhibition
//	
//	
//	public async Task SetUser()
//	{
//		try
//		{
//			using (var call = client.setUser())
//			{
//				var responseReaderTask = Task.Run(async () =>
//				{
//					while (await call.ResponseStream.MoveNext())
//					{
//						var note = call.ResponseStream.Current;
//						Debug.Log("Received: " + note.ToString());
//					}
//				});
//
//				await call.RequestStream.WriteAsync(user);
//				await call.RequestStream.CompleteAsync();
//				await responseReaderTask;
//
//			}
//		}
//		catch (RpcException e)
//		{
//			Debug.Log("RPC failed" + e);
//			throw;
//		}
//	}
//
//	
//	public async Task GetUser(float deltaTime = 1.0f)
//	{
//		try
//		{
//			using (var call = client.getUser())
//			{
//				var responseReaderTask = Task.Run(async () =>
//				{
//					while (await call.ResponseStream.MoveNext())
//					{
//						var note = call.ResponseStream.Current;
//						Debug.Log("Second user received: " + note);
//						secondUser = note;
//						//deltaTime used to update values per second instead per frame
//						/*secondUser.PlayerPosition.X *= deltaTime;
//						secondUser.PlayerPosition.Y *= deltaTime;
//						secondUser.PlayerPosition.Z *= deltaTime;*/
//					}
//				});
//
//				RequestUser requestUser = new RequestUser {RequestUserID = user.Id};
//
//				await call.RequestStream.WriteAsync(requestUser);
//				await call.RequestStream.CompleteAsync();
//				await responseReaderTask;
//
//			}
//		}
//		catch (RpcException e)
//		{
//			Debug.Log("RPC failed" + e);
//			throw;
//		}
//	}

		private void Run()
		{
		
			channel = new Channel(host, port, ChannelCredentials.Insecure);
			client = new multiUserSync.multiUserSyncClient(channel);
			DateTime time = DateTime.Now;
		
			while (!stop || channel.State != ChannelState.Shutdown) //The synchronization happens in the while loop
			{
				//resetEvent.WaitOne();
				var now = DateTime.Now;
				var deltaTime = now - time;
				time = now;
				SetUser(firstUser);
				GetUser(firstUserId); //Trick to get user which is NOT equal firstUserId, details see implementation on server
				//deltaTime.TotalSeconds;
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
					secondUserPresence = true;
				
				secondUserId = responseUser.Id;
				secondUserPosition.x = responseUser.UserPosition.X + translateX;
				secondUserPosition.y = responseUser.UserPosition.Y;
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

		/*private void UpdateFirstUser()
		{
			firstUser.Id = firstUserId;
			firstUser.UserPosition.X = firstUserPosition.x;
			firstUser.UserPosition.Y = firstUserPosition.y;
			firstUser.UserPosition.Z = firstUserPosition.z;
			firstUser.UserRotation.X = firstUserRotation.x;
			firstUser.UserRotation.Y = firstUserRotation.y;
			firstUser.UserRotation.Z = firstUserRotation.z;
			firstUser.UserRotation.W = firstUserRotation.w;
			firstUser.UserScale.X = firstUserScale.x;
			firstUser.UserScale.Y = firstUserScale.y;
			firstUser.UserScale.Z = firstUserScale.z;
		}
		
		private void UpdateSecondUser()
		{
			secondUser.Id = secondUserId;
			secondUser.UserPosition.X = secondUserPosition.x;
			secondUser.UserPosition.Y = secondUserPosition.y;
			secondUser.UserPosition.Z = secondUserPosition.z;
			secondUser.UserRotation.X = secondUserRotation.x;
			secondUser.UserRotation.Y = secondUserRotation.y;
			secondUser.UserRotation.Z = secondUserRotation.z;
			secondUser.UserRotation.W = secondUserRotation.w;
			secondUser.UserScale.X = secondUserScale.x;
			secondUser.UserScale.Y = secondUserScale.y;
			secondUser.UserScale.Z = secondUserScale.z;
		}*/
	}
}