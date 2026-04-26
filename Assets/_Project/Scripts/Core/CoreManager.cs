using UnityEngine;

public class CoreManager : MonoBehaviour
{
    public static CoreManager Instance { get; private set; }

    private void Awake()
    {
        // ΩÃ±€≈Ê ºº∆√
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
}
