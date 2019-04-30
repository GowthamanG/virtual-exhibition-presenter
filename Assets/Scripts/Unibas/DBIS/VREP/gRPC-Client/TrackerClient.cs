using System;
using System.Threading;
using Grpc.Core;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

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
		private Vector3 firstTrackerVRPosition;
		private Quaternion firstTrackerRotation;
		private bool stop;
		public bool trackerIsActive;
		private bool trackerIsInstantiated;
		private bool strangeTrackerIsActive;
		private GameObject cubetracker;
		private float translateX, translateY, translateZ;

		private SteamVR_TrackedObject trackedObject;

		// Use this for initialization
		void Start()
		{
			if (trackerIsActive)
			{

		
				firstTrackerId = GetInstanceID();
				firstTrackerPhysicalPosition = InputTracking.GetLocalPosition(XRNode.Head);
				firstTrackerVRPosition = transform.position;
				firstTrackerRotation = transform.rotation;

				firstTracker = new Tracker()
				{
					Id = firstTrackerId,

					TrackerPhysicalPosition = new Vector()
					{
						X = firstTrackerPhysicalPosition.x,
						Y = firstTrackerPhysicalPosition.y,
						Z = firstTrackerPhysicalPosition.z,
					},
					
					TrackerVRPositon = new Vector()
					{
						X = firstTrackerVRPosition.x,
						Y = firstTrackerVRPosition.y,
						Z = firstTrackerVRPosition.z
					},

					TrackerRotation = new Quadrublet()
					{
						X = firstTrackerRotation.x,
						Y = firstTrackerRotation.y,
						Z = firstTrackerRotation.z,
						W = firstTrackerRotation.w
					}
				};

			}
			else
			{
				firstTracker = new Tracker();
				firstTrackerId = 0;
				firstTrackerVRPosition = new Vector3();
				firstTrackerRotation = new Quaternion();
			}

			trackedObject = GetComponent<SteamVR_TrackedObject>();

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
			translateX = player.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x;
			translateY = player.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y;
			translateZ = player.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z;
			
			Debug.Log("VR Post: " + transform.position + " ,Physical Pos: " + trackedObject.transform.position );
			
			if (trackerIsActive)
			{
				firstTrackerVRPosition = transform.position;
				firstTrackerRotation = transform.rotation;
				UpdateTracker(firstTracker, firstTrackerId, firstTrackerPhysicalPosition, firstTrackerVRPosition, firstTrackerRotation);
			}


			if (trackerIsActive == false && trackerIsInstantiated == false && strangeTrackerIsActive)
			{
				trackerIsInstantiated = true;
				cubetracker = Instantiate(box, firstTrackerVRPosition, firstTrackerRotation);
			}

			if (trackerIsActive == false && strangeTrackerIsActive && trackerIsInstantiated)
				cubetracker.transform.SetPositionAndRotation(firstTrackerVRPosition, firstTrackerRotation);


		}

		private void Run()
		{
		
			channel = new Channel(host, port, ChannelCredentials.Insecure);
			client = new multiUserSync.multiUserSyncClient(channel);
			DateTime time = DateTime.Now;
		
			while (!stop || channel.State != ChannelState.Shutdown) //The synchronization happens in the while loop
			{
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
					firstTrackerVRPosition.x = responseTracker.TrackerPhysicalPosition.X + translateX;
					firstTrackerVRPosition.y = responseTracker.TrackerPhysicalPosition.Y + translateY;
					firstTrackerVRPosition.z = responseTracker.TrackerPhysicalPosition.Z + translateZ;
					firstTrackerRotation.x = responseTracker.TrackerRotation.X;
					firstTrackerRotation.y = responseTracker.TrackerRotation.Y;
					firstTrackerRotation.z = responseTracker.TrackerRotation.Z;
					firstTrackerRotation.w = responseTracker.TrackerRotation.W;
				}

			}
			catch (RpcException e)
			{
				Debug.Log("RPC failed in method \"getUser\" " + e);
			}
		}


		private void UpdateTracker(Tracker tracker, int trackerId, Vector3 physicalPosition, Vector3 vrPosition, Quaternion rotation)
		{
			tracker.Id = trackerId;
			tracker.TrackerPhysicalPosition.X = physicalPosition.x;
			tracker.TrackerPhysicalPosition.Y = physicalPosition.y;
			tracker.TrackerPhysicalPosition.Z = physicalPosition.z;
			tracker.TrackerVRPositon.X = vrPosition.x;
			tracker.TrackerVRPositon.Y = vrPosition.y;
			tracker.TrackerVRPositon.Z = vrPosition.z;
			tracker.TrackerRotation.X = rotation.x;
			tracker.TrackerRotation.Y = rotation.y;
			tracker.TrackerRotation.Z = rotation.z;
			tracker.TrackerRotation.W = rotation.w;
		}

	}
}