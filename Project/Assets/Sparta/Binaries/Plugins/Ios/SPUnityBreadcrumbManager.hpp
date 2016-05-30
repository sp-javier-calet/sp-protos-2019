#ifndef __SPUnityBreadcrumbManager__
#define __SPUnityBreadcrumbManager__

#include <string>
#include <list>

namespace socialpoint
{
    class SPUnityBreadcrumbManager
    {
      public:
        typedef std::list<std::string> LogList;

        static SPUnityBreadcrumbManager* getInstance();

        //Non-copyable
        SPUnityBreadcrumbManager(SPUnityBreadcrumbManager const&) = delete;
        SPUnityBreadcrumbManager& operator=(SPUnityBreadcrumbManager const&) = delete;
        
        ~SPUnityBreadcrumbManager() = default;

        void leaveBreadcrumb(const std::string& info);
        void setMaxLogs(size_t maxLogs);
        std::string getLog() const;
        void clear();

        void setDumpFilePath(const std::string& directory, const std::string& file);
        std::string dumpToFile();
        std::string dumpToFile(const std::string& directory, const std::string& file);

      private:
      	static const unsigned kDefaultSizeLogs;

      	size_t _maxLogs;
        LogList _logList;
        std::string _breadcrumbDirectory;
        std::string _breadcrumbFile;

        SPUnityBreadcrumbManager();

        void resizeIfNeeded();
    };
}

#endif /* defined(__SPUnityBreadcrumbManager__) */
