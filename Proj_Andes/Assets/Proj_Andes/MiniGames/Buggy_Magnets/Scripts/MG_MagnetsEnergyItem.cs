using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_MagnetsEnergyItem : MonoBehaviour
{

    [SerializeField] ParticleSystem capturedVFX;
    [SerializeField] AudioSource capturedSFX;
    [SerializeField] SpriteRenderer spriteRef;

      public void OnWasPicked(Pool <MG_MagnetsEnergyItem> pool)
    {
        StartCoroutine(_OnCapturedwithDelay(pool));
    }
    IEnumerator _OnCapturedwithDelay(Pool<MG_MagnetsEnergyItem> pool)
    {
        capturedVFX.Play();
        capturedSFX.Play();
        spriteRef.enabled = false;
        yield return new WaitForSeconds(0.7f);
        pool.RecycleItem(this);
        spriteRef.enabled = true;

    }
}
