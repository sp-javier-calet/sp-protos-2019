#include "SPUnityBreadcrumbManager.hpp"
#include <sstream>

namespace socialpoint
{
    const unsigned SPUnityBreadcrumbManager::kDefaultSizeLogs = 50;

    SPUnityBreadcrumbManager* SPUnityBreadcrumbManager::getInstance()
    {
        //Singleton instance
        static SPUnityBreadcrumbManager instance;
        return &instance;
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