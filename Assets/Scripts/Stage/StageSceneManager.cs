using UnityEngine;

public class StageSceneManager : MonoBehaviour
{
    public GameObject[] stageObjects;

    void Start()
    {
        string targetStage = StageManager.selectedStageName;

        foreach (GameObject obj in stageObjects)
        {
            if (obj.name == targetStage)
            {
                obj.SetActive(true);
            }
            else
            {
                obj.SetActive(false);
            }
        }
    }
}
