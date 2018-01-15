#ifndef __sparta__BreadcrumbManager__
#define __sparta__BreadcrumbManager__

#include <string>
#include <list>

class BreadcrumbManager
{
  public:
    typedef std::list<std::string> LogList;

    static BreadcrumbManager& getInstance();

    // Non-copyable
    BreadcrumbManager(BreadcrumbManager const&) = delete;
    BreadcrumbManager& operator=(BreadcrumbManager const&) = delete;

    ~BreadcrumbManager() = default;

    void leaveBreadcrumb(const std::string& info);
    void setMaxLogs(size_t maxLogs);
    std::string getLog() const;
    void clear();

    void setDumpFilePath(const std::string& directory, const std::string& file);
    std::string dumpToFile() const;
    std::string dumpToFile(const std::string& directory, const std::string& file) const;

  private:
    static const unsigned kDefaultSizeLogs;

    size_t _maxLogs;
    LogList _logList;
    std::string _breadcrumbDirectory;
    std::string _breadcrumbFile;

    BreadcrumbManager();

    void resizeIfNeeded();
};

#endif /* defined(__BreadcrumbManager__) */
