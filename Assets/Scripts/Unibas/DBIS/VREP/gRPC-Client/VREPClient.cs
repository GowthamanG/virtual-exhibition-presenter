using System;
using System.Collections;
using System.Collections.Generic;
using Grpc.Core;
using Grpc.Core.Utils;
using UnityEditor.VersionControl;
using UnityEngine;
using System.Threading.Tasks;
using Valve.VR.InteractionSystem;
using Task = System.Threading.Tasks.Task;

public class VREPClient : MonoBehaviour
{
	public string host;
	public int port;
	public GameObject player1;
	public GameObject secondUserObject;
	private multiUserSync.multiUserSyncClient client;
	private User user;
	private User secondUser;
	// Use this for initialization
	void Start ()
	{
		Channel channel = new Channel(host, port, ChannelCredentials.Insecure);
		
		client = new multiUserSync.multiUserSyncClient(channel);
		
		var position = player1.transform.position;
		var rotation = player1.transform.rotation;
		var lossyScale = player1.transform.lossyScale;

		user = new User
		{
			Id = player1.GetInstanceID(),
			
			PlayerPosition = new Vector()
			{
				X = position.x,
				Y = position.y,
				Z = position.z,
			},
			
			PlayerRotation = new Vector()
			{
				X = rotation.x,
				Y = rotation.y,
				Z = rotation.z,
			},
			
			PlayerScale = new Vector()
			{
				X = lossyScale.x,
				Y = lossyScale.y,
				Z = lossyScale.z,
			}
		};
		
		Debug.Log("Player ID: " + user.Id);
		Debug.Log("Player details: " + player1.ToString());
		Debug.Log("Player position x: " + user.PlayerPosition.X);

		SetUser().Wait();
		GetUser(Time.deltaTime).Wait();
		
		Vector3 positionSecondUser = new Vector3(secondUser.PlayerPosition.X, secondUser.PlayerPosition.Y,
			secondUser.PlayerPosition.Z);

		Instantiate(secondUserObject, positionSecondUser, Quaternion.Euler(0,0,0));

		channel.ShutdownAsync().Wait();
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		updateUser(user, player1);
		updateUser(secondUser, secondUserObject);
		SetUser().Wait();
		GetUser(Time.deltaTime).Wait();

	}

	//This method is to update the partners position in the same exhibition
	

	public async Task SetUser()
	{
		try
		{
			using (var call = client.setUser())
			{
				var responseReaderTask = Task.Run(async () =>
				{
					while (await call.ResponseStream.MoveNext())
					{
						var note = call.ResponseStream.Current;
						Debug.Log("Received: " + note.ToString());
					}
				});

				await call.RequestStream.WriteAsync(user);
				await call.RequestStream.CompleteAsync();
				await responseReaderTask;

			}
		}
		catch (RpcException e)
		{
			Console.WriteLine("RPC failed" + e);
			throw;
		}
	}

	
	public async Task GetUser(float deltaTime)
	{
		try
		{
			using (var call = client.getUser())
			{
				var responseReaderTask = Task.Run(async () =>
				{
					while (await call.ResponseStream.MoveNext())
					{
						var note = call.ResponseStream.Current;
						Debug.Log("Second user received: " + note);
						secondUser = note;
						//deltaTime used to update values per second instead per frame
						secondUser.PlayerPosition.X *= deltaTime;
						secondUser.PlayerPosition.Y *= deltaTime;
						secondUser.PlayerPosition.Z *= deltaTime;
					}
				});

				RequestUser requestUser = new RequestUser {RequestUserID = user.Id};

				await call.RequestStream.WriteAsync(requestUser);
				await call.RequestStream.CompleteAsync();
				await responseReaderTask;

			}
		}
		catch (RpcException e)
		{
			Console.WriteLine("RPC failed" + e);
			throw;
		}
	}

	void updateUser(User user, GameObject gameObject)
	{
		var position = gameObject.transform.position;
		var rotation = gameObject.transform.rotation;
		var lossyScale = gameObject.transform.lossyScale;
		
		user = new User
		{
			Id = gameObject.GetInstanceID(),
			
			PlayerPosition = new Vector()
			{
				X = position.x,
				Y = position.y,
				Z = position.z,
			},
			
			PlayerRotation = new Vector()
			{
				X = rotation.x,
				Y = rotation.y,
				Z = rotation.z,
			},
			
			PlayerScale = new Vector()
			{
				X = lossyScale.x,
				Y = lossyScale.y,
				Z = lossyScale.z,
			}
		};

		user.Id = gameObject.GetInstanceID();
		user.PlayerPosition.X = position.x;
		user.PlayerPosition.Y = position.y;
		user.PlayerPosition.Z = position.z;
		user.PlayerRotation.X = rotation.x;
		user.PlayerRotation.Y = rotation.y;
		user.PlayerRotation.Z = rotation.z;
		user.PlayerScale.X = rotation.x;
		user.PlayerScale.Y = rotation.y;
		user.PlayerScale.Z = rotation.z;

	}
	
}
