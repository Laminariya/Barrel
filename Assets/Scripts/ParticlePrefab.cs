using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePrefab : MonoBehaviour
{

    private float _maxScale;
    private float _minScale;
    private float _speed;
    private Transform _transform;
    private Coroutine _coroutine;

    public void Init()
    {
        _maxScale = GameManager.Instance.MaxScale;
        _minScale = GameManager.Instance.MinScale;
        _speed = GameManager.Instance.SpeedParticle;
        _transform = transform;
        Hide();
    }

    public void Show()
    {
        GameManager.Instance.CountParticle++;
        gameObject.SetActive(true);
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(StartShow());
    }

    public void Finish()
    {

    }

    public void Hide()
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
        GameManager.Instance.CountParticle--;
        gameObject.SetActive(false);
        GameManager.Instance.ParticlePrefabs.Enqueue(this);
    }

    IEnumerator StartShow()
    {
        float s = _minScale;
        _transform.localScale = Vector3.one * s;
        while (_transform.localScale.x < _maxScale)
        {
            s += Time.deltaTime * _speed;
            _transform.localScale = Vector3.one * s;
            yield return null;
        }

        yield return new WaitForSeconds(3f);
        while (_transform.localScale.x > _minScale)
        {
            s -= Time.deltaTime * _speed;
            _transform.localScale = Vector3.one * s;
            yield return null;
        }

        Hide();
    }
}
