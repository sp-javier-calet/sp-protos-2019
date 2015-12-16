//
//  SSLCertReader.h
//  hydra
//
//  Created by Mario Quesada on 22/01/14.
//  Copyright (c) 2014 socialpoint. All rights reserved.
//

#ifndef __hydra__SSLCertificateReader__
#define __hydra__SSLCertificateReader__

#include <string>


    class SSLCertificateReader
    {
        static const unsigned char _secret[];

      public:
        static char* readFromPath(const std::string& path);

        void static printEncryptedPEM(const void* encrypted, int size);
        void static printPEM(const char* pem);
        int static diff(const char* a, const char* b, int length);

        void static encrypt(char** encrypt, const char* cert, int size);
        void static decrypt(char** cert, const void* encrypt, int size);
        void static scapeSpecialCharacters(unsigned char u, char* buffer, size_t buflen);
    };

#endif /* defined(__hydra__SSLCertificateReader__) */
