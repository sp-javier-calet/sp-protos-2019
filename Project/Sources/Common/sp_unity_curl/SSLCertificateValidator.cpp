//
//  SSLCertificateValidator.cpp
//  hydra
//
//  Created by Mario Quesada on 21/01/14.
//  Copyright (c) 2014 socialpoint. All rights reserved.
//

#include "SSLCertificateValidator.h"

#include <openssl/ssl.h>
#include <openssl/pem.h>

#include <cassert>


    void SSLCertificateValidator::setSample(const char* sample)
    {
        _cert = NULL;
        BIO* bio = NULL;

        bio = BIO_new_mem_buf((char*)sample, -1);
        assert(NULL != bio);

        _cert = PEM_read_bio_X509(bio, NULL, 0, NULL);
    }

    void SSLCertificateValidator::setSample(X509* sample)
    {
        _cert = sample;
    }

    bool SSLCertificateValidator::validateAgainst(X509* current)
    {
        if(!_cert)
        {
            // if no sample certificate there is nothing to check against
            return true;
        }

        if(!current)
        {
            return false;
        }

        assert(current);

        char s[256];
        char sample_s[256];

        bool isOk = current->cert_info->key->public_key->length == _cert->cert_info->key->public_key->length;
        isOk &= ASN1_INTEGER_get(X509_get_serialNumber(current)) == ASN1_INTEGER_get(X509_get_serialNumber(_cert));
        isOk &= X509_certificate_type(_cert, X509_get_pubkey(current)) == X509_certificate_type(_cert, X509_get_pubkey(_cert));
        isOk &= ASN1_INTEGER_get(X509_get_serialNumber(current)) == ASN1_INTEGER_get(X509_get_serialNumber(_cert));

        X509_NAME_oneline(X509_get_issuer_name(current), s, 256);
        X509_NAME_oneline(X509_get_issuer_name(_cert), sample_s, 256);
        isOk &= strcmp(s, sample_s) == 0;

        X509_NAME_oneline(X509_get_subject_name(current), s, 256);
        X509_NAME_oneline(X509_get_subject_name(_cert), sample_s, 256);
        isOk &= strcmp(s, sample_s) == 0;

        isOk &= X509_certificate_type(current, X509_get_pubkey(current)) == X509_certificate_type(current, X509_get_pubkey(_cert));

        isOk &= ASN1_INTEGER_get(X509_get_serialNumber(current)) == ASN1_INTEGER_get(X509_get_serialNumber(_cert));

        return isOk;
    }

