#include "JniObject.hpp"
#include <android/log.h>
#include <algorithm>

const int Jni::kErrorJniFindCreate = 800;
const int Jni::kErrorJniCallCreate = 801;
const int Jni::kErrorJniFindMethod = 802;
const int Jni::kErrorJniCallMethod = 803;
const int Jni::kErrorJniFindField = 804;
const int Jni::kErrorJniCallField = 805;
const int Jni::kErrorJniFindClass = 806;
const int Jni::kErrorJniFindSingleton = 807;

#define LOG_TAG "JniObject"
const char* Jni::kErrorTagJniObject = LOG_TAG;
#define  LogError(...)  __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

#pragma mark - Jni

Jni::Jni()
: _java(nullptr)
, _env(nullptr)
{
}

Jni::Jni(const Jni& other)
{
    assert(false);
}

Jni::~Jni()
{
    if(!_classes.empty())
    {
        JNIEnv* env = getEnvironment();
        if(env)
        {
            for(ClassMap::const_iterator itr = _classes.begin(); itr != _classes.end(); ++itr)
            {
                env->DeleteGlobalRef(itr->second);
            }
        }
    }
}

Jni& Jni::get()
{
    static Jni jni;
    return jni;
}

JavaVM* Jni::getJava()
{
    return _java;
}

void Jni::setJava(JavaVM* java)
{
    _java = java;
}


JNIEnv* Jni::getEnvironment()
{
    if(!_env)
    {
        if(!_java)
        {
            LogError("JavaVM pointer not set.");
        }
        else if(_java->GetEnv((void**)&_env, JNI_VERSION_1_4) != JNI_OK)
        {
            LogError("Could not get java environment.");
        }
    }
    if(_java->AttachCurrentThread(&_env, nullptr) < 0)
    {
        LogError("Could not attach the java environment to the current thread.");
    }
    return _env;
}

jclass Jni::getClass(const std::string& classPath, bool cache)
{
    ClassMap::const_iterator itr = _classes.find(classPath);
    if(itr != _classes.end())
    {
        return itr->second;
    }
    JNIEnv* env = getEnvironment();
    if(env)
    {
        jclass cls = (jclass)env->FindClass(classPath.c_str());
        if(cls)
        {
            if(cache)
            {
                cls = (jclass)env->NewGlobalRef(cls);
                _classes[classPath] = cls;
                return cls;
            }
            else
            {
                return cls;
            }
        }
        else
        {
            env->ExceptionClear();
            LogError("Could not find class '%s'.", classPath.c_str());
        }
    }
    return nullptr;
}

bool Jni::checkException()
{
    JNIEnv* env = getEnvironment();
    if(env)
    {
        if(env->ExceptionCheck())
        {
            env->ExceptionClear();
            return true;
        }
    }
    return false;
}

#pragma mark - JniObject

JniObject::JniObject(const std::string& classPath, jobject objId, jclass classId)
: _instance(nullptr)
, _class(nullptr)
, _classPath(classPath)
{
    if(objId != nullptr || classId != nullptr || !classPath.empty())
    {
        init(objId, classId);
    }
}

JniObject::JniObject(jclass classId, jobject objId)
: _instance(nullptr)
, _class(nullptr)
{
    if(objId != nullptr || classId != nullptr)
    {
        init(objId, classId);
    }
}

JniObject::JniObject(jobject objId)
: _instance(nullptr)
, _class(nullptr)
{
    if(objId != nullptr)
    {
        init(objId, nullptr);
    }
}

JniObject::JniObject(const JniObject& other)
: _instance(nullptr)
, _class(nullptr)
, _classPath(other._classPath)
{
    init(other._instance, other._class);
}

void JniObject::init(jobject objId, jclass classId)
{
    JNIEnv* env = getEnvironment();
    std::replace(_classPath.begin(), _classPath.end(), '.', '/');
    if(env)
    {
        bool removeLocalClass = false;
        if(!classId)
        {
            if(objId)
            {
                classId = env->GetObjectClass(objId);
                removeLocalClass = true;
            }
            else if(!_classPath.empty())
            {
                classId = Jni::get().getClass(_classPath);
            }
        }
        if(classId)
        {
            _class = (jclass)env->NewGlobalRef(classId);
        }
        if(objId)
        {
            _instance = env->NewGlobalRef(objId);
        }
        if(removeLocalClass)
        {
            env->DeleteLocalRef(classId);
        }
    }
    if(_classPath.empty() && _instance && _class)
    {
        _classPath = JniObject("java/lang/Class", _class).call("getName", std::string());
    }
    if(!_class)
    {
        std::string err("Could not find class");
        if(_classPath.empty())
        {
            err += ".";
        }
        else
        {
            err += " '" + _classPath + "'.";
        }
        setError(err, Jni::kErrorJniFindClass, Jni::kErrorTagJniObject);
    }
}

