using System;
using System.Collections.Generic;
using System.Threading;
using Grpc.Core;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace Unibas.DBIS.VREP
{
	public class TrackerClient : MonoBehaviour
	{
		private Thread connectionThread;
		public string host;
		public int port;
		public GameObject box;
		public GameObject player;
		private Vector3 playerPosition;
		private multiUserSync.multiUserSyncClient client;
		private Tracker tracker;
		private Channel channel;
		private int trackerId;
		private Vector3 trackerPhysicalPosition;
		private Vector3 trackerVRPosition;
		private Quaternion trackerRotation;
		private bool stop;
		public bool trackerIsActive;
		private bool trackerIsInstantiated;
		private bool strangeTrackerIsActive;
		private GameObject cubetracker;

		private List<XRNodeState> nodes;
		private SteamVR_TrackedObject trackedObject;
		private XRNodeState hardwareTracker;

		private Vector3 translate;

		// Use this for initialization
		void Start()
		{
			nodes = new List<XRNodeState>();
			InputTracking.GetNodeStates(nodes);
			hardwareTracker = nodes.Find(state => trackedObject);

			
			if (trackerIsActive)
			{
				
				trackerId = GetInstanceID();
				hardwareTracker.TryGetPosition(out trackerPhysicalPosition);
				trackerVRPosition = transform.position;
				trackerRotation = transform.rotation;

				tracker = new Tracker()
				{
					Id = trackerId,

					TrackerPhysicalPosition = new Vector()
					{
						X = trackerPhysicalPosition.x,
						Y = trackerPhysicalPosition.y,
						Z = trackerPhysicalPosition.z,
					},
					
					TrackerVRPositon = new Vector()
					{
						X = trackerVRPosition.x,
						Y = trackerVRPosition.y,
						Z = trackerVRPosition.z
					},

					TrackerRotation = new Quadrublet()
					{
						X = trackerRotation.x,
						Y = trackerRotation.y,
						Z = trackerRotation.z,
						W = trackerRotation.w
					}
				};

			}
			else
			{
				tracker = new Tracker();
				trackerId = 0;
				trackerVRPosition = new Vector3();
				trackerRotation = new Quaternion();
			}

			trackedObject = GetComponent<SteamVR_TrackedObject>();			

			trackerIsInstantiated = false;
			strangeTrackerIsActive = false;

			cubetracker = new GameObject();

			connectionThread = new Thread(Run);
			connectionThread.Start();
			
			translate = new Vector3()
			{
				x = player.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x,
				y = player.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y,
				z = player.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z
			};
			
			
		}

		// Update is called once per frame
		void Update()
		{

			Debug.Log("TRACKER PHYSICAL POSITION: " + trackerPhysicalPosition);
			translate.x = player.transform.position.x - InputTracking.GetLocalPosition(XRNode.Head).x;
			translate.y = player.transform.position.y - InputTracking.GetLocalPosition(XRNode.Head).y;
			translate.z = player.transform.position.z - InputTracking.GetLocalPosition(XRNode.Head).z;
			
			if (trackerIsActive)
			{
				hardwareTracker.TryGetPosition(out trackerPhysicalPosition);
				trackerVRPosition = transform.position;
				trackerRotation = transform.rotation;
				UpdateTracker(tracker, trackerId, trackerPhysicalPosition, trackerVRPosition, trackerRotation);
			}


			if (trackerIsActive == false && trackerIsInstantiated == false && strangeTrackerIsActive)
			{
				trackerIsInstantiated = true;
				cubetracker = Instantiate(box, trackerVRPosition, trackerRotation);
			}

			if (trackerIsActive == false && strangeTrackerIsActive && trackerIsInstantiated)
				cubetracker.transform.SetPositionAndRotation(trackerVRPosition, trackerRotation);


		}

		private void Run()
		{
		
			channel = new Channel(host, port, ChannelCredentials.Insecure);
			client = new multiUserSync.multiUserSyncClient(channel);
		
			while (!stop || channel.State != ChannelState.Shutdown) //The synchronization happens in the while loop
			{
				SetTracker(tracker);
				if (trackerIsActive == false)
				{
					GetTracker(00); //Trick to get user which is NOT equal firstUserId, details see implementation on server
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

					this.trackerId = responseTracker.Id;
					trackerVRPosition.x = responseTracker.TrackerPhysicalPosition.X + translate.x;
					trackerVRPosition.y = responseTracker.TrackerPhysicalPosition.Y + translate.y;
					trackerVRPosition.z = responseTracker.TrackerPhysicalPosition.Z + translate.z;
					
					trackerRotation.x = responseTracker.TrackerRotation.X;
					trackerRotation.y = responseTracker.TrackerRotation.Y;
					trackerRotation.z = responseTracker.TrackerRotation.Z;
					trackerRotation.w = responseTracker.TrackerRotation.W;
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