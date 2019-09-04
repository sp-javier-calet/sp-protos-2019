
using System.Collections.Generic;
using SocialPoint.Rendering.Components;
using SocialPoint.Utils;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GSB_PlayerController : MonoBehaviour
{
    bool _pressedDown = false;
    bool _pressedUp = false;

    Triangulator _triangulator = null;
    bool _holding = false;
    long _holdingStartTime = 0;
    bool _shapeIsClosed = false;

    List<GSB_EnemyController> SelectingEnemies = new List<GSB_EnemyController>();
    List<GSB_EnemyController> ExplodingEnemies = new List<GSB_EnemyController>();
    List<GSB_EnemyController> EnemiesInside = new List<GSB_EnemyController>();

    List<GameObject> HullGOs = new List<GameObject>();
    List<BCSHModifier> AmmoBCSH = new List<BCSHModifier>();

    List<GSB_EnemyController> CurrentEnemies = new List<GSB_EnemyController>();

    Vector3 _vecTemp = Vector3.zero;
    Timer _ammoRefillTimer = new Timer();
    int _currentAmmo = -1;

    void Awake()
    {
        /*
        if(GSB_GameManager.Instance.CurrentGameState == CP_GameManager.GameState.E_PLAYING_1_PLAYER)
        {
            Init();
        }
        */

        CurrentEnemies.AddRange(FindObjectsOfType<GSB_EnemyController>());
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
    }

    public void Init()
    {
    }

    void OnTriggerEnter(Collider other)
    {

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
            Vector2[] vertices2D = new Vector2[SelectingEnemies.Count];
            for(var i = 0; i < SelectingEnemies.Count; ++i)
            {
                vertices2D[i] = new Vector2(SelectingEnemies[i].transform.position.x, SelectingEnemies[i].transform.position.y);
            }

            _triangulator = new Triangulator(vertices2D);
            int[] indices = _triangulator.Triangulate();

            Vector3[] vertices = new Vector3[vertices2D.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices2D[i].x,vertices2D[i].y, -0.15f);
            }

            if(GSB_SceneManager.Instance.SelectionMesh.sharedMesh == null)
            {
                Mesh newMesh = new Mesh();
                newMesh.vertices = vertices;
                newMesh.triangles = indices;
                newMesh.RecalculateBounds();

                GSB_SceneManager.Instance.SelectionMesh.sharedMesh = newMesh;
            }
            else
            {
                GSB_SceneManager.Instance.SelectionMesh.sharedMesh.vertices = vertices;
                GSB_SceneManager.Instance.SelectionMesh.sharedMesh.RecalculateBounds();
            }

            if(indices.Length == 3 && SelectingEnemies.Count == 4)
            {
                _triangulator = null;

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
            for(var i = 0; i < CurrentEnemies.Count; ++i)
            {
                if(!SelectingEnemies.Contains(CurrentEnemies[i]))
                {
                    for(var j = 0; j < GSB_SceneManager.Instance.SelectionMesh.sharedMesh.triangles.Length / 3; ++j)
                    {
                        Vector3 A = GSB_SceneManager.Instance.SelectionMesh.sharedMesh.vertices[GSB_SceneManager.Instance.SelectionMesh.sharedMesh.triangles[(j*3)+0]];
                        Vector3 B = GSB_SceneManager.Instance.SelectionMesh.sharedMesh.vertices[GSB_SceneManager.Instance.SelectionMesh.sharedMesh.triangles[(j*3)+1]];
                        Vector3 C = GSB_SceneManager.Instance.SelectionMesh.sharedMesh.vertices[GSB_SceneManager.Instance.SelectionMesh.sharedMesh.triangles[(j*3)+2]];
                        Vector3 P = CurrentEnemies[i].transform.position;

                        if(_triangulator.InsideTriangle(A, B, C, P))
                        {
                            Debug.Log("inside: " + CurrentEnemies[i].name);
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

    void ResetSelection()
    {
        for(var i = 0; i < GSB_SceneManager.Instance.SelectionLine.Count; ++i)
        {
            GSB_SceneManager.Instance.SelectionLine[i].positionCount = 0;
        }

        if(GSB_SceneManager.Instance.SelectionMesh != null)
        {
            GSB_SceneManager.Instance.SelectionMesh.sharedMesh = null;
        }

        for(var i = 0; i < SelectingEnemies.Count; ++i)
        {
            SelectingEnemies[i].SetTargetEnabled(false);
        }
        SelectingEnemies.Clear();

        if(GSB_SceneManager.Instance.TimeBarFiller != null)
        {
            GSB_SceneManager.Instance.TimeBarFiller.transform.localScale = Vector3.one;
        }
    }

    void ChangeLastLineColor(Color c)
    {
        LineRenderer currentLine = GSB_SceneManager.Instance.SelectionLine[GSB_SceneManager.Instance.SelectionLine.Count-1];
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

    void LateUpdate()
    {
        if(_currentAmmo < GSB_SceneManager.Instance.AmmoMax)
        {
            if(_ammoRefillTimer.IsFinished)
            {
                _currentAmmo++;
                UpdateAmmoUI();

                _ammoRefillTimer.Wait(GSB_SceneManager.Instance.AmmoRegenerationTime);
            }
        }

        if(!_holding)
        {
            if(_pressedDown)
            {
                GSB_EnemyController enemyTouch = CheckEnemyTouch();
                if(enemyTouch != null && _currentAmmo > 0)
                {
                    ResetSelection();

                    SelectingEnemies.Add(enemyTouch);
                    enemyTouch.SetTargetEnabled(true);

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
                if(enemyTouch != null)
                {
                    if(!SelectingEnemies.Contains(enemyTouch))
                    {
                        if(SelectingEnemies.Count < _currentAmmo)
                        {
                            SelectingEnemies.Add(enemyTouch);
                            enemyTouch.SetTargetEnabled(true);

                            AddPositionToSelectionLine(enemyTouch);

                            UpdateAmmoUI();
                        }
                        else
                        {
                            if(!_shapeIsClosed)
                            {
                                ChangeLastLineColor(Color.red);
                            }
                        }
                    }
                    else
                    {
                        if(SelectingEnemies.Count > 2)
                        {
                            if(enemyTouch == SelectingEnemies[0] && !_shapeIsClosed)
                            {
                                _shapeIsClosed = GenerateCollisionShapeFromEnemies();

                                if(_shapeIsClosed)
                                {
                                    UpdateLastPositionToSelectionLine(enemyTouch.transform.position, true);

                                    ChangeLastLineColor(new Color(0.2f, 0.486f, 0.745f, 1f));
                                }
                            }
                        }
                    }
                }
                else
                {
                    if(!_shapeIsClosed && SelectingEnemies.Count == _currentAmmo)
                    {
                        ChangeLastLineColor(Color.yellow);
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
                                    SelectingEnemies[SelectingEnemies.Count-1].SetTargetEnabled(false);
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
                _currentAmmo -= SelectingEnemies.Count;
                _ammoRefillTimer.Wait(GSB_SceneManager.Instance.AmmoRegenerationTime);

                ResetSelection();
                UpdateAmmoUI();

                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;

                _holding = false;
            }
        }

        _pressedDown = false;
        _pressedUp = false;
    }
}