JniObject::~JniObject()
{
    clear();
}

void JniObject::clear()
{
    JNIEnv* env = getEnvironment();
    if(!env)
    {
        return;
    }
    if(_class)
    {
        env->DeleteGlobalRef(_class);
        _class = nullptr;
    }
    if(_instance)
    {
        env->DeleteGlobalRef(_instance);
        _instance = nullptr;
    }
}

std::string JniObject::getSignature() const
{
    return std::string("L") + getClassPath() + ";";
}

const std::string& JniObject::getError() const
{
    return _error;
}

bool JniObject::hasError() const
{
    return !_error.empty();
}

void JniObject::setError(const std::string& msg, int code, const std::string& tag)
{
    LogError("JniObject %p: %s - %d - %s", this, msg.c_str(), code, tag.c_str());
    _error = msg;
}

const std::string& JniObject::getClassPath() const
{
    return _classPath;
}

JNIEnv* JniObject::getEnvironment()
{
    return Jni::get().getEnvironment();
}

jclass JniObject::getClass() const
{
    return _class;
}

jobject JniObject::getInstance() const
{
    return _instance;
}

jobject JniObject::getNewLocalInstance() const
{
    JNIEnv* env = getEnvironment();
    if(!env)
    {
        return 0;
    }
    return env->NewLocalRef(getInstance());
}

bool JniObject::isInstanceOf(const std::string& classPath) const
{
    std::string fclassPath(classPath);
    std::replace(fclassPath.begin(), fclassPath.end(), '.', '/');
    JNIEnv* env = getEnvironment();
    if(!env)
    {
        return false;
    }
    jclass cls = env->FindClass(fclassPath.c_str());
    return env->IsInstanceOf(getInstance(), cls);
}

JniObject JniObject::findSingleton(const std::string& classPath)
{
    JniObject cls(classPath);

    JniObject obj = cls.staticField("instance", cls);
    if(!obj)
    {
        obj = cls.staticCall("getInstance", cls);
    }
    if(!obj)
    {
        obj.setError("Could not find singleton instance.", Jni::kErrorJniFindSingleton, Jni::kErrorTagJniObject);
    }
    return obj;
}

JniObject::operator bool() const
{
    return getInstance() != nullptr;
}

JniObject& JniObject::operator=(const JniObject& other)
{
    clear();
    _classPath = other._classPath;
    init(other._instance, other._class);
    return *this;
}

bool JniObject::operator==(const JniObject& other) const
{
    JNIEnv* env = getEnvironment();
    if(!env)
    {
        return false;
    }
    jobject a = getInstance();
    jobject b = other.getInstance();
    if(a && b)
    {
        return env->IsSameObject(a, b);
    }
    a = getClass();
    b = other.getClass();
    return env->IsSameObject(a, b);
}

#pragma mark - JniObject::getSignaturePart

template <>
std::string JniObject::getSignaturePart<std::string>(const std::string& val)
{
    return "Ljava/lang/String;";
}

template <>
std::string JniObject::getSignaturePart(const JniObject& val)
{
    return val.getSignature();
}

template <>
std::string JniObject::getSignaturePart(const double& val)
{
    return "D";
}

template <>
std::string JniObject::getSignaturePart(const bool& val)
{
    return "Z";
}

template <>
std::string JniObject::getSignaturePart(const jboolean& val)
{
    return "Z";
}

template <>
std::string JniObject::getSignaturePart(const jbyte& val)
{
    return "B";
}

template <>
std::string JniObject::getSignaturePart(const jchar& val)
{
    return "C";
}

template <>
std::string JniObject::getSignaturePart(const jshort& val)
{
    return "S";
}

template <>
std::string JniObject::getSignaturePart(const jlong& val)
{
    return "J";
}

