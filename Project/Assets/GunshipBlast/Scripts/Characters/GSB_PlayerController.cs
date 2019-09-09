
using System.Collections.Generic;
using SocialPoint.Rendering.Components;
using SocialPoint.Utils;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using DG.Tweening;
using UnityEngine.Networking;

public class GSB_PlayerController : MonoBehaviour
{
    public GameObject ShipTransform = null;
    public BCSHModifier ShipBCSH = null;
    public GameObject ShipShoots = null;
    public GameObject Explosion = null;

    bool _pressedDown = false;
    bool _pressedUp = false;

    Triangulator _triangulator = new Triangulator();
    bool _holding = false;
    bool _shapeIsClosed = false;

    bool _shooting = false;
    int _shootToEnemyIdx = 0;
    Timer _shootTimer = new Timer();
    public bool Shooting { get { return _shooting; } }

    List<GSB_EnemyController> SelectingEnemies = new List<GSB_EnemyController>();
    List<GSB_EnemyController> EnemiesInside = new List<GSB_EnemyController>();
    List<GSB_EnemyController> EnemiesToShoot = new List<GSB_EnemyController>();

    List<GameObject> HullGOs = new List<GameObject>();
    List<BCSHModifier> AmmoBCSH = new List<BCSHModifier>();

    Sequence _tremblingAnimation = null;
    Vector3 _vecTemp = Vector3.zero;
    Timer _ammoRefillTimer = new Timer();
    int _currentAmmo = -1;
    int _ammoWasted = 0;
    int _ammoAsCombinationReward = 0;
    int _currentHealth = -1;
    float _currentTimePercentage = 1f;
    long _timeProcessStartingTime = 0;
    int _timeProcessAvailableTime = 0;
    bool _dying = false;
    Timer _explosionTimer = new Timer();

    Vector2[] _shapeVertices2D = null;
    Vector3[] _shapeVertices3D = null;

    void Awake()
    {
        _shapeVertices2D = new Vector2[GSB_SceneManager.Instance.AmmoMax];
        _shapeVertices3D = new Vector3[GSB_SceneManager.Instance.AmmoMax];
    }

    void Start()
    {
        if(GSB_SceneManager.Instance.HealthBox != null)
        {
            for(var i = 0; i < GSB_SceneManager.Instance.HealthBox.transform.childCount; ++i)
            {
                HullGOs.Add(GSB_SceneManager.Instance.HealthBox.transform.GetChild(i).gameObject);
            }
        }

        if(GSB_SceneManager.Instance.AmmoBox != null)
        {
            AmmoBCSH.AddRange(GSB_SceneManager.Instance.AmmoBox.GetComponentsInChildren<BCSHModifier>());
        }

        _currentAmmo = GSB_SceneManager.Instance.AmmoMax;
        _currentHealth = GSB_SceneManager.Instance.HealthMax;
        _currentTimePercentage = 1f;
        _timeProcessStartingTime = TimeUtils.TimestampMilliseconds;
        _timeProcessAvailableTime = GSB_SceneManager.Instance.TotalTimeRegeneration;
    }

    public void OnPressedDown()
    {
        _pressedDown = true;
    }

    public void OnPressedUp()
    {
        _pressedUp = true;
    }

    GSB_EnemyController CheckEnemyTouch()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var layerMaskEnemies = (1 << 19);
        if(Physics.Raycast(ray, out hit, float.MaxValue, layerMaskEnemies))
        {
            return hit.transform.GetComponent<GSB_EnemyController>();
        }

