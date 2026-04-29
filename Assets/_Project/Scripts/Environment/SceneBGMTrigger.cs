using UnityEngine;
public class SceneBGMTrigger : MonoBehaviour
{
    [SerializeField] private string bgmName = "";

    private void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance?.PlayBGM(bgmName);
        else
            Debug.Log("AudioManager.Instance 를 찾지 못하였습니다.");
    }
}