using System;

public struct ActionUpdater
{
    const float _defaultDelay = 1f;
    
    float _freq;
    float _accTime;

    Action<float> _action;

    static Random _random;

    public ActionUpdater(Action<float> action, float freq, float startDelay = _defaultDelay)
    {
        _action = action;
        _freq = freq;

        if(_random == null)
        {
            _random = new Random();
        }

        _accTime = freq - ((float)_random.NextDouble()) * startDelay;
    }

    public void Update(float dt)
    {
        _accTime += dt;
        Consume();
    }

    void Consume()
    {
        if(_accTime > _freq)
        {
            _accTime -= _freq;
            _action(_freq);
        }
    }
}
