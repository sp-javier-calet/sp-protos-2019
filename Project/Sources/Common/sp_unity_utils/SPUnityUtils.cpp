#include "SPUnityUtils.h"
#include <random>
#include <limits>

class RandomEngine
{
  private:
    std::mt19937 _engine;

  public:
    RandomEngine()
    : _engine(std::random_device()())
    {
    }

    template <typename T, typename D>
    T getNext(D distri)
    {
        return distri(_engine);
    }
};

static RandomEngine engine;

unsigned int SPUnityUtilsGetRandomUnsignedInt()
{
    return engine.getNext<unsigned int>(
      std::uniform_int_distribution<unsigned int>(std::numeric_limits<unsigned int>::min(), std::numeric_limits<unsigned int>::max()));
}

int SPUnityUtilsGetRandomInt()
{
    return engine.getNext<int>(std::uniform_int_distribution<int>(std::numeric_limits<int>::min(), std::numeric_limits<int>::max()));
}

int SPUnityUtilsGetRandomIntRange(int min, int max)
{
    return engine.getNext<int>(std::uniform_int_distribution<int>(min, max));
}

float SPUnityUtilsGetRandomFloatRange(float min, float max)
{
    return engine.getNext<float>(std::uniform_real_distribution<float>(min, max));
}

double SPUnityUtilsGetRandomDoubleRange(double min, double max)
{
    return engine.getNext<double>(std::uniform_real_distribution<double>(min, max));
}