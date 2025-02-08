using UnityEngine;

public class SliderAnimator : MonoBehaviour
{
	[SerializeField] private float _speed = 1f;
	[SerializeField] private Morpher _morpher;
	[SerializeField] private GameObject _nextObject;
	[SerializeField] private GameObject _oldObject;

	private float _currentValue = 0f;
	private float _targetValue = 0f;
	private bool _isAnimating = false;

	private void Update()
	{
		if (_isAnimating)
		{
			_currentValue = Mathf.MoveTowards(_currentValue, _targetValue, _speed * Time.deltaTime);
			_morpher.SetSlider(_currentValue);

			if (Mathf.Approximately(_currentValue, _targetValue))
			{
				_isAnimating = false;
				OnAnimationComplete();
			}
		}

		if (Input.GetKeyDown(KeyCode.R) && !_isAnimating)
		{
			_targetValue = _currentValue == 0f ? 1f : 0f;
			if (_targetValue == 0f)
			{
				OnAnimationComplete();
			}
			_isAnimating = true;
		}
	}

	private void OnAnimationComplete()
	{
		if (_nextObject != null) _nextObject.SetActive(_targetValue == 1);
		if (_oldObject != null) _oldObject.SetActive(_targetValue == 0);
	}
}
