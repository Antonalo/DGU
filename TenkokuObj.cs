using System;

[Serializable]
public class TenkokuObj
{
	public float timeCompression = 100f;

	public int currentMinute = 45;

	public int currentHour = 5;

	public int currentDay = 22;

	public int currentMonth = 3;

	public int currentYear = 2013;

	public float volumeAmbDay = 1f;

	public float setLatitude;

	public float skyBrightness = 1f;

	public float nightBrightness = 0.4f;

	public float atmosphereDensity = 0.55f;

	public float weather_cloudAltoStratusAmt;

	public float weather_cloudCirrusAmt;

	public float weather_cloudCumulusAmt = 0.2f;

	public float weather_cloudScale = 0.5f;

	public float weather_cloudSpeed;

	public float weather_OvercastAmt;

	public float weather_RainAmt;

	public float weather_SnowAmt;

	public float weather_WindAmt;

	public float weather_WindDir;

	public float weather_FogAmt;

	public float weather_FogHeight;

	public float weather_humidity = 0.25f;

	public float weather_rainbow;

	public float weather_lightning;

	public float weather_lightningRange = 180f;

	public bool autoTime;
}
