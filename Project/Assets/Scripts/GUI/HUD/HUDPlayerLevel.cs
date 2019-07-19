//-----------------------------------------------------------------------
// HUDPlayerLevel.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SocialPoint.Dependency;

public class HUDPlayerLevel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    Text _level;

    //PlayerModel _player;  // TODO IVAN

    void Start()
    {
        //_player = Services.Instance.Resolve<PlayerModel>();
        //_level.text = _player.Level.ToString();
    }

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Player level hud button clicked");
    }

    #endregion
}


