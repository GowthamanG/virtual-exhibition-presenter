using System;
using System.Threading;
using Grpc.Core;
using UnityEngine;
using UnityEngine.XR;

namespace Unibas.DBIS.VREP
{
	public class TrackerClient : MonoBehaviour
	{
		private Thread connectionThread;
		public string host;
		public int port;
		public GameObject box;
		public GameObject player;
		private multiUserSync.multiUserSyncClient client;
		private Tracker firstTracker;
		private Channel channel;
		private int firstTrackerId;
		private Vector3 firstTrackerPhysicalPosition;
		private Quaternion firstTrackerPhysicalRotation;
		private Vector3 firstTrackerVRWorldPosition;
		private Quaternion firstTrackerVRWorldRotation;
		
		private bool stop;
		public bool trackerIsActive;
		private bool trackerIsInstantiated;
		private bool strangeTrackerIsActive;
		private GameObject cubetracker;
		private float translateX, translateY, translateZ;

		// Use this for initialization
		void Start ()
		{
			if (trackerIsActive)
			{
				
				
				firstTrackerId = GetInstanceID();
				firstTrackerPhysicalPosition = transform.position;
				firstTrackerPhysicalRotation = transform.rotation;

				firstTracker = new Tracker()
				{
					Id = firstTrackerId,

					TrackerPhysicalPosition = new Vector()
					{
						X = firstTrackerPhysicalPosition.x,
						Y = firstTrackerPhysicalPosition.y,
						Z = firstTrackerPhysicalPosition.z,
					},

					TrackerPhysicalRotation = new Quadrublet()
					{
						X = firstTrackerPhysicalRotation.x,
						Y = firstTrackerPhysicalRotation.y,
						Z = firstTrackerPhysicalRotation.z,
						W = firstTrackerPhysicalRotation.w
					},
					
					TrackerVRWorldPosition = new Vector()
					{
						X = firstTrackerPhysicalPosition.x,
						Y = firstTrackerPhysicalPosition.y,
						Z = firstTrackerPhysicalPosition.z,
					},

					TrackerVRWorldRotation = new Quadrublet()
					{
						X = firstTrackerPhysicalRotation.x,
						Y = firstTrackerPhysicalRotation.y,
						Z = firstTrackerPhysicalRotation.z,
						W = firstTrackerPhysicalRotation.w
					},

				};

			}
			else
			{
				firstTracker = new Tracker();
				firstTrackerId = 0;
				firstTrackerVRWorldPosition = new Vector3();
				firstTrackerVRWorldRotation = new Quaternion();
			}

			trackerIsInstantiated = false;
			strangeTrackerIsActive = false;

			cubetracker = new GameObject();
			
			connectionThread = new Thread(Run);
			connectionThread.Start();

			translateX = player.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x;
			translateY = player.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y;
			translateZ = player.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z;
		}
	
		// Update is called once per frame
		void Update()
		{
			if (trackerIsActive)
			{
				firstTrackerPhysicalPosition = transform.position;
				firstTrackerPhysicalRotation = transform.rotation;
				UpdateTracker(firstTracker, firstTrackerId, firstTrackerVRWorldPosition, firstTrackerVRWorldRotation, firstTrackerPhysicalPosition, firstTrackerPhysicalRotation);
			}


			if (trackerIsActive == false && trackerIsInstantiated == false && strangeTrackerIsActive)
			{
				trackerIsInstantiated = true;
				cubetracker = Instantiate(box, firstTrackerPhysicalPosition, firstTrackerPhysicalRotation);
			}

			if (trackerIsActive == false && strangeTrackerIsActive && trackerIsInstantiated)
				cubetracker.transform.SetPositionAndRotation(firstTrackerPhysicalPosition, firstTrackerPhysicalRotation);


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
				SetTracker(firstTracker);
				if (trackerIsActive == false)
				{
					GetTracker(00); //Trick to get user which is NOT equal firstUserId, details see implementation on server
					//deltaTime.TotalSeconds;
					strangeTrackerIsActive = true;
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
					firstTrackerPhysicalPosition.x = responseTracker.TrackerPhysicalPosition.X + translateX;
					firstTrackerPhysicalPosition.y = responseTracker.TrackerPhysicalPosition.Y + translateY;
					firstTrackerPhysicalPosition.z = responseTracker.TrackerPhysicalPosition.Z + translateZ;
					
					firstTrackerPhysicalRotation.x = responseTracker.TrackerPhysicalRotation.X;
					firstTrackerPhysicalRotation.y = responseTracker.TrackerPhysicalRotation.Y;
					firstTrackerPhysicalRotation.z = responseTracker.TrackerPhysicalRotation.Z;
					firstTrackerPhysicalRotation.w = responseTracker.TrackerPhysicalRotation.W;
					
					firstTrackerVRWorldPosition.x = responseTracker.TrackerVRWorldPosition.X + translateX;
					firstTrackerVRWorldPosition.y = responseTracker.TrackerVRWorldPosition.Y + translateY;
					firstTrackerVRWorldPosition.z = responseTracker.TrackerVRWorldPosition.Z + translateZ;
					
					firstTrackerVRWorldRotation.x = responseTracker.TrackerVRWorldRotation.X;
					firstTrackerVRWorldRotation.y = responseTracker.TrackerVRWorldRotation.Y;
					firstTrackerVRWorldRotation.z = responseTracker.TrackerVRWorldRotation.Z;
					firstTrackerVRWorldRotation.w = responseTracker.TrackerVRWorldRotation.W;
				}

			}
			catch (RpcException e)
			{
				Debug.Log("RPC failed in method \"getUser\" " + e);
			}
		}


		private void UpdateTracker(Tracker tracker, int userId, Vector3 worldPosition, Quaternion worldRotation, Vector3 physicalPosition, Quaternion physicalRotation)
		{
			tracker.Id = userId;
			tracker.TrackerVRWorldPosition.X = worldPosition.x;
			tracker.TrackerVRWorldPosition.Y = worldPosition.y;
			tracker.TrackerVRWorldPosition.Z = worldPosition.z;
			
			tracker.TrackerVRWorldRotation.X = worldRotation.x;
			tracker.TrackerVRWorldRotation.Y = worldRotation.y;
			tracker.TrackerVRWorldRotation.Z = worldRotation.z;
			tracker.TrackerVRWorldRotation.W = worldRotation.w;
			
			tracker.TrackerPhysicalPosition.X = physicalPosition.x;
			tracker.TrackerPhysicalPosition.Y = physicalPosition.y;
			tracker.TrackerPhysicalPosition.Z = physicalPosition.z;
			
			tracker.TrackerPhysicalRotation.X = physicalRotation.x;
			tracker.TrackerPhysicalRotation.Y = physicalRotation.y;
			tracker.TrackerPhysicalRotation.Z = physicalRotation.z;
			tracker.TrackerPhysicalRotation.W = physicalRotation.w;
			
			
		}

	}
}