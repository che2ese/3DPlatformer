using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingProcess : MonoBehaviour
{
    public TMP_Text ProgressIndicator; // ����� �ؽ�Ʈ
    public Image LoadingBar;           // �ε� ��
    public float currentValue = 0f;   // ���� ���� ��
    private float speed = 20f;                // ���� �ӵ�
    public GameObject Loading;         // �ε� �г�

    // Start is called before the first frame update
    void Start()
    {
        ResetLoading(); // ���� �� �ε� ���� �ʱ�ȭ
    }

    // Update is called once per frame
    void Update()
    {
        if (currentValue < 100)
        {
            currentValue += speed * Time.deltaTime;
            ProgressIndicator.text = ((int)currentValue).ToString() + "%";
        }
        else
        {
            ProgressIndicator.text = "Done";

            if (Loading != null && Loading.activeSelf)
            {
                ResetLoading(); // �ε� ���� �ʱ�ȭ
                Loading.SetActive(false); // �ε� �г� ��Ȱ��ȭ
            }
        }

        LoadingBar.fillAmount = currentValue / 100;
    }

    // �ε� ���� �ʱ�ȭ
    public void ResetLoading()
    {
        currentValue = 0f; // ���� �� �ʱ�ȭ
        if (Loading != null)
        {
            Loading.SetActive(true); // �ε� �г� Ȱ��ȭ
        }

        if (ProgressIndicator != null)
        {
            ProgressIndicator.text = "0%"; // �ʱ� ����� ǥ��
        }

        if (LoadingBar != null)
        {
            LoadingBar.fillAmount = 0f; // �ε� �� �ʱ�ȭ
        }
    }
}
