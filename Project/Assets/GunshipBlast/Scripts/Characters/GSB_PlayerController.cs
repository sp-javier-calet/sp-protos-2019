
using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEngine;

public class GSB_PlayerController : MonoBehaviour
{
    bool _pressedDown = false;
    bool _pressedUp = false;

    Triangulator _triangulator = null;
    bool _holding = false;
    long _holdingStartTime = 0;

    public List<GSB_EnemyController> SelectingEnemies = new List<GSB_EnemyController>();
    public List<GSB_EnemyController> EnemiesInside = new List<GSB_EnemyController>();

    List<GSB_EnemyController> CurrentEnemies = new List<GSB_EnemyController>();

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
                vertices2D[i] = new Vector2(SelectingEnemies[i].transform.position.x, SelectingEnemies[i].transform.position.z);
            }

            _triangulator = new Triangulator(vertices2D);
            int[] indices = _triangulator.Triangulate();
            Debug.Log("indices: " + indices.Length);

            Vector3[] vertices = new Vector3[vertices2D.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices2D[i].x,0, vertices2D[i].y);
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
        if(GSB_SceneManager.Instance.SelectionLine != null)
        {
            if(enemy != null)
            {
                if(GSB_SceneManager.Instance.SelectionLine.positionCount == 0)
                {
                    GSB_SceneManager.Instance.SelectionLine.positionCount++;
                }
                GSB_SceneManager.Instance.SelectionLine.SetPosition(GSB_SceneManager.Instance.SelectionLine.positionCount-1, enemy.transform.position);

                GSB_SceneManager.Instance.SelectionLine.positionCount++;
                GSB_SceneManager.Instance.SelectionLine.SetPosition(GSB_SceneManager.Instance.SelectionLine.positionCount-1, enemy.transform.position);
            }
        }
    }

    void UpdateLastPositionToSelectionLine(Vector3 position)
    {
        if(GSB_SceneManager.Instance.SelectionLine != null && GSB_SceneManager.Instance.SelectionLine.positionCount > 1)
        {
            GSB_SceneManager.Instance.SelectionLine.SetPosition(GSB_SceneManager.Instance.SelectionLine.positionCount-1, position);
        }
    }

    void LateUpdate()
    {
        if(!_holding)
        {
            if(_pressedDown)
            {
                GSB_EnemyController enemyTouch = CheckEnemyTouch();
                if(enemyTouch != null)
                {
                    Debug.Log("enemyTouch: " + enemyTouch.name);

                    if(GSB_SceneManager.Instance.SelectionLine != null)
                    {
                        GSB_SceneManager.Instance.SelectionLine.positionCount = 0;
                    }

                    AddPositionToSelectionLine(enemyTouch);

                    SelectingEnemies.Clear();
                    SelectingEnemies.Add(enemyTouch);

                    Time.timeScale = GSB_SceneManager.Instance.SlowDown;
                    Time.fixedDeltaTime = 0.02f * Time.timeScale;

                    _holding = true;
                    _holdingStartTime = TimeUtils.TimestampMilliseconds;
                }
            }
        }

        if(_holding)
        {
            Vector3 touchBackground = Vector3.zero;
            CheckBackgroundTouch(out touchBackground);

            if(touchBackground != Vector3.zero)
            {
                UpdateLastPositionToSelectionLine(touchBackground);
            }

            /*
            GSB_EnemyController enemyTouch = CheckEnemyTouch();
            if(enemyTouch != null)
            {
                if(!SelectingEnemies.Contains(enemyTouch) && SelectingEnemies.Count < 4)
                {
                    Debug.Log("enemyTouch: " + enemyTouch.name);

                    AddPositionToSelectionLine(enemyTouch);

                    SelectingEnemies.Add(enemyTouch);
                }
                else
                {
                    if(SelectingEnemies.Count > 2)
                    {
                        if(enemyTouch == SelectingEnemies[0])
                        {
                            bool isClosedMesh = GenerateCollisionShapeFromEnemies();
                            if(isClosedMesh)
                            {
                                CheckEnemiesInside(out EnemiesInside);

                                for(var i = 0; i < EnemiesInside.Count; ++i)
                                {
                                    Debug.Log(EnemiesInside[i].name);
                                }
                            }

                            Debug.Log("isClosedMesh: " + isClosedMesh);
                        }
                    }
                }
            }
            */
        }

        if(!_pressedDown && _pressedUp)
        {
            if(GSB_SceneManager.Instance.SelectionMesh != null)
            {
                GSB_SceneManager.Instance.SelectionMesh.sharedMesh = null;
            }

            SelectingEnemies.Clear();

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            _holding = false;
        }

        /*
        var dist = 0f;
        if(GetHitDistance(out dist, out _hitDown, transform.position, -Vector3.up, 0.1f))
        {
            if(_playerState == PlayerState.E_JUMPING_FALL || _playerState == PlayerState.E_DAMAGED_FALL)
            {
                if(_rigidBody != null)
                {
                    _rigidBody.velocity = Vector3.zero;
                }

                if(_playerState == PlayerState.E_DAMAGED_FALL)
                {
                    AfterDamage();
                }

                if(_sceneManager.CurrentBattleState == CP_SceneManager.BattleState.E_PLAYING)
                {
                    if(_playerState == PlayerState.E_JUMPING_FALL && _memoryJump)
                    {
                        _hasDoubleHump = false;
                        _memoryJump = false;

                        Jump();
                    }
                    else
                    {
                        Walk();
                    }
                }
            }
        }
        */

        _pressedDown = false;
        _pressedUp = false;
    }
}
