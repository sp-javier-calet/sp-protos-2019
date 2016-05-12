#ifndef __hydra__SPUnityBreadcrumbManager__
#define __hydra__SPUnityBreadcrumbManager__

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

      private:
      	//Singleton instance
      	static SPUnityBreadcrumbManager* _instance;

      	static const unsigned kDefaultSizeLogs;

      	size_t _maxLogs;
        LogList _logList;

        SPUnityBreadcrumbManager();

        void resizeIfNeeded();
    };
}

#endif /* defined(__hydra__SPUnityBreadcrumbManager__) */
