//
//  SPPurchaseUtils.h
//  sp_unity_plugins
//
//  Created by Andres Barrera on 18/04/16.
//
//

#ifndef SPPurchaseUtils_h
#define SPPurchaseUtils_h

#include <string>
#include <sstream>
#include <vector>

class SPPurchaseUtils
{
    public:
    
        static std::vector<std::string> split(const std::string &s, char delim) {
                std::vector<std::string> elems;
                split(s, delim, elems);
                return elems;
        }
    
    private:
    
        static std::vector<std::string> &split(const std::string &s, char delim, std::vector<std::string> &elems) {
            std::stringstream ss(s);
            std::string item;
            while (std::getline(ss, item, delim)) {
                elems.push_back(item);
            }
            return elems;
        }
};

#endif /* SPPurchaseUtils_h */
