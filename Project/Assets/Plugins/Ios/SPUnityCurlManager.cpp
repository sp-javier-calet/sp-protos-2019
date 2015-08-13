#include "SPUnityCurlManager.h"


SPUnityCurlGlobalInfo::SPUnityCurlGlobalInfo()
: multi(NULL)
, still_running(0)
{
}

SPUnityCurlConnInfo::SPUnityCurlConnInfo(intptr_t id)
: id(id)
, easy(NULL)
, responseCode(0)
, downloadSize(0.0)
, downloadSpeed(0.0)
{
}

SPUnityCurlManager::SPUnityCurlManager()
{
}

SPUnityCurlManager& SPUnityCurlManager::getInstance()
{
    static SPUnityCurlManager instance;
    return instance;
}

void SPUnityCurlManager::addConn(intptr_t id)
{
    ConnInfo* conn = new ConnInfo(id);
    _clients.insert(std::make_pair(id, conn));
}

SPUnityCurlManager::ConnInfo* SPUnityCurlManager::getConnById(intptr_t id)
{
    MClients::iterator it =  _clients.find(id);
    if ( it != _clients.end())
    {
        return it->second;
    }
    return NULL;
}

bool SPUnityCurlManager::removeConn(intptr_t id)
{
    MClients::iterator it =  _clients.find(id);
    if ( it == _clients.end())
    {
        return false;
    }
    delete it->second;
    _clients.erase(it);
    return true;
}
