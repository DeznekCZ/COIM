
using System;
using Mafi.Collections.ImmutableCollections;
using UnityEngine;

namespace WindPower.Data.Unity;

public class BlurEffect : MonoBehaviour {
	public ImmutableArray<GameObject> blurModels;
	public ImmutableArray<BlurConfig> blurConfig;
	public int lastConfigIndex = -1;

	public void SetAnglePerSecond(float anglePerSecond) {
		// update visibility of blur models
		if (blurModels == null || blurConfig == null) {
			return;
		}

		// convert from angle per tick to RPM
		float speedRpm = anglePerSecond * 60f / 360f;
		for (int configIndex = 0; configIndex < blurConfig.Length; configIndex++) {
			BlurConfig config = blurConfig[configIndex];
			if (!(speedRpm >= config.minRPM) || !(speedRpm <= config.maxRPM)) {
				continue;
			}
			if (configIndex == lastConfigIndex) {
				break;
			}
			for (int i = 0; i < blurModels.Length; i++) {
				if (blurModels[i]) {
					blurModels[i].SetActive(i >= config.minIndexInModels && i <= config.maxIndexInModels);
				}
			}
			lastConfigIndex = configIndex;
			break;
		}
	}
}

public struct BlurConfig(float minRpm, float maxRpm, float minIndexInModels, float maxIndexInModels) {
	public float minRPM = minRpm;
	public float maxRPM = maxRpm;
	public float minIndexInModels = minIndexInModels;
	public float maxIndexInModels = maxIndexInModels;
}
