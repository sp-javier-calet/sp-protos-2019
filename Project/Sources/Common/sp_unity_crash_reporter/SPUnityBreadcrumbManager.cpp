#include "SPUnityBreadcrumbManager.hpp"
#include "SPUnityFileUtils.hpp"
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
    , _breadcrumbDirectory()
    , _breadcrumbFile()
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

    void SPUnityBreadcrumbManager::setDumpFilePath(const std::string& directory, const std::string& file)
    {
        _breadcrumbDirectory = directory;
        _breadcrumbFile = file;
    }

    std::string SPUnityBreadcrumbManager::dumpToFile()
    {
        if(!_breadcrumbDirectory.empty() && !_breadcrumbFile.empty())
        {
            return dumpToFile(_breadcrumbDirectory, _breadcrumbFile);
        }
        return std::string();
    }

    std::string SPUnityBreadcrumbManager::dumpToFile(const std::string& directory, const std::string& file)
    {
        if(SPUnityFileUtils::createDirectory(_breadcrumbDirectory))
        {
            std::string filePath(_breadcrumbDirectory + _breadcrumbFile);
            SPUnityFileUtils::createFileWithData(getLog(), filePath);
            return filePath;
        }
        return std::string();
    }
}