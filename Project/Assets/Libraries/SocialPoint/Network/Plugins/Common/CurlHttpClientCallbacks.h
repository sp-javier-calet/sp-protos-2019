//
//  CurlCertificate.h
//  hydra
//
//  Created by Mario Quesada on 17/01/14.
//
//

#ifndef __Hydra__CurlHttpClientCallbacks__
#define __Hydra__CurlHttpClientCallbacks__

extern "C" {
#include "curl/curl.h"
}



    CURLcode addSingleCertificate(const char* certBuffer, CURL* curl, void* sslctx, void* parm);

    CURLcode curlCallback(CURL* curl, void* sslctx, void* parm);

#endif /* defined(__Hydra__CurlHttpClientCallbacks__) */
