using System.Collections;
using System.Collections.Generic;
using Grpc.Core;
using Grpc.Core.Utils;
using UnityEditor.VersionControl;
using UnityEngine;

using UnityEngine.Networking;

public class VREPClient : MonoBehaviour
{

	private GameObject player1;

	// Use this for initialization
	void Start ()
	{
		Channel channel = new Channel("localhost:8080", ChannelCredentials.Insecure);
		
		var client = new multiUserSync.multiUserSyncClient(channel);

		player1 = GameObject.Find("Player");
		
		Debug.Log("Der Name des Players im Spiel ist: " + player1.name);

		User user = new User();
		user.PlayerPosition.X = player1.transform.position.x;
		user.PlayerPosition.Y = player1.transform.position.y;
		user.PlayerPosition.Z = player1.transform.position.z;
		user.PlayerRotation.X = player1.transform.rotation.x;
		user.PlayerRotation.Y = player1.transform.rotation.y;
		user.PlayerRotation.Z = player1.transform.rotation.z;
		user.PlayerScale.X = player1.transform.lossyScale.x;
		user.PlayerScale.Y = player1.transform.lossyScale.y;
		user.PlayerScale.Z = player1.transform.lossyScale.z;
	}
	
	// Update is called once per frame
	void Update () {
		
		
	}
	

	
}
