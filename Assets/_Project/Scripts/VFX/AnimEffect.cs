using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class AnimEffect : MonoBehaviour
{
    /// <summary>
    /// 애니메이션의 가장 마지막 프레임에 Animation Event로 이 함수를 달아줍니다!
    /// </summary>
    public void DeactivateEffect()
    {
        // 스스로를 끄면 ObjectPoolManager가 알아서 대기열로 회수합니다.
        gameObject.SetActive(false);
    }
}