        return null;
    }

    GSB_EnemyController CheckLineCrossingEnemies()
    {
        LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count - 1];

        Vector3 enemyToLineRay =
            currentLine.GetPosition(1) - SelectingEnemies[SelectingEnemies.Count - 1].transform.position;

        var distance = enemyToLineRay.magnitude;

        RaycastHit hit;
        var layerMaskEnemies = (1 << 19);
        if(Physics.Raycast(SelectingEnemies[SelectingEnemies.Count - 1].transform.position, enemyToLineRay.normalized,
            out hit, distance, layerMaskEnemies))
        {
            return hit.transform.GetComponent<GSB_EnemyController>();
        }

        return null;
    }

    void CheckBackgroundTouch(out Vector3 hitPosition)
    {
        hitPosition = Vector3.zero;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var layerMaskBackground = (1 << 10);
        if(Physics.Raycast(ray, out hit, float.MaxValue, layerMaskBackground))
        {
            hitPosition = hit.point;
            hitPosition.z = -0.15f;
        }
    }

    bool GenerateCollisionShapeFromEnemies()
    {
        if(GSB_SceneManager.Instance.SelectionMesh != null)
        {
            for(var i = 0; i < SelectingEnemies.Count; ++i)
            {
                _shapeVertices2D[i].x = SelectingEnemies[i].transform.position.x;
                _shapeVertices2D[i].y = SelectingEnemies[i].transform.position.y;
            }

            _triangulator.Init(_shapeVertices2D, SelectingEnemies.Count);
            int[] indices = _triangulator.Triangulate();

            Mesh shapeMesh = null;
            var setTriangles = false;

            if(GSB_SceneManager.Instance.SelectionMesh.sharedMesh == null)
            {
                setTriangles = true;
                shapeMesh = new Mesh();

                GSB_SceneManager.Instance.SelectionMesh.sharedMesh = shapeMesh;
            }
            else
            {
                shapeMesh = GSB_SceneManager.Instance.SelectionMesh.sharedMesh;
            }

            if(_shapeVertices3D == null || SelectingEnemies.Count > _shapeVertices3D.Length)
            {
                _shapeVertices3D = new Vector3[SelectingEnemies.Count];
            }

            for(int i = 0; i < SelectingEnemies.Count; i++)
            {
                _shapeVertices3D[i].x = _shapeVertices2D[i].x;
                _shapeVertices3D[i].y = _shapeVertices2D[i].y;
                _shapeVertices3D[i].z = -0.15f;
            }

            if(shapeMesh != null)
            {
                shapeMesh.vertices = _shapeVertices3D;
                if(setTriangles)
                {
                    shapeMesh.triangles = indices;
                }

                shapeMesh.RecalculateBounds();
            }

            if(indices.Length == 3 && SelectingEnemies.Count == 4)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    void CheckEnemiesInside(out List<GSB_EnemyController> enemiesInside)
    {
        enemiesInside = new List<GSB_EnemyController>();

        if(_triangulator != null)
        {
            for(var i = 0; i < GSB_SceneManager.Instance.Enemies.Count; ++i)
            {
                if(!SelectingEnemies.Contains(GSB_SceneManager.Instance.Enemies[i]))
                {
                    for(var j = 0; j < GSB_SceneManager.Instance.SelectionMesh.sharedMesh.triangles.Length / 3; ++j)
                    {
                        Vector3 A = GSB_SceneManager.Instance.SelectionMesh.sharedMesh.vertices[
                            GSB_SceneManager.Instance.SelectionMesh.sharedMesh.triangles[
                                (j * 3) + 0]];

                        Vector3 B = GSB_SceneManager.Instance.SelectionMesh.sharedMesh.vertices[
                            GSB_SceneManager.Instance.SelectionMesh.sharedMesh.triangles[
                                (j * 3) + 1]];

                        Vector3 C = GSB_SceneManager.Instance.SelectionMesh.sharedMesh.vertices[
                            GSB_SceneManager.Instance.SelectionMesh.sharedMesh.triangles[
                                (j * 3) + 2]];

                        Vector3 P = GSB_SceneManager.Instance.Enemies[i].transform.position;

                        if(_triangulator.InsideTriangle(A, B, C, P))
                        {
                            enemiesInside.Add(GSB_SceneManager.Instance.Enemies[i]);
                        }
                    }
                }
            }
        }
    }

    void AddPositionToSelectionLine(GSB_EnemyController enemy)
    {
        LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count - 1];
        currentLine.positionCount = 2;

        currentLine.SetPosition(0, enemy.transform.position);
        currentLine.SetPosition(1, enemy.transform.position);

        if(SelectingEnemies.Count > 1)
        {
            LineRenderer previousLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count - 2];
            previousLine.SetPosition(1, enemy.transform.position);
        }
    }

    void RemovePositionToSelectionLine()
    {
        LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count - 1];
        currentLine.positionCount = 0;
    }

    void UpdateLastPositionToSelectionLine(Vector3 position, bool forced = false)
    {
        LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count - 1];
        if(currentLine != null)
        {
            currentLine.SetPosition(1, position);
        }
    }

    void UpdatePositionToSelectionLines()
    {
        for(var i = 0; i < SelectingEnemies.Count; ++i)
        {
            LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[i];
            currentLine.SetPosition(0, SelectingEnemies[i].transform.position);

            if(i < SelectingEnemies.Count - 1)
            {
                currentLine.SetPosition(1, SelectingEnemies[i + 1].transform.position);
            }
        }

        if(_shapeIsClosed)
        {
            LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count - 1];
            currentLine.SetPosition(0, SelectingEnemies[SelectingEnemies.Count - 1].transform.position);
            currentLine.SetPosition(1, SelectingEnemies[0].transform.position);
        }
    }

    public void ShipHasBeenDestroyed(GSB_EnemyController enemy)
    {
        if(SelectingEnemies.Contains(enemy))
        {
            var enemyIdx = SelectingEnemies.IndexOf(enemy);

            if(_shapeIsClosed)
            {
                LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count - 1];
                currentLine.positionCount = 0;
            }

            for(var i = SelectingEnemies.Count - 1; i >= enemyIdx; i--)
            {
                SelectingEnemies[i].SetTargetEnabled(false, i == 0);
                SelectingEnemies.RemoveAt(i);
            }

            for(var i = SelectingEnemies.Count; i < GSB_SceneManager.Instance.AmmoMax; i++)
            {
                LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[i];
                currentLine.positionCount = 0;
            }

            if(SelectingEnemies.Count == 0)
            {
                _holding = false;

                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;

                ResetSelection();
            }

            if(GSB_SceneManager.Instance.SelectionMesh != null)
            {
                GSB_SceneManager.Instance.SelectionMesh.sharedMesh = null;
            }

            GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_cancel");

            _shapeIsClosed = false;
        }
    }

    void ResetSelection()
    {
        for(var i = 0; i < GSB_SceneManager.Instance.SelectionLine.Count; ++i)
        {
            GSB_SceneManager.Instance.SelectionLine[i].positionCount = 0;

            ChangeLastLineColor(new Color(0.2f, 0.486f, 0.745f, 1f), i);
        }

        if(GSB_SceneManager.Instance.SelectionMesh != null)
        {
            GSB_SceneManager.Instance.SelectionMesh.sharedMesh = null;
        }

        for(var i = 0; i < SelectingEnemies.Count; ++i)
        {
            SelectingEnemies[i].SetTargetEnabled(false, false);
        }

        SelectingEnemies.Clear();

        _currentTimePercentage = _currentTimePercentage - ((TimeUtils.TimestampMilliseconds - _timeProcessStartingTime) / (float)_timeProcessAvailableTime);

        _shapeIsClosed = false;
    }

    void StartTimeRecovering()
    {
        _timeProcessStartingTime = TimeUtils.TimestampMilliseconds;
        _timeProcessAvailableTime = (int)((1f - _currentTimePercentage) * GSB_SceneManager.Instance.TotalTimeRegeneration);

        UpdateTimeBarUI(_currentTimePercentage);
    }

    void ChangeLastLineColor(Color c, int lineIndex)
    {
        LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[lineIndex];
        if(currentLine != null)
        {
            currentLine.startColor = c;
            currentLine.endColor = c;
        }
    }

    void UpdateAmmoUI()
    {
        for(var i = 0; i < AmmoBCSH.Count; ++i)
        {
            var stateToSet = "disabled";

            if(i < _currentAmmo)
            {
                stateToSet = "default";

                if(_holding)
                {
                    if(i < SelectingEnemies.Count)
                    {
                        stateToSet = "bright";
                    }
                }
            }

            AmmoBCSH[i].ApplyBCSHState(stateToSet);
        }
    }

    void UpdateHealthUI()
    {
        for(var i = 0; i < HullGOs.Count; ++i)
        {
            HullGOs[i].SetActive(i < _currentHealth ? true : false);
        }
    }

    public void MakeDamage(int damage)
    {
        if(GSB_SceneManager.Instance.BattleSubState == GSB_SceneManager.EBattleState.E_WIN)
        {
            return;
        }

        if(GSB_SceneManager.Instance.BattleState != GSB_SceneManager.EBattleState.E_GAMEOVER)
        {
            _currentHealth -= damage;
            if(_currentHealth < 0)
            {
                _currentHealth = 0;
            }

            if(_currentHealth > GSB_SceneManager.Instance.HealthMax)
            {
                _currentHealth = GSB_SceneManager.Instance.HealthMax;
            }

            if(_currentHealth == 0)
            {
                GSB_SceneManager.Instance.ChangeState(GSB_SceneManager.EBattleState.E_GAMEOVER);
            }
        }

        if(damage > 0)
        {
            if(_tremblingAnimation != null)
            {
                _tremblingAnimation.Kill();
                _tremblingAnimation = null;
            }

            if(ShipTransform != null)
            {
                if(GSB_SceneManager.Instance.BattleState != GSB_SceneManager.EBattleState.E_GAMEOVER)
                {
                    ShipTransform.transform.localPosition = Vector3.zero;

                    _tremblingAnimation = DOTween.Sequence();
                    _tremblingAnimation.Append(ShipTransform
                                               .transform.DOLocalMove(new Vector3(0.06f, 0.0f, 0.0f),
                                                   500 / 1000.0f / 10f).SetLoops(5, LoopType.Yoyo));

                    _tremblingAnimation.Play();
                }
                else
                {
                    if(!_dying)
                    {
                        ShipTransform.transform.localPosition = Vector3.zero;
                        ShipTransform.transform.DOLocalMove(new Vector3(3.0f, -1.5f, 0.0f), 8f);

                        _dying = true;
                        _explosionTimer.Wait(0f);
                    }
                }
            }

            if(ShipBCSH != null)
            {
                ShipBCSH.ApplyBCSHStateProgressive("damaged", 0, 0f);
                ShipBCSH.ApplyBCSHStateProgressive("default", 0, 0.4f);
            }
        }

        UpdateHealthUI();
    }

    void ShootToEnemies()
    {
        EnemiesToShoot.Clear();

        if(ShipShoots != null)
        {
            EnemiesToShoot.AddRange(SelectingEnemies);
            EnemiesInside.Clear();

            if(_shapeIsClosed)
            {
                CheckEnemiesInside(out EnemiesInside);
                EnemiesToShoot.AddRange(EnemiesInside);
            }
        }

        if(GSB_GameManager.Instance.CurrentGameState == GSB_GameManager.GameState.E_PLAYING_2_VERSUS)
        {
            if(GSB_GameManager.Instance.NetworkController.PlayerOnlineController != null)
            {
                GSB_GameManager.Instance.NetworkController.PlayerOnlineController.CmdSendShips(GSB_GameManager.Instance.NetworkController.PlayerControllerId, EnemiesToShoot.Count);
            }
        }

        var extraCombos = 0;
        Vector3 comboUICenter = Vector3.zero;
        for(var j = 0; j < SelectingEnemies.Count; ++j)
        {
            comboUICenter += SelectingEnemies[j].transform.position;
        }

        comboUICenter /= SelectingEnemies.Count;

        _ammoAsCombinationReward = EnemiesInside.Count * GSB_SceneManager.Instance.FullShapeEnergyRecoverMultiplier;
        if(_ammoAsCombinationReward > 0)
        {
            if(GSB_SceneManager.Instance.WorldUIParent != null && GSB_SceneManager.Instance.WorldUICombo != null)
            {
                GameObject comboUI = Instantiate(GSB_SceneManager.Instance.WorldUICombo);
                if(comboUI != null)
                {
                    comboUI.transform.SetParent(GSB_SceneManager.Instance.WorldUIParent.transform, false);
                    comboUI.transform.position = comboUICenter + (-Vector3.up * extraCombos);
                    extraCombos++;

                    GSB_Combo comboScript = comboUI.GetComponent<GSB_Combo>();
                    if(comboScript != null)
                    {
                        comboScript.SetComboTypeAndData(GSB_Combo.EComboType.E_COMBO_SHAPE, _ammoAsCombinationReward,_ammoAsCombinationReward);
                    }
                }
            }
        }

        for(var i = 0; i < 4; ++i)
        {
            var sameColorAmount = 0;

            for(var j = 0; j < SelectingEnemies.Count; ++j)
            {
                if(SelectingEnemies[j].ShipType == (GSB_EnemyController.EShipType)i)
                {
                    sameColorAmount++;
                }
            }

            for(var j = GSB_SceneManager.Instance.CombinationRepeatDatas.Count - 1; j >= 0; j--)
            {
                if(sameColorAmount >= GSB_SceneManager.Instance.CombinationRepeatDatas[j].ShipColorRepeatAmount)
                {
                    _ammoAsCombinationReward += GSB_SceneManager.Instance.CombinationRepeatDatas[j].ShipColorRepeatAmmoReward;

                    if(GSB_SceneManager.Instance.WorldUIParent != null && GSB_SceneManager.Instance.WorldUICombo != null)
                    {
                        GameObject comboUI = Instantiate(GSB_SceneManager.Instance.WorldUICombo);
                        if(comboUI != null)
                        {
                            comboUI.transform.SetParent(GSB_SceneManager.Instance.WorldUIParent.transform, false);
                            comboUI.transform.position = comboUICenter + (-Vector3.up * extraCombos);
                            extraCombos++;

                            GSB_Combo comboScript = comboUI.GetComponent<GSB_Combo>();
                            if(comboScript != null)
                            {
                                comboScript.SetComboTypeAndData(GSB_Combo.EComboType.E_COMBO_AMOUNT, GSB_SceneManager.Instance.CombinationRepeatDatas[j].ShipColorRepeatAmmoReward, sameColorAmount);
                                comboScript.AddComboUniqueShip((GSB_EnemyController.EShipType)i);
                            }
                        }
                    }

                    break;
                }
            }
        }

        List<bool> uniqueTypes = new List<bool>();
        for(var i = 0; i < 4; ++i)
        {
            uniqueTypes.Add(false);
        }

        var accumulatedUniqueness = -1;

        for(var i = 0; i < SelectingEnemies.Count; ++i)
        {
            if(uniqueTypes[(int)SelectingEnemies[i].ShipType] == false)
            {
                uniqueTypes[(int)SelectingEnemies[i].ShipType] = true;
                accumulatedUniqueness++;
            }
        }

        /*
        List<bool> uniqueTypes = new List<bool>();
        for(var i = 0; i < 4; ++i)
        {
            uniqueTypes.Add(false);
        }

        var accumulatedUniqueness = -1;

        for(var i = 0; i < SelectingEnemies.Count; ++i)
        {
            var unique = true;
            for(var j = 0; j < SelectingEnemies.Count; ++j)
            {
                if(SelectingEnemies[i] != SelectingEnemies[j])
                {
                    if(SelectingEnemies[i].ShipType == SelectingEnemies[j].ShipType)
                    {
                        unique = false;
                    }
                }
            }

            if(unique)
            {
                uniqueTypes[(int)SelectingEnemies[i].ShipType] = true;

                accumulatedUniqueness++;
            }
        }
        */

        if(accumulatedUniqueness >= 0 && GSB_SceneManager.Instance.CombinationDatas[accumulatedUniqueness].ShipColorUniqueAmmoReward > 0)
        {
            _ammoAsCombinationReward += GSB_SceneManager.Instance.CombinationDatas[accumulatedUniqueness].ShipColorUniqueAmmoReward;

            if(GSB_SceneManager.Instance.WorldUIParent != null && GSB_SceneManager.Instance.WorldUICombo != null)
            {
                GameObject comboUI = Instantiate(GSB_SceneManager.Instance.WorldUICombo);
                if(comboUI != null)
                {
                    comboUI.transform.SetParent(GSB_SceneManager.Instance.WorldUIParent.transform, false);
                    comboUI.transform.position = comboUICenter + (-Vector3.up * extraCombos);
                    extraCombos++;

                    GSB_Combo comboScript = comboUI.GetComponent<GSB_Combo>();
                    if(comboScript != null)
                    {
                        comboScript.SetComboTypeAndData(GSB_Combo.EComboType.E_COMBO_UNIQUES, GSB_SceneManager.Instance.CombinationDatas[accumulatedUniqueness] .ShipColorUniqueAmmoReward);

                        for(var i = 0; i < uniqueTypes.Count; ++i)
                        {
                            if(uniqueTypes[i])
                            {
                                comboScript.AddComboUniqueShip((GSB_EnemyController.EShipType)i);
                            }
                        }
                    }
                }
            }
        }

        if(extraCombos > 0)
        {
            GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_ammoreward");
        }

        _shootToEnemyIdx = 0;
        _shootTimer.Wait(0f);
        _shooting = true;
    }

    void GenerateShootToTarget(GSB_EnemyController target)
    {
        GameObject shoot = Instantiate(ShipShoots);
        if(shoot != null)
        {
            GSB_Shoot shootScript = shoot.GetComponentInChildren<GSB_Shoot>();
            if(shootScript != null)
            {
                shootScript.OriginPosition = new Vector3(0.48f, -4.02f, -0.12f);
                shootScript.DestPosition = target.transform.position;
                shootScript.TimeTravel = GSB_SceneManager.Instance.ShootStopTime;
                shootScript.TargetEnemy = target;
            }
        }

        _shootTimer.Wait(GSB_SceneManager.Instance.ShootStopTime / 1000f / EnemiesToShoot.Count);

        GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_canon");

        _shootToEnemyIdx++;
    }

    public void GenerateManaParticles(Vector3 origin)
    {
        GameObject mana = Instantiate(GSB_SceneManager.Instance.ManaGO);
        if(mana != null)
        {
            GSB_Mana manaScript = mana.GetComponentInChildren<GSB_Mana>();
            if(manaScript != null)
            {
                manaScript.OriginPosition = origin;
                manaScript.DestPosition = new Vector3(1.7f, 4.23f, -0.15f);
                manaScript.TimeTravel = GSB_SceneManager.Instance.ManaTime;
            }
        }
    }

    void UpdateTimeBarUI(float delta)
    {
        if(GSB_SceneManager.Instance.TimeBarFiller != null)
        {
            _vecTemp = Vector3.one;

            if(delta < 0f)
            {
                delta = 0f;
            }
            if(delta >= 1f)
            {
                delta = 1f;
            }

            if(delta < GSB_SceneManager.Instance.FlagDownTimePercentage)
            {
                if(GSB_SceneManager.Instance.TimeBarFillerBCSH != null && GSB_SceneManager.Instance.TimeBarFillerBCSH.CurrentAppliedBCSHState != 0)
                {
                    GSB_SceneManager.Instance.TimeBarFillerBCSH.ApplyBCSHStateProgressive("default", 0, 0f);
                }
            }
            else
            {
                if(GSB_SceneManager.Instance.TimeBarFillerBCSH != null && GSB_SceneManager.Instance.TimeBarFillerBCSH.CurrentAppliedBCSHState != 1)
                {
                    GSB_SceneManager.Instance.TimeBarFillerBCSH.ApplyBCSHStateProgressive("hasenergy", 0, 0.2f);
                }
            }

            _vecTemp.x = delta;

            GSB_SceneManager.Instance.TimeBarFiller.transform.localScale = _vecTemp;
        }
    }

    void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            MakeDamage(1);
        }

        if(GSB_SceneManager.Instance.BattleSubState == GSB_SceneManager.EBattleState.E_WIN)
        {
            return;
        }

        if(_dying)
        {
            if(_explosionTimer.IsFinished)
            {
                if(Explosion != null)
                {
                    GameObject explosion = Instantiate(Explosion);
                    if(explosion != null)
                    {
                        explosion.transform.position = ShipTransform.transform.position + new Vector3(-2.5f + Random.Range(0f, 5f), 0.4f - Random.Range(0f, 0.4f), 0f);
                    }

                    GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_explosion", false, 0.2f);
                }

                _explosionTimer.Wait(0.2f);
            }

            return;
        }

        if(_currentAmmo < GSB_SceneManager.Instance.AmmoMax && !_shooting)
        {
            if(_ammoRefillTimer.IsFinished)
            {
                _currentAmmo++;
                UpdateAmmoUI();

                if (_currentAmmo == GSB_SceneManager.Instance.AmmoMax)
                {
                    GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_ammofull", false, 0.25f);
                }
                else
                {
                    GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_ammorecover", false, 0.25f);
                }

                for(var i = 0; i < GSB_SceneManager.Instance.SelectionLine.Count; ++i)
                {
                    ChangeLastLineColor(new Color(0.2f, 0.486f, 0.745f, 1f), i);
                }

                _ammoRefillTimer.Wait(GSB_SceneManager.Instance.AmmoRegenerationTime);
            }
        }

        if(!_holding && !_shooting)
        {
            var deltaRecoveringTime = 1f;
            if(_timeProcessAvailableTime > 0)
            {
                deltaRecoveringTime = _currentTimePercentage + (((TimeUtils.TimestampMilliseconds - _timeProcessStartingTime) / (float)_timeProcessAvailableTime) * (1f - _currentTimePercentage));
            }
            if(deltaRecoveringTime >= 1f)
            {
                deltaRecoveringTime = 1f;
            }
            UpdateTimeBarUI(deltaRecoveringTime);

            if(_pressedDown && deltaRecoveringTime >= GSB_SceneManager.Instance.FlagDownTimePercentage)
            {
                GSB_EnemyController enemyTouch = CheckEnemyTouch();
                if(enemyTouch != null && _currentAmmo > 0)
                {
                    ResetSelection();

                    SelectingEnemies.Add(enemyTouch);
                    enemyTouch.SetTargetEnabled(true, true);

                    AddPositionToSelectionLine(enemyTouch);

                    Time.timeScale = GSB_SceneManager.Instance.SlowDown;
                    Time.fixedDeltaTime = 0.02f * Time.timeScale;

                    _holding = true;

                    _timeProcessStartingTime = TimeUtils.TimestampMilliseconds;
                    _timeProcessAvailableTime = (int) (deltaRecoveringTime * GSB_SceneManager.Instance.TargetTimeMS);
                    _currentTimePercentage = deltaRecoveringTime - GSB_SceneManager.Instance.FlagDownTimePercentage;

                    _shapeIsClosed = false;

                    UpdateAmmoUI();
                }
            }
        }

        if(_holding)
        {
            UpdatePositionToSelectionLines();

            if(_shapeIsClosed)
            {
                _shapeIsClosed = GenerateCollisionShapeFromEnemies();
            }

            var deltaHolding = _currentTimePercentage - ((TimeUtils.TimestampMilliseconds - _timeProcessStartingTime) / (float)_timeProcessAvailableTime);
            UpdateTimeBarUI(deltaHolding);

            Vector3 touchBackground = Vector3.zero;
            CheckBackgroundTouch(out touchBackground);

            if(touchBackground != Vector3.zero)
            {
                if(!_shapeIsClosed)
                {
                    UpdateLastPositionToSelectionLine(touchBackground);
                }

                GSB_EnemyController enemyTouch = CheckLineCrossingEnemies();
                if(enemyTouch != null && !_shapeIsClosed)
                {
                    if(!SelectingEnemies.Contains(enemyTouch))
                    {
                        if(SelectingEnemies.Count < _currentAmmo)
                        {
                            SelectingEnemies.Add(enemyTouch);
                            enemyTouch.SetTargetEnabled(true, false);

                            AddPositionToSelectionLine(enemyTouch);

                            UpdateAmmoUI();
                        }
                        else
                        {
                            if(!_shapeIsClosed)
                            {
                                ChangeLastLineColor(Color.red, _currentAmmo-1);
                            }
                        }
                    }
                    else
                    {
                        if(SelectingEnemies.Count > 2)
                        {
                            if(enemyTouch == SelectingEnemies[0])
                            {
                                _shapeIsClosed = GenerateCollisionShapeFromEnemies();

                                if(_shapeIsClosed)
                                {
                                    GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_closeshape");

                                    UpdateLastPositionToSelectionLine(enemyTouch.transform.position, true);

                                    ChangeLastLineColor(new Color(0.2f, 0.486f, 0.745f, 1f), _currentAmmo-1);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if(!_shapeIsClosed && SelectingEnemies.Count == _currentAmmo)
                    {
                        ChangeLastLineColor(Color.yellow, _currentAmmo-1);
                    }
                }

                enemyTouch = CheckEnemyTouch();
                if(enemyTouch != null)
                {
                    if(SelectingEnemies.Contains(enemyTouch))
                    {
                        if(SelectingEnemies.Count >= 2)
                        {
                            var removeLastLine = false;
                            var removeSelectedShip = true;

                            if(_shapeIsClosed)
                            {
                                if(enemyTouch == SelectingEnemies[SelectingEnemies.Count - 1])
                                {
                                    removeLastLine = true;
                                    removeSelectedShip = false;
                                }
                            }
                            else
                            {
                                if(enemyTouch == SelectingEnemies[SelectingEnemies.Count - 2])
                                {
                                    removeLastLine = true;
                                }
                            }

                            if (removeLastLine)
                            {
                                if(GSB_SceneManager.Instance.SelectionMesh != null)
                                {
                                    GSB_SceneManager.Instance.SelectionMesh.sharedMesh = null;
                                }

                                if(removeSelectedShip)
                                {
                                    GameAudioManager.SharedInstance.PlaySound("Audio/Sounds/GSB_cancel");

                                    RemovePositionToSelectionLine();
                                    SelectingEnemies[SelectingEnemies.Count-1].SetTargetEnabled(false, false);
                                    SelectingEnemies.RemoveAt(SelectingEnemies.Count - 1);

                                    UpdateAmmoUI();
                                }

                                _shapeIsClosed = false;
                            }
                        }
                    }
                }
            }

            if((!_pressedDown && _pressedUp) || (_holding && deltaHolding <= 0f))
            {
                if(_holding)
                {
                    ShootToEnemies();

                    Time.timeScale = 1f;
                    Time.fixedDeltaTime = 0.02f * Time.timeScale;

                    _ammoWasted = SelectingEnemies.Count;

                    ResetSelection();

                    _holding = false;
                }
            }
        }

        if(_shooting)
        {
            if(_shootTimer.IsFinished)
            {
                if(_shootToEnemyIdx < EnemiesToShoot.Count)
                {
                    GenerateShootToTarget(EnemiesToShoot[_shootToEnemyIdx]);

                    if(_ammoWasted > 0)
                    {
                        _currentAmmo--;
                        _ammoWasted--;

                        UpdateAmmoUI();
                    }
                }
                else
                {
                    var allDestroyed = true;
                    for(var i = 0; i < EnemiesToShoot.Count; ++i)
                    {
                        if(EnemiesToShoot[i] != null)
                        {
                            allDestroyed = false;
                            break;
                        }
                    }

                    if(allDestroyed)
                    {
                        EnemiesToShoot.Clear();

                        _currentTimePercentage += _ammoAsCombinationReward * GSB_SceneManager.Instance.EnergyRecoverPercentage;
                        if(_currentTimePercentage > 1f)
                        {
                            _currentTimePercentage = 1f;
                        }

                        /*
                        _currentAmmo += _ammoAsCombinationReward;
                        if(_currentAmmo > GSB_SceneManager.Instance.AmmoMax)
                        {
                            _currentAmmo = GSB_SceneManager.Instance.AmmoMax;
                        }
                        UpdateAmmoUI();
                        */

                        _ammoRefillTimer.Wait(GSB_SceneManager.Instance.AmmoRegenerationTime);

                        StartTimeRecovering();
                        _shooting = false;
                    }
                }
            }
        }

        _pressedDown = false;
        _pressedUp = false;
    }
}