template <>
std::string JniObject::getSignaturePart(const long& val)
{
    return "J";
}

template <>
std::string JniObject::getSignaturePart(const jint& val)
{
    return "I";
}

template <>
std::string JniObject::getSignaturePart(const unsigned int& val)
{
    return "I";
}

template <>
std::string JniObject::getSignaturePart(const jfloat& val)
{
    return "F";
}

template <>
std::string JniObject::getSignaturePart(const jobject& val)
{
    return JniObject(val).getSignature();
}

std::string JniObject::getSignaturePart()
{
    return "V";
}

template <>
std::string JniObject::getObjectSignaturePart<std::string>(const std::string& val)
{
    return "java/lang/String";
}

template <>
std::string JniObject::getObjectSignaturePart(const JniObject& val)
{
    return val.getClassPath();
}

template <>
std::string JniObject::getObjectSignaturePart(const bool& val)
{
    return "java/lang/Boolean";
}

template <>
std::string JniObject::getObjectSignaturePart(const jboolean& val)
{
    return "java/lang/Boolean";
}

template <>
std::string JniObject::getObjectSignaturePart(const jlong& val)
{
    return "java/lang/Long";
}

template <>
std::string JniObject::getObjectSignaturePart(const long& val)
{
    return "java/lang/Long";
}

template <>
std::string JniObject::getObjectSignaturePart(const double& val)
{
    return "java/lang/Double";
}

template <>
std::string JniObject::getObjectSignaturePart(const char& val)
{
    return "java/lang/Byte";
}

template <>
std::string JniObject::getObjectSignaturePart(const jint& val)
{
    return "java/lang/Integer";
}

template <>
std::string JniObject::getObjectSignaturePart(const unsigned int& val)
{
    return "java/lang/Integer";
}

template <>
std::string JniObject::getObjectSignaturePart(const jfloat& val)
{
    return "java/lang/Float";
}

template <>
std::string JniObject::getObjectSignaturePart(const jobject& val)
{
    return JniObject(val).getClassPath();
}

#pragma mark - JniObject::convertToJavaValue

template <>
jvalue JniObject::convertToJavaValue(const bool& obj)
{
    jvalue val;
    val.z = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const bool& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const jboolean& obj)
{
    jvalue val;
    val.z = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const jboolean& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const jbyte& obj)
{
    jvalue val;
    val.b = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const jbyte& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const jchar& obj)
{
    jvalue val;
    val.c = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const jchar& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const jshort& obj)
{
    jvalue val;
    val.s = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const jshort& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const jint& obj)
{
    jvalue val;
    val.i = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const jint& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const unsigned int& obj)
{
    jvalue val;
    val.i = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const unsigned int& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const long& obj)
{
    jvalue val;
    val.j = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const long& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const jlong& obj)
{
    jvalue val;
    val.j = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const jlong& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const jfloat& obj)
{
    jvalue val;
    val.f = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const jfloat& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const jdouble& obj)
{
    jvalue val;
    val.d = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const jdouble& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const jobject& obj)
{
    jvalue val;
    val.l = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const jobject& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const JniObject& obj)
{
    return convertToJavaValue(obj.getInstance());
}
template <>
bool JniObject::isJobjectArgument(const JniObject& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const jarray& obj)
{
    jvalue val;
    val.l = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const jarray& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const jstring& obj)
{
    jvalue val;
    val.l = obj;
    return val;
}
template <>
bool JniObject::isJobjectArgument(const jstring& obj)
{
    return false;
}

template <>
jvalue JniObject::convertToJavaValue(const std::string& obj)
{
    JNIEnv* env = getEnvironment();
    if(!env)
    {
        return jvalue();
    }
    return convertToJavaValue(env->NewStringUTF(obj.c_str()));
}
template <>
bool JniObject::isJobjectArgument(const std::string& obj)
{
    return true;
}

#pragma mark - JniObject::convertFromJavaObject

template <>
bool JniObject::convertFromJavaObject(JNIEnv* env, jobject obj, std::string& out)
{
    if(!obj)
    {
        out = "";
        return true;
    }
    jstring jstr = (jstring)obj;
    const char* chars = env->GetStringUTFChars(jstr, NULL);
    if(!chars)
    {
        return false;
    }
    out = chars;
    env->ReleaseStringUTFChars(jstr, chars);
    env->DeleteLocalRef(obj);
    return true;
}

