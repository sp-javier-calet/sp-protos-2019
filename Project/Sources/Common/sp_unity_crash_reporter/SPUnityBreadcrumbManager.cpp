#include "SPUnityBreadcrumbManager.hpp"
#include <sstream>

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

    void SPUnityBreadcrumbManager::leaveBreadcrumb(const std::string& info)
    {
        _logList.push_front(info);
        resizeIfNeeded();
    }

    void SPUnityBreadcrumbManager::setMaxLogs(size_t maxLogs)
    {
        _maxLogs = maxLogs;
        resizeIfNeeded();
    }

    std::string SPUnityBreadcrumbManager::getLog() const
    {
        std::stringstream ss;

        for(const std::string& log : _logList)
        {
            ss << log << "\n";
        }

        return ss.str();
    }

    void SPUnityBreadcrumbManager::clear()
    {
        _logList.clear();
    }

    void SPUnityBreadcrumbManager::resizeIfNeeded()
    {
        if(_logList.size() > _maxLogs)
        {
            _logList.resize(_maxLogs);
        }
    }
}