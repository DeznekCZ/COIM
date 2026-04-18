using System;
using System.Collections.Generic;
using Mafi.Collections;
using UnityEngine;
using WindPower.Data.Unity;

public class RotateZ : MonoBehaviour
{
	[Range(0, 360f)]
	public float m_speedRPM = 0f;

	public Transform m_rotatedTransform;
	public BlurEffect m_blurEffect;

	// Update is called once per frame
	void Update()
	{
		// convert from RPM to degrees per second
		float anglePerSecond = m_speedRPM * 360.0f / 60.0f;
		m_rotatedTransform.Rotate(0.0f, 0.0f, anglePerSecond * Time.deltaTime);

		if (m_blurEffect) {
			m_blurEffect.SetAnglePerSecond(anglePerSecond);
		}
	}
}
