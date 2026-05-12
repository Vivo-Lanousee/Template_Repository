using UnityEngine;
using VContainer;

//Playerの実データ
public class TestMono : MonoBehaviour
{
    [Inject]
    PlayerSystem player;

    private void Start()
    {
        player.Damage(10);
    }
}


