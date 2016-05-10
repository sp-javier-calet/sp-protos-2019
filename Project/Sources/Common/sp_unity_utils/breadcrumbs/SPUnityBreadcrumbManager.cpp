#include "SPUnityBreadcrumbManager.hpp"

namespace socialpoint
{
    SPUnityBreadcrumbManager* SPUnityBreadcrumbManager::_instance = nullptr;

    const unsigned SPUnityBreadcrumbManager::kDefaultSizeLogs = 50;

    SPUnityBreadcrumbManager* SPUnityBreadcrumbManager::getInstance()
    {
        if(!_instance)
        {
            _instance = new SPUnityBreadcrumbManager();
        }
        return _instance;
    }

    SPUnityBreadcrumbManager::SPUnityBreadcrumbManager()
    : _maxLogs(kDefaultSizeLogs)
    , _logList()
    {

    }

    SPUnityBreadcrumbManager::~SPUnityBreadcrumbManager()
    {

    }

    void SPUnityBreadcrumbManager::leaveBreadcrumb(time_t timestamp, const std::string& info)
    {

    }

    void SPUnityBreadcrumbManager::setMaxLogs(size_t maxLogs)
    {

    }

    void SPUnityBreadcrumbManager::clear()
    {

    }
}