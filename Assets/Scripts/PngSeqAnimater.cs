using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PngSeqAnimater : MonoBehaviour
{
    [SerializeField] private RawImage _targetImage;
    [SerializeField] private string _folderPath;
    [SerializeField] private string _prefix;    //�ړ���
    [SerializeField] private string _suffix;    //�ړ���

    [SerializeField] private int _startNumber;
    [SerializeField] private int _endNumber;
    [SerializeField] private float _frameInterval = 0.1f;
    [SerializeField] private List<Texture2D> _animatedPngSpriteList;

    private int _currentFrame;
    private bool _isAnimation = false;

    private void Awake()
    {
        for (int i = _startNumber; i < _endNumber + 1; i++)
        {
            _animatedPngSpriteList.Add(Resources.Load(_folderPath + "/" + _prefix + i +_suffix , typeof(Texture2D)) as Texture2D);
        }
        StartAnimation();
    }

    [ContextMenu("StartAnimation")]
    public void StartAnimation()
    {
        _isAnimation = true;
        StartCoroutine(UpdatePNG());
    }

    [ContextMenu("StopAnimation")]
    public void StopAnimation()
    {
        _isAnimation = false;
    }

    private IEnumerator UpdatePNG()
    {
        while (_isAnimation)
        {
            _currentFrame++;
            try {

                _targetImage.texture = _animatedPngSpriteList[_currentFrame];
                if (_currentFrame >= _endNumber - _startNumber) _currentFrame = 0;
                
            }
            catch(Exception ex){
                Debug.Log(ex);
            }
            yield return new WaitForSeconds(_frameInterval);
        }

        yield break;
    }
}