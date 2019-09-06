﻿
using System.Collections.Generic;
using SocialPoint.Rendering.Components;
using SocialPoint.Utils;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using DG.Tweening;

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
    long _holdingStartTime = 0;
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
    int _differentColorsExploding = 0;
    int _currentHealth = -1;
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
    }

    bool GetHitDistance(out float distance, out RaycastHit hit, Vector3 initPosition, Vector3 direction, float maxDistance = 0.0001f, int layerMask = 0)
    {
        distance = 0f;

        Ray downRay = new Ray(initPosition, direction);
        if (Physics.Raycast(downRay, out hit, maxDistance, layerMask))
        {
            distance = hit.distance;
            return true;
        }

        return false;
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
        if (Physics.Raycast(ray, out hit, float.MaxValue, layerMaskEnemies))
        {
            return hit.transform.GetComponent<GSB_EnemyController>();
        }

        return null;
    }

    GSB_EnemyController CheckLineCrossingEnemies()
    {
        LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count-1];

        Vector3 enemyToLineRay = currentLine.GetPosition(1) - SelectingEnemies[SelectingEnemies.Count-1].transform.position;
        var distance = enemyToLineRay.magnitude;

        RaycastHit hit;
        var layerMaskEnemies = (1 << 19);
        if (Physics.Raycast(SelectingEnemies[SelectingEnemies.Count-1].transform.position, enemyToLineRay.normalized, out hit, distance, layerMaskEnemies))
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
        if (Physics.Raycast(ray, out hit, float.MaxValue, layerMaskBackground))
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

            for (int i = 0; i < SelectingEnemies.Count; i++)
            {
                _shapeVertices3D[i].x = _shapeVertices2D[i].x;
                _shapeVertices3D[i].y = _shapeVertices2D[i].y;
                _shapeVertices3D[i].z = -0.15f;
            }

            if (shapeMesh != null)
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
                        Vector3 A = GSB_SceneManager.Instance.SelectionMesh.sharedMesh.vertices[GSB_SceneManager.Instance.SelectionMesh.sharedMesh.triangles[(j*3)+0]];
                        Vector3 B = GSB_SceneManager.Instance.SelectionMesh.sharedMesh.vertices[GSB_SceneManager.Instance.SelectionMesh.sharedMesh.triangles[(j*3)+1]];
                        Vector3 C = GSB_SceneManager.Instance.SelectionMesh.sharedMesh.vertices[GSB_SceneManager.Instance.SelectionMesh.sharedMesh.triangles[(j*3)+2]];
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
        LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count-1];
        currentLine.positionCount = 2;

        currentLine.SetPosition(0, enemy.transform.position);
        currentLine.SetPosition(1, enemy.transform.position);

        if(SelectingEnemies.Count > 1)
        {
            LineRenderer previousLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count-2];
            previousLine.SetPosition(1, enemy.transform.position);
        }
    }

    void RemovePositionToSelectionLine()
    {
        LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count-1];
        currentLine.positionCount = 0;
    }

    void UpdateLastPositionToSelectionLine(Vector3 position, bool forced = false)
    {
        LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count-1];
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
                currentLine.SetPosition(1, SelectingEnemies[i+1].transform.position);
            }
        }

        if (_shapeIsClosed)
        {
            LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[SelectingEnemies.Count-1];
            currentLine.SetPosition(0, SelectingEnemies[SelectingEnemies.Count-1].transform.position);
            currentLine.SetPosition(1, SelectingEnemies[0].transform.position);
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

        if(GSB_SceneManager.Instance.TimeBarFiller != null)
        {
            GSB_SceneManager.Instance.TimeBarFiller.transform.localScale = Vector3.one;
        }

        _shapeIsClosed = false;
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
                    _tremblingAnimation.Append(ShipTransform.transform.DOLocalMove(new Vector3(0.06f, 0.0f, 0.0f),500 / 1000.0f / 10f).SetLoops(5, LoopType.Yoyo));
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

            if(_shapeIsClosed)
            {
                CheckEnemiesInside(out EnemiesInside);
                EnemiesToShoot.AddRange(EnemiesInside);
            }
        }

        _differentColorsExploding = 0;
        for(var i = 0; i < 4; ++i)
        {
            for(var j = 0; j < EnemiesToShoot.Count; ++j)
            {
                if(EnemiesToShoot[j].ShipType == (GSB_EnemyController.EShipType)i)
                {
                    _differentColorsExploding++;

                    break;
                }
            }
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

        _shootToEnemyIdx++;
    }

    void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            MakeDamage(1);
        }

        if(_dying && _explosionTimer.IsFinished)
        {
            if(Explosion != null)
            {
                GameObject explosion = Instantiate(Explosion);
                if(explosion != null)
                {
                    explosion.transform.position = ShipTransform.transform.position + new Vector3(-2.5f + Random.Range(0f, 5f), 0.4f - Random.Range(0f, 0.4f), 0f);
                }
            }

            _explosionTimer.Wait(0.2f);
        }

        if(_currentAmmo < GSB_SceneManager.Instance.AmmoMax && !_shooting)
        {
            if(_ammoRefillTimer.IsFinished)
            {
                _currentAmmo++;
                UpdateAmmoUI();

                for(var i = 0; i < GSB_SceneManager.Instance.SelectionLine.Count; ++i)
                {
                    ChangeLastLineColor(new Color(0.2f, 0.486f, 0.745f, 1f), i);
                }

                _ammoRefillTimer.Wait(GSB_SceneManager.Instance.AmmoRegenerationTime);
            }
        }

        if(!_holding && !_shooting)
        {
            if(_pressedDown)
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
                    _holdingStartTime = TimeUtils.TimestampMilliseconds;

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
        }

        if(_holding)
        {
            var deltaHolding = (TimeUtils.TimestampMilliseconds - _holdingStartTime) / (float)GSB_SceneManager.Instance.TargetTimeMS;

            if(GSB_SceneManager.Instance.TimeBarFiller != null)
            {
                _vecTemp = Vector3.one;
                _vecTemp.x = 1f - deltaHolding;

                GSB_SceneManager.Instance.TimeBarFiller.transform.localScale = _vecTemp;
            }

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

            if((!_pressedDown && _pressedUp) || (_holding && deltaHolding >= 1f))
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

                        _ammoRefillTimer.Wait(GSB_SceneManager.Instance.AmmoRegenerationTime);

                        _shooting = false;
                    }
                }
            }
        }

        _pressedDown = false;
        _pressedUp = false;
    }
}
