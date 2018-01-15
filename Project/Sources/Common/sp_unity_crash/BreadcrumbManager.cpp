#include "BreadcrumbManager.hpp"
#include "SPUnityFileUtils.hpp"
#include <sstream>

const unsigned BreadcrumbManager::kDefaultSizeLogs = 50;

BreadcrumbManager& BreadcrumbManager::getInstance()
{
    // Singleton instance
    static BreadcrumbManager instance;
    return instance;
}

BreadcrumbManager::BreadcrumbManager()
: _maxLogs(kDefaultSizeLogs)
, _logList()
, _breadcrumbDirectory()
, _breadcrumbFile()
{
}

void BreadcrumbManager::leaveBreadcrumb(const std::string& info)
{
    _logList.push_front(info);
    resizeIfNeeded();
}

void BreadcrumbManager::setMaxLogs(size_t maxLogs)
{
    _maxLogs = maxLogs;
    resizeIfNeeded();
}

std::string BreadcrumbManager::getLog() const
{
    std::stringstream ss;

    for(const std::string& log : _logList)
    {
        ss << log << "\n";
    }

    return ss.str();
}

void BreadcrumbManager::clear()
{
    _logList.clear();
}

void BreadcrumbManager::resizeIfNeeded()
{
    if(_logList.size() > _maxLogs)
    {
        _logList.resize(_maxLogs);
    }
}

void BreadcrumbManager::setDumpFilePath(const std::string& directory, const std::string& file)
{
    _breadcrumbDirectory = directory;
    _breadcrumbFile = file;
}

std::string BreadcrumbManager::dumpToFile() const
{
    if(!_breadcrumbDirectory.empty() && !_breadcrumbFile.empty())
    {
        return dumpToFile(_breadcrumbDirectory, _breadcrumbFile);
    }
    return std::string();
}

std::string BreadcrumbManager::dumpToFile(const std::string& directory, const std::string& file) const
{
    if(SPUnityFileUtils::createDirectory(_breadcrumbDirectory))
    {
        std::string filePath(_breadcrumbDirectory + _breadcrumbFile);
        SPUnityFileUtils::createFileWithData(getLog(), filePath);
        return filePath;
    }
    return std::string();
}