template <>
bool JniObject::convertFromJavaObject(JNIEnv* env, jobject obj, JniObject& out)
{
    out = obj;
    env->DeleteLocalRef(obj);
    return true;
}

#pragma mark - JniObject call jni

template <>
void JniObject::callStaticJavaMethod(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
{
    return env->CallStaticVoidMethodA(classId, methodId, args);
}

template <>
jobject JniObject::callStaticJavaMethod(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
{
    return env->CallStaticObjectMethodA(classId, methodId, args);
}

template <>
double JniObject::callStaticJavaMethod(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
{
    return env->CallStaticDoubleMethodA(classId, methodId, args);
}

template <>
long JniObject::callStaticJavaMethod(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
{
    return env->CallStaticLongMethodA(classId, methodId, args);
}


template <>
jlong JniObject::callStaticJavaMethod(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
{
    return env->CallStaticLongMethodA(classId, methodId, args);
}

template <>
float JniObject::callStaticJavaMethod(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
{
    return env->CallStaticFloatMethodA(classId, methodId, args);
}

template <>
int JniObject::callStaticJavaMethod(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
{
    return env->CallStaticIntMethodA(classId, methodId, args);
}

template <>
bool JniObject::callStaticJavaMethod(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
{
    return env->CallStaticBooleanMethodA(classId, methodId, args);
}

template <>
std::string JniObject::callStaticJavaMethod(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
{
    return convertFromJavaObject<std::string>(env, callStaticJavaMethod<jobject>(env, classId, methodId, args));
}

template <>
JniObject JniObject::callStaticJavaMethod(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
{
    return convertFromJavaObject<JniObject>(env, callStaticJavaMethod<jobject>(env, classId, methodId, args));
}

void JniObject::callJavaVoidMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args)
{
    env->CallVoidMethodA(objId, methodId, args);
}

template <>
void JniObject::callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, jobject& out)
{
    out = env->CallObjectMethodA(objId, methodId, args);
}

template <>
void JniObject::callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, double& out)
{
    out = env->CallDoubleMethodA(objId, methodId, args);
}

template <>
void JniObject::callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, bool& out)
{
    out = env->CallBooleanMethodA(objId, methodId, args);
}

template <>
void JniObject::callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, long& out)
{
    out = env->CallLongMethodA(objId, methodId, args);
}

template <>
void JniObject::callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, jlong& out)
{
    out = env->CallLongMethodA(objId, methodId, args);
}

template <>
void JniObject::callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, float& out)
{
    out = env->CallFloatMethodA(objId, methodId, args);
}

template <>
void JniObject::callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, int& out)
{
    out = env->CallIntMethodA(objId, methodId, args);
}

template <>
void JniObject::callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, std::string& out)
{
    callJavaObjectMethod(env, objId, methodId, args, out);
}

template <>
void JniObject::callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, JniObject& out)
{
    callJavaObjectMethod(env, objId, methodId, args, out);
}

template <>
jobject JniObject::getJavaStaticField(JNIEnv* env, jclass classId, jfieldID fieldId)
{
    return env->GetStaticObjectField(classId, fieldId);
}

template <>
double JniObject::getJavaStaticField(JNIEnv* env, jclass classId, jfieldID fieldId)
{
    return env->GetStaticDoubleField(classId, fieldId);
}

template <>
long JniObject::getJavaStaticField(JNIEnv* env, jclass classId, jfieldID fieldId)
{
    return env->GetStaticLongField(classId, fieldId);
}

template <>
jlong JniObject::getJavaStaticField(JNIEnv* env, jclass classId, jfieldID fieldId)
{
    return env->GetStaticLongField(classId, fieldId);
}

template <>
float JniObject::getJavaStaticField(JNIEnv* env, jclass classId, jfieldID fieldId)
{
    return env->GetStaticFloatField(classId, fieldId);
}

template <>
int JniObject::getJavaStaticField(JNIEnv* env, jclass classId, jfieldID fieldId)
{
    return env->GetStaticIntField(classId, fieldId);
}

template <>
std::string JniObject::getJavaStaticField(JNIEnv* env, jclass classId, jfieldID fieldId)
{
    return convertFromJavaObject<std::string>(getJavaStaticField<jobject>(env, classId, fieldId));
}

