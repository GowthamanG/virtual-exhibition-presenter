using System;
using System.Threading;
using Grpc.Core;
using UnityEngine;
using UnityEngine.XR;

namespace Unibas.DBIS.VREP
{
	public class TrackerClient : MonoBehaviour
	{
		//private AutoResetEvent resetEvent;
		private Thread connectionThread;
		public string host;
		public int port;
		public GameObject box;
		public GameObject player;
		private multiUserSync.multiUserSyncClient client;
		private Tracker firstTracker;
		private Channel channel;
		private int firstTrackerId;
		private Vector3 firstTrackerPosition;
		private Quaternion firstTrackerRotation;
		private Vector3 firstTrackerScale;
		private bool stop;
		public bool trackerIsActive;
		private bool trackerInstantiated;
		private bool strangeTrackerActive;
		private GameObject cubetracker;
		private float translateX, translateY, translateZ;

		// Use this for initialization
		void Start ()
		{
			if (trackerIsActive)
			{
				
				
				firstTrackerId = GetInstanceID();
				firstTrackerPosition = transform.position;
				firstTrackerRotation = transform.rotation;

				firstTracker = new Tracker()
				{
					Id = firstTrackerId,

					TrackerPosition = new Vector()
					{
						X = firstTrackerPosition.x,
						Y = firstTrackerPosition.y,
						Z = firstTrackerPosition.z,
					},

					TrackerRotation = new Quadrublet()
					{
						X = firstTrackerRotation.x,
						Y = firstTrackerRotation.y,
						Z = firstTrackerRotation.z,
						W = firstTrackerRotation.w
					},

					TrackerScale = new Vector()
					{
						X = firstTrackerScale.x,
						Y = firstTrackerScale.y,
						Z = firstTrackerScale.z,
					}
				};

			}
			else
			{
				firstTracker = new Tracker();
				firstTrackerId = 0;
				firstTrackerPosition = new Vector3();
				firstTrackerRotation = new Quaternion();
				firstTrackerScale = new Vector3();
			}

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
		

			trackerInstantiated = false;
			strangeTrackerActive = false;

			cubetracker = new GameObject();
			
			connectionThread = new Thread(Run);
			connectionThread.Start();

			translateX = player.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x;
			translateY = player.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y;
			translateZ = player.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z;
			




			//client.setUser(user);
			//SetUser(user);


			/*task = SetUser();
		task.Wait();*/

			//Vector3 positionSecondUser = new Vector3(secondUser.PlayerPosition.X, secondUser.PlayerPosition.Y, secondUser.PlayerPosition.Z);

			//Instantiate(secondUserObject, positionSecondUser, Quaternion.Euler(0,0,0));

			//channel.ShutdownAsync().Wait();

		}
	
		// Update is called once per frame
		void Update()
		{
			/*UpdateUser(user, player1);
		//updateUser(secondUser, secondUserObject);
		//task.Wait();
		//client.setUser(user);
		client.setUser(user);*/
			//resetEvent.Set();
			if (trackerIsActive)
			{
				/*firstTrackerPosition = transform.position;
				firstTrackerRotation = transform.rotation;*/
				firstTrackerPosition = transform.position;
				firstTrackerRotation = transform.rotation;
				firstTrackerScale = transform.lossyScale;
				UpdateTracker(firstTracker, firstTrackerId, firstTrackerPosition, firstTrackerRotation, firstTrackerScale);
			}

			
			//UpdateUser(secondUser, secondUserId, secondUserPosition, secondUserRotation, secondUserScale);

			if (trackerIsActive == false && trackerInstantiated == false && strangeTrackerActive)
			{
				trackerInstantiated = true;
				cubetracker = Instantiate(box, firstTrackerPosition, firstTrackerRotation);
			}

			//player2.transform.position = secondUserPosition;
			//player2.transform.rotation = secondUserRotation;
			if (trackerIsActive == false && strangeTrackerActive && trackerInstantiated)
				cubetracker.transform.SetPositionAndRotation(firstTrackerPosition, firstTrackerRotation);


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
				SetTracker(firstTracker);
				if (trackerIsActive == false)
				{
					GetTracker(00); //Trick to get user which is NOT equal firstUserId, details see implementation on server
					//deltaTime.TotalSeconds;
					strangeTrackerActive = true;
				}

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


		private void SetTracker(Tracker tracker)
		{
			try
			{
				Response serverResponse = client.setTracker(tracker);
				Debug.Log("User is set: " + serverResponse.Response_);

			}
			catch (RpcException e)
			{
				Debug.Log("RPC failed in method \"setTracker\"" + e);
			}
		
		}

		private void GetTracker(int trackerId)
		{
			try
			{
				RequestTracker requestTracker = new RequestTracker()
				{
					 RequestTrackerID = trackerId
				};

				var responseTracker = client.getTracker(requestTracker);

				if (responseTracker.Id != 0)
				{

					firstTrackerId = responseTracker.Id;
					firstTrackerPosition.x = responseTracker.TrackerPosition.X + translateX;
					firstTrackerPosition.y = responseTracker.TrackerPosition.Y - translateY;
					firstTrackerPosition.z = responseTracker.TrackerPosition.Z - translateZ;
					firstTrackerRotation.x = responseTracker.TrackerRotation.X;
					firstTrackerRotation.y = responseTracker.TrackerRotation.Y;
					firstTrackerRotation.z = responseTracker.TrackerRotation.Z;
					firstTrackerRotation.w = responseTracker.TrackerRotation.W;
					firstTrackerScale.x = responseTracker.TrackerScale.X;
					firstTrackerScale.y = responseTracker.TrackerScale.Y;
					firstTrackerScale.z = responseTracker.TrackerScale.Z;
				}

			}
			catch (RpcException e)
			{
				Debug.Log("RPC failed in method \"getUser\" " + e);
			}
		}


		private void UpdateTracker(Tracker tracker, int trackerId, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			tracker.Id = trackerId;
			tracker.TrackerPosition.X = position.x;
			tracker.TrackerPosition.Y = position.y;
			tracker.TrackerPosition.Z = position.z;
			tracker.TrackerRotation.X = rotation.x;
			tracker.TrackerRotation.Y = rotation.y;
			tracker.TrackerRotation.Z = rotation.z;
			tracker.TrackerRotation.W = rotation.w;
			tracker.TrackerScale.X = scale.x;
			tracker.TrackerScale.Y = scale.y;
			tracker.TrackerScale.Z = scale.z;
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