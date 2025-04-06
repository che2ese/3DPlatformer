using UnityEngine;
using UnityEngine.SceneManagement;

public class StagePortal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Enter with: " + other.name); // 추가!
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Entered: " + gameObject.name); // 추가!
            StageManager.selectedStageName = gameObject.name;
            SceneManager.LoadScene("StageScene");
        }
    }

}
