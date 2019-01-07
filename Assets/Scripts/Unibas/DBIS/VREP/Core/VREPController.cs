﻿using System;
using DefaultNamespace;
using DefaultNamespace.VREM;
using DefaultNamespace.VREM.Model;
using Unibas.DBIS.VREP.Core;
using UnityEngine;
using World;

namespace Unibas.DBIS.VREP
{
    public class VREPController : MonoBehaviour
    {
        private VREMClient _vremClient;
        private BuildingManager _buildingManager;
        public String ExhibitionId = "5c17b10ea6abfddbb3fa66ae";

        public Vector3 LobbySpawn = new Vector3(0, -9, 0);

        public Settings Settings;

        public string settingsPath;

        public static VREPController Instance;

        private void Awake()
        {
            if (Application.isEditor)
            {
                if (string.IsNullOrEmpty(settingsPath))
                {
                    Settings = Settings.LoadSettings();
                }
                else
                {
                    Settings = Settings.LoadSettings(settingsPath);
                }
            }
            else
            {
                Settings = Settings.LoadSettings();
            }

            SanitizeHost();
            Instance = this;
        }

        private void SanitizeHost()
        {
            if (!Settings.VREMAddress.EndsWith("/"))
            {
                Settings.VREMAddress += "/";
            }

            if (!Settings.VREMAddress.StartsWith("http://"))
            {
                Settings.VREMAddress = "http://" + Settings.VREMAddress;
            }
        }

        private void OnApplicationQuit()
        {
            Settings.StoreSettings();
        }

        private void Start()
        {
            if (Settings == null)
            {
                Settings = Settings.LoadSettings();
                if (Settings == null)
                {
                    Settings = Settings.Default();
                }
            }
            var go = GameObject.FindWithTag("Player");
            if (go != null && Settings.StartInLobby)
            {
                go.transform.position = new Vector3(0, -9.9f, 0);
            }

            Debug.Log("Starting ExMan");
            _vremClient = gameObject.AddComponent<VREMClient>();
            _buildingManager = GetComponent<BuildingManager>();

            LoadAndCreateExhibition();
        }

        public void LoadAndCreateExhibition()
        {
            _vremClient.ServerUrl = Settings.VREMAddress;

            var exId = "";
            if (Settings.exhibitionIds != null && Settings.exhibitionIds.Length > 0 && Settings.exhibitionIds[0] != null)
            {
                exId = Settings.exhibitionIds[0];
            }
            else
            {
                exId = ExhibitionId;
            }


            _vremClient.RequestExhibition(exId, ParseExhibition);
            Debug.Log("Requested ex");
        }

        private void ParseExhibition(string json)
        {
            if (json == null)
            {
                Debug.LogError("Couldn't load exhibition from backend");
                Debug.Log("Loading placeholder instead");
                var jtf = Resources.Load<TextAsset>("Configs/placeholder-exhibition");
                json = jtf.text;
            }

            Debug.Log(json);
            Exhibition ex = JsonUtility.FromJson<Exhibition>(json);
            Debug.Log(json);
            Debug.Log(ex);
            Debug.Log(_buildingManager);
            // TODO create lobby
            
            ExhibitionManager exhibitionManager = new ExhibitionManager(ex);
            exhibitionManager.GenerateExhibition();
            //_buildingManager.Create(ex);
            
            
            //_buildingManager.BuildRoom(ex.rooms[0]);
/*
      if (Settings.PlaygroundEnabled)
      {*/
            /*
            ex.rooms[0].position = new Vector3(15,0,0);
            ObjectFactory.BuildRoom(ex.rooms[0]);
            */
            //    }
        }
    }
}