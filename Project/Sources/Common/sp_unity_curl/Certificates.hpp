//
//  Certificates.hpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 23/09/16.
//
//

#ifndef __sparta__Certificates__
#define __sparta__Certificates__

#include <string>

class Certificate
{
    const uint8_t* key;
    size_t keySize;
    
    void obfuscate(const uint8_t* in, uint8_t** out, size_t size);

public:
    Certificate();
    Certificate(const std::string& name);
    
    bool getPinnedKey(uint8_t** out);
};

#endif /* __sparta__Certificates__ */
