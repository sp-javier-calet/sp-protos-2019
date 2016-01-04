//
//  SSLCertReader.cpp
//  hydra
//
//  Created by Mario Quesada on 22/01/14.
//  Copyright (c) 2014 socialpoint. All rights reserved.
//

#include "SSLCertificateReader.h"

#include <iostream>
#include <fstream>
#include <cstdlib>
#include <cassert>


#define CERT_SECRET_LENGTH 8// need to be a macro to be used in a array of fixed size
    const unsigned char SSLCertificateReader::_secret[CERT_SECRET_LENGTH] = {55, 11, 44, 71, 66, 177, 253, 122};

    void SSLCertificateReader::encrypt(char** encrypt, const char* cert, int size)
    {
        (*encrypt) = new char[size];

        int i;

        unsigned char* certEncriptedCasted = (unsigned char*)encrypt;

        for(i = 0; i < size; i++)
        {
            certEncriptedCasted[i] = cert[i] ^ SSLCertificateReader::_secret[i % CERT_SECRET_LENGTH];
        }
        printEncryptedPEM(encrypt,size);
    }

    void SSLCertificateReader::decrypt(char** cert, const void* encrypt, int size)
    {
        unsigned char* certDecripted = (unsigned char*)new char[size];

        int i;

        unsigned char* certEncriptedCasted = (unsigned char*)encrypt;

        for(i = 0; i < size; i++)
        {
            certDecripted[i] = certEncriptedCasted[i] ^ SSLCertificateReader::_secret[i % CERT_SECRET_LENGTH];
        }

        certDecripted[size - 1] = 0;
        (*cert) = ((char*)certDecripted);
    }

    char* SSLCertificateReader::readFromPath(const std::string& path)
    {
        std::streampos lenght = 0;

        std::ifstream is(path, std::ifstream::binary);

        // get length of file:
        is.seekg(0, is.end);
        lenght = is.tellg();
        is.seekg(0, is.beg);

        char* buffer = new char[lenght];

        // read data as a block:
        is.read(buffer, lenght);

        is.close();

        return buffer;
    }

    void SSLCertificateReader::printEncryptedPEM(const void* encrypted, int size)
    {
        assert(encrypted);

        std::cout << "[start_message]";

        char* buffer = new char[25];

        const char* certEncriptedCasted = (const char*)encrypted;

        for(int i = 0; i < size; ++i)
        {
            char c = certEncriptedCasted[i];

            scapeSpecialCharacters(c, buffer, 25);
            printf("%s", buffer);
        }

        std::cout << "[end_message]" << std::endl;
    }

    void SSLCertificateReader::printPEM(const char* pem)
    {
        assert(pem);

        std::string certs(pem);

        std::string delimiter = "\n";
        size_t pos = 0;
        std::string token;

        while((pos = certs.find(delimiter)) != std::string::npos)
        {
            pos += delimiter.size();
            token = certs.substr(0, pos - delimiter.size());
            certs.erase(0, pos + delimiter.length());

            std::string line = "\n\"" + token + "\\n\"\\";
            std::cout << line;
        }
    }

    int SSLCertificateReader::diff(const char* a, const char* b, int length)
    {
        assert(a);
        assert(b);

        int diff = 0;

        for(int i = 0; i < length; i++)
        {
            diff += abs(a[i] - b[i]);
        }

        return diff;
    }

    void SSLCertificateReader::scapeSpecialCharacters(unsigned char u, char* buffer, size_t buflen)
    {
        assert(buffer);

        if(buflen < 2)
            *buffer = '\0';
        else if(isprint(u) && u != '\'' && u != '\"' && u != '\\' && u != '\?')
            sprintf(buffer, "%c", u);
        else if(buflen < 3)
            *buffer = '\0';
        else
        {
            switch(u)
            {
                case '\a':
                    strcpy(buffer, "\\a");
                    break;
                case '\b':
                    strcpy(buffer, "\\b");
                    break;
                case '\f':
                    strcpy(buffer, "\\f");
                    break;
                case '\n':
                    strcpy(buffer, "\\n");
                    break;
                case '\r':
                    strcpy(buffer, "\\r");
                    break;
                case '\t':
                    strcpy(buffer, "\\t");
                    break;
                case '\v':
                    strcpy(buffer, "\\v");
                    break;
                case '\\':
                    strcpy(buffer, "\\\\");
                    break;
                case '\'':
                    strcpy(buffer, "\\'");
                    break;
                case '\"':
                    strcpy(buffer, "\\\"");
                    break;
                case '\?':
                    strcpy(buffer, "\\\?");
                    break;
                default:
                    if(buflen < 5)
                        *buffer = '\0';
                    else
                        sprintf(buffer, "\\%03o", u);
                    break;
            }
        }
    }

