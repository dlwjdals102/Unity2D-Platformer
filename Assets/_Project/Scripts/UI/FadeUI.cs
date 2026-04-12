using UnityEngine;
using UnityEngine.UI;

// 이 스크립트를 넣으면 Image 컴포넌트가 무조건 같이 붙습니다.
[RequireComponent(typeof(Image))]
public class FadeUI : MonoBehaviour
{
    public Image fadeImage { get; private set; }

    private void Awake()
    {
        fadeImage = GetComponent<Image>();
    }
}