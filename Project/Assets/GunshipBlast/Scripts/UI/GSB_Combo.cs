
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GSB_Combo : MonoBehaviour
{
    public enum EComboType
    {
        E_COMBO_AMOUNT,
        E_COMBO_UNIQUES,
        E_COMBO_SHAPE
    }

    public GameObject Panel = null;
    public TextMeshProUGUI ShipAmount = null;
    public TextMeshProUGUI AmmoReward = null;
    public GameObject Shape = null;
    public List<GameObject> ShipUniques = new List<GameObject>();

    void Start()
    {
        if(Panel != null)
        {
            Explosion explosion = GetComponent<Explosion>();
            if(explosion != null)
            {
                Panel.transform.DOLocalMove(new Vector3(0f, -1f, 0f), explosion.TimeToDisappear);
            }
        }
    }

    public void SetComboTypeAndData(EComboType type, int rewardAmount, int shipAmount = 0)
    {
        if(type == EComboType.E_COMBO_AMOUNT)
        {
            if(ShipAmount != null)
            {
                ShipAmount.gameObject.SetActive(true);
                ShipAmount.text = "x" + shipAmount;
            }
        }
        else if(type == EComboType.E_COMBO_UNIQUES)
        {
            for(var i = 0; i < ShipUniques.Count; ++i)
            {
                ShipUniques[i].SetActive(false);
            }
        }
        else if(type == EComboType.E_COMBO_SHAPE)
        {
            if(Shape != null)
            {
                Shape.gameObject.SetActive(true);
            }

            if(ShipAmount != null)
            {
                ShipAmount.gameObject.SetActive(true);
                ShipAmount.text = "x" + shipAmount;
            }
        }

        if(AmmoReward != null)
        {
            AmmoReward.text = "+" + rewardAmount;
        }
    }

    public void AddComboUniqueShip(GSB_EnemyController.EShipType shipType)
    {
        ShipUniques[(int)shipType].SetActive(true);
    }
}
