using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FrustrationTermometer", menuName = "GameSequencesList/FrustrationTermometer")]
public class FrustrationTermometer : SimpleGameSequenceItem
{
    public FrustrationLevel selectedFrustrationLevel;
    public FrustrationLevel defaultFrustrationLevel;
}
