//
//  SSLCertificate.cpp
//  hydra
//
//  Created by Mario Quesada on 21/01/14.
//  Copyright (c) 2014 socialpoint. All rights reserved.
//

#include "SSLCertificateReader.h"
#include "SSLCertificate.h"
#include "SSLCertificateValidator.h"

#include <cassert>
#include <cmath>


    SSLCertificate::SSLCertificate()
    : _ca(NULL)
    , _caEncrypt(NULL)
    , _caSize(0)
    , _cert(NULL)
    , _certEncrypt(NULL)
    , _certSize(0)
    {
        _validator = new SSLCertificateValidator();
    }

    SSLCertificate::~SSLCertificate()
    {
        if(_validator)
        {
            delete(_validator);
            _validator = NULL;
        }

        if(_ca)
        {
            delete[](_ca);
            _ca = NULL;
        }

        if(_cert)
        {
            delete[](_cert);
            _cert = NULL;
        }
    }

    void SSLCertificate::initWithEncrypedPem(const void* caEncrypt, int caSize, const void* certEncrypt, int certSize)
    {
        _caEncrypt = caEncrypt;
        _caSize = caSize;

        _certEncrypt = certEncrypt;
        _certSize = certSize;

        SSLCertificateReader::decrypt(&_ca, _caEncrypt, _caSize);
        
        SSLCertificateReader::decrypt(&_cert, _certEncrypt, _certSize);
        
        _validator->setSample(_cert);
    }

    const char* SSLCertificate::getCABundlePEM()
    {
        return _ca;
    }

    const char* SSLCertificate::getCertificatePEM()
    {
        return _cert;
    }

    bool SSLCertificate::validateAgainst(X509* cert)
    {
        assert(_validator);

        return _validator->validateAgainst(cert);
    }

