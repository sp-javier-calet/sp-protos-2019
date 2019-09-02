
using UnityEngine;

public class CG_SceneManager : MonoBehaviour
{
    public GameObject EnvironmentGO = null;

    const int kNumStagesByColumn = 2;
    const int kNumStagesByRow = 4;

    void Awake()
    {
        if(EnvironmentGO != null)
        {
            var currentPosX = 0f;
            var currentPosY = 0f;
            for(var i = 0; i < kNumStagesByColumn; ++i)
            {
                currentPosX = 0f;

                for(var j = 0; j < kNumStagesByRow; ++j)
                {
                    GameObject newStage = Instantiate(EnvironmentGO);
                    newStage.transform.position = new Vector3(currentPosX, currentPosY, 0f);

                    currentPosX += 36f;
                }

                currentPosY -= 28f;
            }
        }
    }
}
