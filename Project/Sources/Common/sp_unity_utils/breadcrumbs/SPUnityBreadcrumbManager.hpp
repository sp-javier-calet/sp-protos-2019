#ifndef __hydra__SPUnityBreadcrumbManager__
#define __hydra__SPUnityBreadcrumbManager__

#include <string>
#include <list>

namespace socialpoint
{
    class SPUnityBreadcrumbManager
    {
      public:
        typedef std::pair<time_t, std::string> Log;
        typedef std::list<Log> LogList;

        static SPUnityBreadcrumbManager* getInstance();

        //Non-copyable
        SPUnityBreadcrumbManager(SPUnityBreadcrumbManager const&) = delete;
    	SPUnityBreadcrumbManager& operator=(SPUnityBreadcrumbManager const&) = delete;
        ~SPUnityBreadcrumbManager();

        void leaveBreadcrumb(time_t timestamp, const std::string& info);
        void setMaxLogs(size_t maxLogs);
        void clear();

      private:
      	//Singleton instance
      	static SPUnityBreadcrumbManager* _instance;

      	static const unsigned kDefaultSizeLogs;

      	size_t _maxLogs;
        LogList _logList;

        SPUnityBreadcrumbManager();
    };
}

#endif /* defined(__hydra__SPUnityBreadcrumbManager__) */
