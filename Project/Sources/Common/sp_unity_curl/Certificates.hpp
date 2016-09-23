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
public:
    const uint8_t* const key;
    const size_t keySize;

    Certificate(const std::string& name);
};

#endif /* __sparta__Certificates__ */
