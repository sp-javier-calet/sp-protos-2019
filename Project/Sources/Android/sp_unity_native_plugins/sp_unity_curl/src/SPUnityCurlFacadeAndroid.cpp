
#include <mutex>
#include <thread>
#include "CurlClient.hpp"

std::mutex m;
bool AppPaused = false;

extern "C"
{
    void SPUnityCurlOnApplicationPause(CurlClient* client, bool paused)
    {
        if(client->isRunning())
        {
            return;
        }

        if(paused)
        {
            std::lock_guard<std::mutex> lk(m);
            AppPaused = true;
            std::thread updater([client]()
                {
                    while(client->isRunning())
                    {
                        std::lock_guard<std::mutex> lk(m);
                        if(AppPaused)
                        {
                            client->update();
                        }
                        else
                        {
                            break;
                        }
                    }
                });
            updater.detach();
        }
        else
        {
            std::lock_guard<std::mutex> lk(m);
            AppPaused = false;
        }
    }
}