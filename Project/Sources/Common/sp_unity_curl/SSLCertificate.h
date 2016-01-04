//
//  SSLCertificate.h
//  hydra
//
//  Created by Mario Quesada on 21/01/14.
//  Copyright (c) 2014 socialpoint. All rights reserved.
//

#ifndef __hydra__SSLCertificate__
#define __hydra__SSLCertificate__

#include "SSLCertificateValidator.h"
#include <openssl/x509.h>
#include <iostream>


    class SSLCertificateValidator;

    class SSLCertificate
    {
        const void* _caEncrypt;
        int _caSize;

        const void* _certEncrypt;
        int _certSize;

        char* _ca;// no need size as char ends with \0
        char* _cert;

        SSLCertificateValidator* _validator;

      public:
        SSLCertificate();
        ~SSLCertificate();

        void initWithEncrypedPem(const void* caEncrypt, int caSize, const void* certEncrypt, int certSize);

        const char* getCABundlePEM();   // our trusted PEMs and CAs
        const char* getCertificatePEM();// single certificate of our game

        bool validateAgainst(X509* cert);
    };


#endif /* defined(__hydra__SSLCertificate__) */