template <>
JniObject JniObject::getJavaStaticField(JNIEnv* env, jclass classId, jfieldID fieldId)
{
    return convertFromJavaObject<JniObject>(getJavaStaticField<jobject>(env, classId, fieldId));
}

template <>
jobject JniObject::getJavaField(JNIEnv* env, jobject objId, jfieldID fieldId)
{
    return env->GetObjectField(objId, fieldId);
}

template <>
double JniObject::getJavaField(JNIEnv* env, jobject objId, jfieldID fieldId)
{
    return env->GetDoubleField(objId, fieldId);
}

template <>
long JniObject::getJavaField(JNIEnv* env, jobject objId, jfieldID fieldId)
{
    return env->GetLongField(objId, fieldId);
}

template <>
float JniObject::getJavaField(JNIEnv* env, jobject objId, jfieldID fieldId)
{
    return env->GetFloatField(objId, fieldId);
}

template <>
int JniObject::getJavaField(JNIEnv* env, jobject objId, jfieldID fieldId)
{
    return env->GetIntField(objId, fieldId);
}

template <>
std::string JniObject::getJavaField(JNIEnv* env, jobject objId, jfieldID fieldId)
{
    return convertFromJavaObject<std::string>(getJavaField<jobject>(env, objId, fieldId));
}

template <>
JniObject JniObject::getJavaField(JNIEnv* env, jobject objId, jfieldID fieldId)
{
    return convertFromJavaObject<JniObject>(getJavaField<jobject>(env, objId, fieldId));
}

template <>
jarray JniObject::createJavaArray(JNIEnv* env, const jobject& element, size_t size)
{
    jclass elmClass = env->GetObjectClass(element);
    return env->NewObjectArray(size, elmClass, 0);
}

template <>
jarray JniObject::createJavaArray(JNIEnv* env, const double& element, size_t size)
{
    return env->NewDoubleArray(size);
}

template <>
jarray JniObject::createJavaArray(JNIEnv* env, const long& element, size_t size)
{
    return env->NewLongArray(size);
}


template <>
jarray JniObject::createJavaArray(JNIEnv* env, const jlong& element, size_t size)
{
    return env->NewLongArray(size);
}

template <>
jarray JniObject::createJavaArray(JNIEnv* env, const float& element, size_t size)
{
    return env->NewFloatArray(size);
}

template <>
jarray JniObject::createJavaArray(JNIEnv* env, const int& element, size_t size)
{
    return env->NewLongArray(size);
}

template <>
jarray JniObject::createJavaArray(JNIEnv* env, const std::string& element, size_t size)
{
    jclass elmClass = env->FindClass("java/lang/String");
    return env->NewObjectArray(size, elmClass, 0);
}

template <>
jarray JniObject::createJavaArray(JNIEnv* env, const JniObject& element, size_t size)
{
    jclass elmClass = element.getClass();
    return env->NewObjectArray(size, elmClass, 0);
}

template <>
jobject JniObject::getJavaArrayElement(JNIEnv* env, jarray arr, size_t position)
{
    return env->GetObjectArrayElement((jobjectArray)arr, position);
}

template <>
double JniObject::getJavaArrayElement(JNIEnv* env, jarray arr, size_t position)
{
    jdouble ret = 0.0;
    env->GetDoubleArrayRegion((jdoubleArray)arr, position, 1, &ret);
    return ret;
}

template <>
long JniObject::getJavaArrayElement(JNIEnv* env, jarray arr, size_t position)
{
    jlong ret = 0;
    env->GetLongArrayRegion((jlongArray)arr, position, 1, &ret);
    return ret;
}

template <>
jlong JniObject::getJavaArrayElement(JNIEnv* env, jarray arr, size_t position)
{
    jlong ret = 0;
    env->GetLongArrayRegion((jlongArray)arr, position, 1, &ret);
    return ret;
}

template <>
float JniObject::getJavaArrayElement(JNIEnv* env, jarray arr, size_t position)
{
    jfloat ret = 0.0f;
    env->GetFloatArrayRegion((jfloatArray)arr, position, 1, &ret);
    return ret;
}

template <>
int JniObject::getJavaArrayElement(JNIEnv* env, jarray arr, size_t position)
{
    jint ret = 0;
    env->GetIntArrayRegion((jintArray)arr, position, 1, &ret);
    return ret;
}

