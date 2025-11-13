using UnityEngine;

public class ActiveWatcher : MonoBehaviour
{
  void OnEnable()
  {
    Debug.Log($"{name} was ENABLED at {Time.time}");
  }

  void OnDisable()
  {
    Debug.Log($"{name} was DISABLED at {Time.time}");
  }
}