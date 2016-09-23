//
//  Mutex.hpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 23/09/16.
//
//

#ifndef __sparta__Mutex__
#define __sparta__Mutex__

#if defined(WIN32) || defined(WIN64)

#include <mutex>
typedef std::Mutex Mutex;

#else

#include "pthread.h"


class Mutex
{
private:
    pthread_mutex_t _mutex;
    bool _initialized;
    
public:
    Mutex()
    : _initialized(false)
    {
    }
    
    void lock()
    {
        if(!_initialized)
        {
            _initialized = true;
            pthread_mutex_init(&_mutex, NULL);
        }
        pthread_mutex_lock(&_mutex);
    }
    
    void unlock()
    {
        pthread_mutex_unlock(&_mutex);
    }
};

#endif


#endif /* __sparta__Mutex__ */
