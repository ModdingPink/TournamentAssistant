﻿using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using TournamentAssistant.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TournamentAssistant.Behaviors
{
    class AntiPause : MonoBehaviour
    {
        public static AntiPause Instance { get; set; }

        static readonly Harmony _harmony = new("Tournament Assistant");

        static PauseController _pauseController;

        static PauseController PauseController
        {
            get
            {
                if (_pauseController == null)
                {
                    _pauseController = Resources.FindObjectsOfTypeAll<PauseController>().First();
                }
                return _pauseController;
            }
        }

        [ThreadStatic]
        static bool _forcePause;

        static bool _allowPause;

        public static bool AllowPause
        {
            get { return _allowPause; }
            set
            {
                if (value == _allowPause)
                {
                    return;
                }

                if (value)
                {
                    TournamentAssistantShared.Logger.Info($"Harmony unpatching {nameof(PauseController)}.{nameof(PauseController.Pause)}");
                    _harmony.Unpatch(
                          AccessTools.Method(typeof(PauseController), nameof(PauseController.Pause)),
                          AccessTools.Method(typeof(AntiPause), nameof(PausePrefix))
                    );
                }
                else
                {
                    TournamentAssistantShared.Logger.Info($"Harmony patching {nameof(PauseController)}.{nameof(PauseController.Pause)}");
                    _harmony.Patch(
                        AccessTools.Method(typeof(PauseController), nameof(PauseController.Pause)),
                        new(AccessTools.Method(typeof(AntiPause), nameof(PausePrefix)))
                    );
                }
                _allowPause = value;
            }
        }

        static bool PausePrefix()
        {
            bool runOriginal = _forcePause || AllowPause;
            TournamentAssistantShared.Logger.Debug($"PausePrefix: {runOriginal}");
            return runOriginal;
        }

        public static IEnumerator WaitCanPause()
        {
            var standardLevelGameplayManager = Resources.FindObjectsOfTypeAll<StandardLevelGameplayManager>().First();
            yield return new WaitUntil(() => standardLevelGameplayManager.GetField<StandardLevelGameplayManager.GameState>("_gameState") == StandardLevelGameplayManager.GameState.Playing);
            yield return new WaitUntil(() => PauseController.GetProperty<bool>("canPause"));
        }

        public static void Pause()
        {
            _forcePause = true;
            try
            {
                PauseController.Pause();
            }
            finally
            {
                _forcePause = false;
            }
        }

        void Awake()
        {
            Instance = this;

            DontDestroyOnLoad(this); //Will actually be destroyed when the main game scene is loaded again, but unfortunately this 
                                     //object is created before the game scene loads, so we need to do this to prevent the game scene
                                     //load from destroying it

            StartCoroutine(DoOnLevelStart());
        }

        public IEnumerator DoOnLevelStart()
        {
            yield return WaitCanPause();
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        }

        public static void Destroy() => Instance.OnDestroy();

        void OnDestroy()
        {
            AllowPause = true;
            Instance = null;
        }
    }
}
