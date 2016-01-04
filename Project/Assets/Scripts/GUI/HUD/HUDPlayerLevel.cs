using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UnityEngine.EventSystems;

public class HUDPlayerLevel: MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    Text _level;

    [Inject]
    PlayerModel _player;

    [PostInject]
    void PostInject()
    {
        _level.text = _player.Level.ToString();
    }

    #region IPointerClickHandler implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Player level hud button clicked");
    }

    #endregion
}


