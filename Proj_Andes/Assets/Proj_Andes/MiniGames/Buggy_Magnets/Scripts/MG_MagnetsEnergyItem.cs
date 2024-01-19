using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_MagnetsEnergyItem : MonoBehaviour, iTutorialType
{

    [SerializeField] ParticleSystem capturedVFX;
    [SerializeField] AudioSource capturedSFX;
    [SerializeField] SpriteRenderer spriteRef;

    float timer = 0;
    float lifeTime = 1;
    Pool<MG_MagnetsEnergyItem> pool;
    bool wasCaptured = false;
    bool hasLifeTime = false;

	public void Init(float _lifeTime, Pool<MG_MagnetsEnergyItem> ownerPool)
    {
        lifeTime = _lifeTime;
        pool = ownerPool;
        timer = 0;
    }

    public void OnWasPicked() => StartCoroutine(_OnCapturedwithDelay());
    
    IEnumerator _OnCapturedwithDelay()
    {
		wasCaptured = true;
		capturedVFX.Play();
        capturedSFX.Play();
        spriteRef.enabled = false;
        yield return new WaitForSeconds(0.7f);
        pool.RecycleItem(this);
        spriteRef.enabled = true;
        wasCaptured = false;

    }
	private void Update()
	{
        if (wasCaptured) return;
        if (!hasLifeTime) return;
        timer += Time.deltaTime;
        if (timer > lifeTime) pool.RecycleItem(this);
	}

    public void StepStart(bool stepCompleted)
    {
        hasLifeTime = !stepCompleted;
    }

    public void StepDone()
    {
        hasLifeTime = true;
    }
}
