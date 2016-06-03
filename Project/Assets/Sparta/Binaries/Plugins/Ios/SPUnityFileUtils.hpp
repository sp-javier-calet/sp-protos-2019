#ifndef __SPUnityFileUtils__
#define __SPUnityFileUtils__

#include <string>
#include <vector>
#include <functional>

class SPUnityFileUtils
{
  public:
    enum class AccessMode : int
    {
        Read = 0,
        ReadBinary,
        Write
    };

    static bool createDirectory(const std::string& pathToDirectory);
    static bool createEmptyFile(const std::string& pathToFile);
    static bool createFileWithData(const std::string& data, const std::string& pathToFile);
    static bool renameFile(const std::string& oldPath, const std::string& newPath);
    static bool removeFile(const std::string& pathToFile);
};

#endif /* defined(__SPUnityFileUtils__) */
