
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class UILoading : MonoBehaviour
    {
        [SerializeField] private Button play;

        private void Start()
        {
            play.onClick.AddListener((() =>
            {
                Loader.Load(Loader.Scene.Battle);
            }));
        }
    }
