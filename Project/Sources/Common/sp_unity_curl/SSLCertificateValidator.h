//
//  SSLCertificateValidator.h
//  hydra
//
//  Created by Mario Quesada on 21/01/14.
//  Copyright (c) 2014 socialpoint. All rights reserved.
//

#ifndef __hydra__SSLCertificateValidator__
#define __hydra__SSLCertificateValidator__

#include <iostream>

#include <openssl/x509.h>


    class SSLCertificateValidator
    {
        X509* _cert;

      public:
        // single pem certificate
        void setSample(const char* sample);
        void setSample(X509* sample);

        bool validateAgainst(X509* cert);
    };


#endif /* defined(__hydra__SSLCertificateValidator__) */
