using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    //When instantiated call method sending coords as parameter
    private void Awake() => SpawnSystem.AddSpawnPoint(transform);

    //When destroyed call method sending coords as parameter
    private void OnDestroy() => SpawnSystem.RemoveSpawnPoint(transform);
}