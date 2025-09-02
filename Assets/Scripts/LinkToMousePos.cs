using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
public class LinkToMousePos : MonoBehaviour
{
   
    [SerializeField] Transform mouseEffect;
    [SerializeField] ParticleSystem mouseEffectPartSystem;
    [SerializeField] ParticleSystem finishEffectPartSystem;
    [SerializeField] Vector3 mousepos;
    [SerializeField] int countParticle = 0;
    [SerializeField] bool effect = false;

    [SerializeField] RenderTexture rt;
    float timer;


    void Start()
    {
        mouseEffectPartSystem = mouseEffect.GetComponent<ParticleSystem>();
        finishEffectPartSystem.Clear();
        //finishEffectPartSystem.enableEmission = false;
    }
    
    private IEnumerator PausaEffect()
    {
        finishEffectPartSystem.enableEmission = true;
        yield return new WaitForSeconds(2);
        finishEffectPartSystem.enableEmission = false;
        Debug.Log("Pausa!");
        effect = false;
        //_bgWhite.gameObject.SetActive(true);

    }
    
    void Update()
    {
        return;
        if (Input.GetKeyDown(KeyCode.L))
        {
            finishEffectPartSystem.enableEmission = !finishEffectPartSystem.enableEmission;
        }


        countParticle = mouseEffectPartSystem.particleCount;
        if (!effect)
        {
            
            Vector3 _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //mouseEffect.localPosition = new Vector3(_pos.x, _pos.y, 50);
            //mousepos = new Vector3(_pos.x, _pos.y, 2);

            if (Input.GetMouseButton(0))
            {
               
                OnEffect(_pos.x, _pos.y);
            }
            else
            {
                OffEffect();
            }

            if (mouseEffectPartSystem.particleCount > 100)
            {
                OffEffect();
                effect = true;
                StartCoroutine(PausaEffect());
            }
        } 
    }

    public void OnEffect(float x, float y)
    {
        mouseEffect.localPosition = new Vector3(x, y, 50);
        mouseEffectPartSystem.Play();
    }

    public void OffEffect()
    {
        mouseEffectPartSystem.Pause();
    }

}
