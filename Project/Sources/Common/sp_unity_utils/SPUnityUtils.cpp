#include "SPUnityUtils.h"
#include <random>

class RandomEngine
{
private:
    std::random_device _device;
    std::random_device::result_type _seed;
    std::mt19937 _engine;

public:
    RandomEngine():
    _seed(_device()),
    _engine(_seed)
    {
    }

    std::random_device::result_type getSeed() const
    {
        return _seed;
    }

    template <typename T, typename D>
    T getNext(D distri)
    {
        return distri(_engine);
    }
};

static RandomEngine engine;

int SPUnityUtilsGetRandom()
{
    return engine.getNext<int>(std::uniform_int_distribution<int>());
}

int SPUnityUtilsGetRandomSeed()
{
    return engine.getSeed();
}