template <>
char JniObject::getJavaArrayElement(JNIEnv* env, jarray arr, size_t position)
{
    jbyte ret = 0;
    env->GetByteArrayRegion((jbyteArray)arr, position, 1, &ret);
    return ret;
}

template <>
std::string JniObject::getJavaArrayElement(JNIEnv* env, jarray arr, size_t position)
{
    jobject obj = getJavaArrayElement<jobject>(env, arr, position);
    return convertFromJavaObject<std::string>(env, obj);
}

template <>
std::vector<char> JniObject::getJavaArrayElement(JNIEnv* env, jarray arr, size_t position)
{
    jobject obj = getJavaArrayElement<jobject>(env, arr, position);
    return convertFromJavaObject<std::vector<char>>(env, obj);
}

template <>
JniObject JniObject::getJavaArrayElement(JNIEnv* env, jarray arr, size_t position)
{
    jobject obj = getJavaArrayElement<jobject>(env, arr, position);
    return convertFromJavaObject<JniObject>(env, obj);
}

template <>
void JniObject::setJavaArrayElement(JNIEnv* env, jarray arr, size_t position, const jobject& elm)
{
    env->SetObjectArrayElement((jobjectArray)arr, position, elm);
}

template <>
void JniObject::setJavaArrayElement(JNIEnv* env, jarray arr, size_t position, const double& elm)
{
    env->SetDoubleArrayRegion((jdoubleArray)arr, position, 1, &elm);
}

template <>
void JniObject::setJavaArrayElement(JNIEnv* env, jarray arr, size_t position, const long& elm)
{
    jlong jelm = elm;
    env->SetLongArrayRegion((jlongArray)arr, position, 1, &jelm);
}

template <>
void JniObject::setJavaArrayElement(JNIEnv* env, jarray arr, size_t position, const jlong& elm)
{
    jlong jelm = elm;
    env->SetLongArrayRegion((jlongArray)arr, position, 1, &jelm);
}

template <>
void JniObject::setJavaArrayElement(JNIEnv* env, jarray arr, size_t position, const float& elm)
{
    env->SetFloatArrayRegion((jfloatArray)arr, position, 1, &elm);
}

template <>
void JniObject::setJavaArrayElement(JNIEnv* env, jarray arr, size_t position, const int& elm)
{
    env->SetIntArrayRegion((jintArray)arr, position, 1, &elm);
}

template <>
void JniObject::setJavaArrayElement(JNIEnv* env, jarray arr, size_t position, const std::string& elm)
{
    jobject obj = env->NewStringUTF(elm.c_str());
    setJavaArrayElement(env, arr, position, obj);
    env->DeleteLocalRef(obj);
}

template <>
void JniObject::setJavaArrayElement(JNIEnv* env, jarray arr, size_t position, const JniObject& elm)
{
    setJavaArrayElement(env, arr, position, elm.getInstance());
}

template <>
void JniObject::objectCast(JNIEnv* env, jobject obj, int& out)
{
    JniObject iobj(getObjectSignaturePart<int>(0), obj);
    out = iobj.call("intValue", 0);
}
template <>
void JniObject::objectCast(JNIEnv* env, jobject obj, float& out)
{
    JniObject iobj(getObjectSignaturePart<float>(0.f), obj);
    out = iobj.call("floatValue", 0.f);
}
template <>
void JniObject::objectCast(JNIEnv* env, jobject obj, double& out)
{
    JniObject iobj(getObjectSignaturePart<double>(0.0), obj);
    out = iobj.call("doubleValue", 0.0);
}
template <>
void JniObject::objectCast(JNIEnv* env, jobject obj, long& out)
{
    JniObject iobj(getObjectSignaturePart<long>(0L), obj);
    out = iobj.call("longValue", 0L);
}
template <>
void JniObject::objectCast(JNIEnv* env, jobject obj, char& out)
{
    JniObject iobj(getObjectSignaturePart<char>(0), obj);
    out = iobj.call("charValue", 0L);
}
template <>
void JniObject::objectCast(JNIEnv* env, jobject obj, std::string& out)
{
    jstring jstr = (jstring)obj;
    const char* chars = env->GetStringUTFChars(jstr, NULL);
    if(chars)
    {
        out = chars;
        env->ReleaseStringUTFChars(jstr, chars);
    }
}
