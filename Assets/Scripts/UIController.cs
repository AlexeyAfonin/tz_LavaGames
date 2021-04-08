using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _infoPanel;
    public GameObject losePanel;

    [Header ("Texts")]
    public Text textMessage;

    [Header("Parametrs")]
    [SerializeField, Range(0.5f, 30f)] private float _timeHideMessage = 1f;

    private void Start()
    {
        Time.timeScale = 1f;
    }

    private void Update()
    {
        _infoPanel.SetActive(textMessage.gameObject.activeSelf);

        if (textMessage.gameObject.activeSelf)
        {
            StartCoroutine(TimerHideUIElement(textMessage.gameObject, _timeHideMessage));
        }
        
    }

    /// <summary>
    /// Скрывает UI элемент через указанное время
    /// </summary>
    /// <param name="element">Элемент, который будет скрыт</param>
    /// <param name="time">Время (в секундах), через которое будет скрыт элемент</param>
    /// <returns></returns>
    private IEnumerator TimerHideUIElement(GameObject element, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        element.SetActive(false);
        StopCoroutine(TimerHideUIElement(element, time));
    }

    /// <summary>
    /// Перезапускает активный уровень (активную сцену)
    /// </summary>
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Выходит из приложения
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }
}
