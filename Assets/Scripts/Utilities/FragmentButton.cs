using UnityEngine;

public class FragmentButton : MonoBehaviour
{
  public string fragmentText;
  private bool isSelected = false;
  private Transform availableParent;
  private Transform selectedParent;
  private PassphraseFormingMicrogame manager;

  public void Initialize(PassphraseFormingMicrogame manager, Transform available, Transform selected)
  {
    this.manager = manager;
    availableParent = available;
    selectedParent = selected;
  }

  public void OnClick()
  {
    if (isSelected)
    {
      // Return to pool
      transform.SetParent(availableParent);
      isSelected = false;
      // manager.UpdatePassword(fragmentText);
    }
    else
    {
      // Move to selected area
      transform.SetParent(selectedParent);
      isSelected = true;
      // manager.UpdatePassword(fragmentText);
    }
  }
}
