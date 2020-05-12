public class Options
{

    public static Options defaultOptions { get; private set; } = new Options(-20f, 0f, 0.4f, true, 400f, true, 100f, 5f);
    public static Options currentOptions = new Options(defaultOptions);

    public float musicVolume;
    public float sfxVolume;
    public float brightness;
    public bool dynamicLighting;
    public float pixelation;
    public bool crtEffect;
    public float fov;
    public float sensitivity;

    public Options(float musicVolume, float sfxVolume, float brightness, bool dynamicLighting, float pixelation, bool crtEffect, float fov, float sensitivity)
    {
        this.musicVolume = musicVolume;
        this.sfxVolume = sfxVolume;
        this.brightness = brightness;
        this.dynamicLighting = dynamicLighting;
        this.pixelation = pixelation;
        this.crtEffect = crtEffect;
        this.fov = fov;
        this.sensitivity = sensitivity;
    }

    public Options(Options clone)
    {
        this.musicVolume = clone.musicVolume;
        this.sfxVolume = clone.sfxVolume;
        this.brightness = clone.brightness;
        this.dynamicLighting = clone.dynamicLighting;
        this.pixelation = clone.pixelation;
        this.crtEffect = clone.crtEffect;
        this.fov = clone.fov;
        this.sensitivity = clone.sensitivity;
    }
}
