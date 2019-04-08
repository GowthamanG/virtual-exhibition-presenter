using System;
using System.Threading;
using Grpc.Core;
using UnityEngine;

namespace Unibas.DBIS.VREP
{
	public class VREPClient : MonoBehaviour
	{
		private AutoResetEvent resetEvent;
		private Thread thread;
		public string host;
		public int port;
		public GameObject player1;
		public GameObject secondUserObject;
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
	

		// Use this for initialization
		void Start ()
		{
			firstUserId = player1.GetInstanceID();
			firstUserPosition = player1.transform.position;
			firstUserRotation = player1.transform.rotation;
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


			secondUser = null;
		
			resetEvent = new AutoResetEvent(false);
		
			thread = new Thread(Run);
			thread.Start();


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
			resetEvent.Set();
	
			firstUserPosition = player1.transform.position;
			firstUserRotation = player1.transform.rotation;
			firstUserScale = player1.transform.lossyScale;
		
			UpdateUser(firstUser, firstUserId, firstUserPosition, firstUserRotation, firstUserScale);

			if (secondUser != null)
			{
				UpdateUser(secondUser, secondUserId, secondUserPosition, secondUserRotation, secondUserScale);
				secondUserObject.transform.position = secondUserPosition;
				secondUserObject.transform.rotation = secondUserRotation;
			}

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
		
			while (!stop || channel.State == ChannelState.Shutdown) //The synchronization happens in the while loop
			{
				resetEvent.WaitOne();
				var now = DateTime.Now;
				var deltaTime = now - time;
				time = now;
				SetUser(firstUser);
				GetUser(firstUserId); //Trick to get user which is NOT equal firstUserId, details see implementation on server
				//deltaTime.TotalSeconds;
				if (secondUser == null && secondUserPresence)
					Instantiate(secondUserObject, secondUserPosition, secondUserRotation);
			}

			channel.ShutdownAsync().Wait();
		}

		private void OnApplicationQuit()
		{
			stop = true;
			thread.Abort();
		}

		private void OnDestroy()
		{
			stop = true;
			thread.Abort();
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
				
				if (secondUser == null)
					secondUser = new User();
				
				secondUserId = responseUser.Id;
				secondUserPosition.x = responseUser.UserPosition.X;
				secondUserPosition.y = responseUser.UserPosition.Y;
				secondUserPosition.z = responseUser.UserPosition.Z;
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