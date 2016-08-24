#include "SPUnityFileUtils.hpp"
#include <sys/stat.h>
#include <errno.h>
#include <assert.h>
#include <dirent.h>
#include <map>

bool SPUnityFileUtils::createDirectory(const std::string& pathToDirectory)
{
    int result = mkdir(pathToDirectory.c_str(), S_IRWXU);

    // Error occurred
    if(result == -1)
    {
        // if directory already exist, return true
        return errno == EEXIST;
    }

    return true;
}

bool SPUnityFileUtils::createEmptyFile(const std::string& pathToFile)
{
    FILE* pFile = fopen(pathToFile.c_str(), "w");

    if(pFile != nullptr)
    {
        fflush(pFile);
        fclose(pFile);
        return true;
    }

    return false;
}

bool SPUnityFileUtils::createFileWithData(const std::string& data, const std::string& pathToFile)
{
    FILE* pFile = fopen(pathToFile.c_str(), "wb");

    if(pFile != nullptr)
    {
        fwrite(data.data(), sizeof(char), data.length(), pFile);
        fflush(pFile);
        fclose(pFile);
        return true;
    }

    return false;
}

bool SPUnityFileUtils::renameFile(const std::string& oldPath, const std::string& newPath)
{
    return rename(oldPath.c_str(), newPath.c_str()) == 0;
}

bool SPUnityFileUtils::removeFile(const std::string& pathToFile)
{
    int result = remove(pathToFile.c_str());

    return !result;
}
