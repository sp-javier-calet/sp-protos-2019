
using System.Collections.Generic;
using UnityEngine;

public class CP_ProgressBar : MonoBehaviour
{
    public RectTransform ProgressBarPlayerContent = null;
    public GameObject ProgressBarPlayer = null;
    public GameObject InitBar;
    public GameObject FinishBar;

    const float kInitCheckPointPosition = 17f;

    List<GameObject> _followingPlayers = new List<GameObject>();
    List<CP_ProgressBarPlayer> _followingPlayersUI = new List<CP_ProgressBarPlayer>();
    float _lastCheckPointPosition = 0f;

    Vector2 _vecTemp = Vector2.zero;

    public void AddPlayerToFollow(GameObject playerGO, int sceneMapLastChekpointIndex)
    {
        if(!_followingPlayers.Contains(playerGO))
        {
            _followingPlayers.Add(playerGO);

            if(ProgressBarPlayerContent != null && ProgressBarPlayer != null)
            {
                GameObject newPlayerBar = Instantiate(ProgressBarPlayer);
                newPlayerBar.transform.SetParent(ProgressBarPlayerContent.transform, false);

                CP_ProgressBarPlayer progressBarPlayer = newPlayerBar.GetComponent<CP_ProgressBarPlayer>();
                progressBarPlayer.GirlTransform.transform.position = new Vector3(-2000f, 2000 * _followingPlayers.Count, 0f);

                _followingPlayersUI.Add(progressBarPlayer);
            }
        }

        _lastCheckPointPosition = (sceneMapLastChekpointIndex * CP_SceneManager.kScenePieceSize) + 1.0f;
    }

    void Update()
    {
        for(var i = 0; i < _followingPlayers.Count; ++i)
        {
            var currentPlayerPos = _followingPlayers[i].transform.position.x;
            if(currentPlayerPos < kInitCheckPointPosition)
            {
                currentPlayerPos = kInitCheckPointPosition;
            }
            if(currentPlayerPos > _lastCheckPointPosition)
            {
                currentPlayerPos = _lastCheckPointPosition;
            }

            if(ProgressBarPlayerContent != null)
            {
                var delta = (currentPlayerPos - kInitCheckPointPosition) / (_lastCheckPointPosition - kInitCheckPointPosition);
                _vecTemp.x = 10.0f + ((ProgressBarPlayerContent.rect.width - 15) * delta);

                _followingPlayersUI[i].SetIconPosition(_vecTemp);
            }
        }
    }
}
