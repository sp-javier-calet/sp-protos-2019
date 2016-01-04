//
//  CurlCertificate.cpp
//  hydra
//
//  Created by Mario Quesada on 17/01/14.
//
// SSL_CTX_set_verify
// http://curl.haxx.se/mail/lib-2005-06/0106.html
//
// Load SSL certificates in memory
// http://www.opensource.apple.com/source/curl/curl-57/curl/docs/examples/cacertinmem.c
//
// Download SSL Certificate script
// sudo rm -f cert.pem && sudo echo -n | openssl s_client -connect host:443 | sed -ne '/-BEGIN CERTIFICATE-/,/-END CERTIFICATE-/p' > ./cert.pem

#include "CurlHttpClientCallbacks.h"
#include "SSLCertificate.h"

#include <openssl/ssl.h>
#include <openssl/x509.h>
#include <openssl/pem.h>
#include <cassert>



#pragma mark - CurlCertificate

    // certificate must be global to use the SSL_CTX_set_verify, not found a way to pass a param
    SSLCertificate* __certificate = NULL;

    CURLcode addSingleCertificate(const char* certificate, CURL* curl, void* sslctx)
    {
        X509* cert = NULL;
        BIO* bio = NULL;

        assert(certificate);

        (void)curl; /* avoid warnings */

        bool isOk = true;

        /* get a BIO */
        bio = BIO_new_mem_buf((char*)certificate, -1);

        if(bio == NULL)
        {
            printf("BIO_new_mem_buf failed\n");

            isOk = false;
        }

        cert = PEM_read_bio_X509(bio, NULL, 0, NULL);
        if(cert == NULL)
        {
            printf("PEM_read_bio_X509 failed...\n");

            isOk = false;
        }

        /*tell SSL to use the X509 certificate*/
        int ret = SSL_CTX_use_certificate((SSL_CTX*)sslctx, cert);
        if(ret != 1)
        {
            printf("Use certificate failed\n");

            isOk = false;
        }

        X509_STORE* store = SSL_CTX_get_cert_store((SSL_CTX*)sslctx);
        if(store == NULL)
        {
            printf("Store has failed\n");

            isOk = false;
        }
        else
        {
            X509_STORE_add_cert(store, cert);
        }


        // clean resources
        if(bio)
        {
            BIO_free(bio);
        }

        if(cert)
        {
            X509_free(cert);
        }

        if(isOk)
        {
            return CURLE_OK;
        }
        else
        {
            return CURLE_ABORTED_BY_CALLBACK;
        }
    }

    CURLcode addServerCertificates(const char* certificates, CURL* curl, void* sslctx)
    {
        std::string certs(certificates);

        // Split the pem file into every certificate
        std::string delimiter = "-----END CERTIFICATE-----";
        size_t pos = 0;
        std::string token;

        bool isOk = true;
        while((pos = certs.find(delimiter)) != std::string::npos)
        {
            pos += delimiter.size();// add the end certificate part as well

            token = certs.substr(0, pos);
            certs.erase(0, pos + delimiter.length());

            CURLcode code = addSingleCertificate(token.c_str(), curl, sslctx);
            isOk &= code == CURLE_OK;
        }

        if(isOk)
        {
            return CURLE_OK;
        }

        return CURLE_ABORTED_BY_CALLBACK;
    }

    int verifyServerCertificateCallback(int preverify_ok, X509_STORE_CTX* x509_ctx)
    {
        // return 0 if peer verification was not passed
        if(0 == preverify_ok)
        {
            return 0;
        }

        X509* cert = X509_STORE_CTX_get_current_cert(x509_ctx);

        bool isValid = true;
        int depth = X509_STORE_CTX_get_error_depth(x509_ctx);
        if(__certificate && 0 == depth)
        {
            isValid = __certificate->validateAgainst(cert);
        }

        return isValid ? 1 : 0;
    }

    CURLcode curlCallback(CURL* curl, void* sslctx, void* parm)
    {
        assert(parm);
        SSLCertificate* certificate = (SSLCertificate*)parm;
        __certificate = certificate;// set global parameter

        // Custom step peer verification
        SSL_CTX_set_verify((SSL_CTX*)sslctx, SSL_VERIFY_PEER, verifyServerCertificateCallback);

        // Add trusted PEM certificates
        CURLcode isOk = addServerCertificates(certificate->getCABundlePEM(), curl, sslctx);

        if(isOk == CURLE_OK)
        {
            return CURLE_OK;
        }

        return CURLE_ABORTED_BY_CALLBACK;
    }

