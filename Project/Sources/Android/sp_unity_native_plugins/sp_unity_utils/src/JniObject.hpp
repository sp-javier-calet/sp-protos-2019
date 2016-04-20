#ifndef __JniObject__
#define __JniObject__

#include <jni.h>
#include <string>
#include <sstream>
#include <vector>
#include <map>
#include <array>
#include <list>
#include <set>
#include <cassert>

/**
 * A singleton to pass the JavaVM* in the jni main function
 * and preload classes
 */
class Jni
{
  private:
    typedef std::map<std::string, jclass> ClassMap;
    JavaVM* _java;
    JNIEnv* _env;
    ClassMap _classes;

    Jni();
    Jni(const Jni& other);

  public:
    static const int kErrorJniFindCreate;
    static const int kErrorJniCallCreate;
    static const int kErrorJniFindMethod;
    static const int kErrorJniCallMethod;
    static const int kErrorJniFindField;
    static const int kErrorJniCallField;
    static const int kErrorJniFindClass;
    static const int kErrorJniFindSingleton;

    static const char* kErrorTagJniObject;

    ~Jni();

    /**
     * This class is a singleton
     */
    static Jni& get();

    /**
     * Set the java virtual machine pointer
     */
    void setJava(JavaVM* java);

    /**
     * Get the java virtual machine pointer
     */
    JavaVM* getJava();

    /**
     * Get the java environment pointer
     * Will attatch to the current thread automatically
     */
    JNIEnv* getEnvironment();

    /**
     * get a class, will be stored in the class cache
     */
    jclass getClass(const std::string& classPath, bool cache = true);

    /**
     * Check if the has been an exception.
     * If an exception is found the exception is cleaned
     */
    bool checkException();
};

/**
 * This class represents a jni object
 */
class JniObject
{
  private:
    jclass _class;
    jobject _instance;
    std::string _error;
    std::string _classPath;

    template <typename Arg, typename... Args>
    static void buildSignature(std::ostringstream& os, const Arg& arg, const Args&... args)
    {
        os << getSignaturePart(arg);
        buildSignature(os, args...);
    }

    static void buildSignature(std::ostringstream& os)
    {
    }

    template <typename Return, typename... Args>
    static std::string createSignature(const Return& ret, const Args&... args)
    {
        std::ostringstream os;
        os << "(";
        buildSignature(os, args...);
        os << ")" << getSignaturePart(ret);
        return os.str();
    }

    template <typename Return, typename... Args>
    static std::string createObjectSignature(const Return& ret, const Args&... args)
    {
        std::ostringstream os;
        os << "(";
        buildSignature(os, args...);
        os << ")"
           << "L" << getObjectSignaturePart(ret) << ";";
        return os.str();
    }

    template <typename... Args>
    static std::string createVoidSignature(const Args&... args)
    {
        std::ostringstream os;
        os << "(";
        buildSignature(os, args...);
        os << ")" << getSignaturePart();
        return os.str();
    }

    template <typename... Args>
    static jvalue* createArguments(const Args&... args)
    {
        size_t size = sizeofArguments<Args...>();
        jvalue* jargs = (jvalue*)malloc(size);
        buildArguments(jargs, 0, args...);
        return jargs;
    }

    static jvalue* createArguments()
    {
        return nullptr;
    }

    template <typename Arg1, typename Arg2, typename... Args>
    static size_t sizeofArguments()
    {
        return sizeof(jvalue) + sizeofArguments<Arg2, Args...>();
    }

    template <typename Arg>
    static size_t sizeofArguments()
    {
        return sizeof(jvalue);
    }

    template <typename Arg1, typename Arg2, typename... Args>
    static void buildArguments(jvalue* jargs, unsigned pos, const Arg1& arg1, const Arg2& arg2, const Args&... args)
    {
        jargs[pos] = convertToJavaValue(arg1);
        buildArguments(jargs, pos + 1, arg2, args...);
    }

    template <typename Arg>
    static void buildArguments(jvalue* jargs, unsigned pos, const Arg& arg)
    {
        jargs[pos] = convertToJavaValue(arg);
    }

    template <typename... Args>
    static void findJobjectArguments(std::vector<jobject*>& jobjs, jvalue* jargs, const Args&... args)
    {
        processJobjectArguments(jobjs, jargs, 0, args...);
    }

    template <typename Arg, typename... Args>
    static void processJobjectArguments(std::vector<jobject*>& jobjs, jvalue* jargs, unsigned pos, const Arg& arg, const Args&... args)
    {
        if(isJobjectArgument(arg))
        {
            jobjs.push_back(&jargs[pos].l);
        }

        processJobjectArguments(jobjs, jargs, pos + 1, args...);
    }

    static void processJobjectArguments(std::vector<jobject*>& jobjs, jvalue* jargs, unsigned pos)
    {
    }

    /**
     * Return the signature for the given type
     */
    template <typename Type>
    static std::string getSignaturePart(const Type& type);

    template <typename Type>
    static std::string getObjectSignaturePart(const Type& type);

    /**
    * Return the signature for the given container element
    */
    template <typename Type>
    static std::string getContainerElementSignaturePart(const Type& container)
    {
        if(container.empty())
        {
            return getSignaturePart(typename Type::value_type());
        }
        else
        {
            return getSignaturePart(*container.begin());
        }
    }

    // template specialization for pointers
    template <typename Type>
    static std::string getSignaturePart(Type* val)
    {
        return getSignaturePart((jlong)val);
    }

    // template specialization for containers
    template <typename Type>
    static std::string getSignaturePart(const std::vector<Type>& val)
    {
        return std::string("[") + getContainerElementSignaturePart(val);
    }
    // template specialization for containers
    template <typename Type>
    static std::string getObjectSignaturePart(const std::vector<Type>& val)
    {
        return std::string("java/util/Collection");
    }
    template <typename Key, typename Value>
    static std::string getObjectSignaturePart(const std::map<Key, Value>& val)
    {
        return std::string("java/util/Map");
    }
    template <typename Type>
    static std::string getSignaturePart(const std::set<Type>& val)
    {
        return std::string("[") + getContainerElementSignaturePart(val);
    }
    template <typename Type, int Size>
    static std::string getSignaturePart(const std::array<Type, Size>& val)
    {
        return std::string("[") + getContainerElementSignaturePart(val);
    }
    template <typename Type>
    static std::string getSignaturePart(const std::list<Type>& val)
    {
        return std::string("[") + getContainerElementSignaturePart(val);
    }
    template <typename Key, typename Value>
    static std::string getSignaturePart(const std::map<Key, Value>& val)
    {
        return "Ljava/util/Map;";
    }

    /**
     * Return the signature for the void type
     */
    static std::string getSignaturePart();

    template <typename Return>
    Return callStaticJavaMethod(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args);

    template <typename Return>
    std::vector<Return> callStaticJavaMethodVector(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
    {
        jobject temp = callStaticJavaMethod<jobject>(env, classId, methodId, args);
        std::vector<Return> result = convertFromJavaVectorOfObjects<Return>(env, temp);
        env->DeleteLocalRef(temp);
        return result;
    }

    template <typename KeyReturn, typename ValueReturn>
    std::map<KeyReturn, ValueReturn> callStaticJavaMethodMap(JNIEnv* env, jclass classId, jmethodID methodId, jvalue* args)
    {
        jobject temp = callStaticJavaMethod<jobject>(env, classId, methodId, args);
        std::map<KeyReturn, ValueReturn> result;
        result = convertFromJavaMapOfObjects<KeyReturn, ValueReturn>(env, temp);
        env->DeleteLocalRef(temp);
        return result;
    }

    template <typename Return>
    std::vector<Return> callJavaMethodVector(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args)
    {
        jobject jout = nullptr;
        callJavaMethod<jobject>(env, objId, methodId, args, jout);
        std::vector<Return> result = convertFromJavaVectorOfObjects<Return>(env, jout);
        env->DeleteLocalRef(jout);
        return result;
    }

    template <typename KeyReturn, typename ValueReturn>
    std::map<KeyReturn, ValueReturn> callJavaMethodMap(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args)
    {
        jobject jout = nullptr;
        callJavaMethod<jobject>(env, objId, methodId, args, jout);
        std::map<KeyReturn, ValueReturn> result = convertFromJavaMapOfObjects<KeyReturn, ValueReturn>(env, jout);
        env->DeleteLocalRef(jout);
        return result;
    }

    void callJavaVoidMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args);

    template <typename Return>
    void callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, Return& out);

    template <typename Return>
    void callJavaObjectMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, Return& out)
    {
        jobject jout = nullptr;
        callJavaMethod(env, objId, methodId, args, jout);
        out = convertFromJavaObject<Return>(jout);
    }

    template <typename Type>
    void callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, std::vector<Type>& out)
    {
        callJavaObjectMethod(env, objId, methodId, args, out);
    }

    template <typename Key, typename Value>
    void callJavaMethod(JNIEnv* env, jobject objId, jmethodID methodId, jvalue* args, std::map<Key, Value>& out)
    {
        callJavaObjectMethod(env, objId, methodId, args, out);
    }

    template <typename Return>
    Return getJavaStaticField(JNIEnv* env, jclass classId, jfieldID fieldId);

    template <typename Return>
    Return getJavaField(JNIEnv* env, jobject objId, jfieldID fieldId);

    void setError(const std::string& msg, int code, const std::string& tag);

  public:
    JniObject(const std::string& classPath, jobject javaObj = nullptr, jclass classId = nullptr);
    JniObject(jclass classId, jobject javaObj);
    JniObject(jobject javaObj = nullptr);
    JniObject(const JniObject& other);
    void init(jobject javaObj, jclass classId);
    ~JniObject();

    /**
     * Clear the retained global references
     */
    void clear();

    /**
     * Find a singleton instance
     * will try the `instance` static field and a `getInstance` static method
     */
    static JniObject findSingleton(const std::string& classPath);

    /**
     * Create a new JniObject
     */
    template <typename... Args>
    static JniObject createNew(const std::string& classPath, Args&&... args)
    {
        JniObject defRet(classPath);
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return defRet;
        }
        jclass classId = Jni::get().getClass(classPath);
        if(!classId)
        {
            return defRet;
        }
        std::string signature(createVoidSignature<Args...>(args...));
        jmethodID methodId = env->GetMethodID(classId, "<init>", signature.c_str());
        if(!methodId || env->ExceptionCheck())
        {
            env->ExceptionClear();
            defRet.setError(std::string("Failed to find constructor '" + classPath + "' with signature '" + signature + "'."),
                            Jni::kErrorJniFindCreate, Jni::kErrorTagJniObject);
        }
        else
        {
            jvalue* jargs = createArguments(args...);
            jobject obj = env->NewObjectA(classId, methodId, jargs);

            std::vector<jobject*> jobjs;
            findJobjectArguments(jobjs, jargs, args...);

            for(jobject* jobj : jobjs)
            {
                env->DeleteLocalRef(*jobj);
            }

            if(env->ExceptionCheck())
            {
                env->ExceptionClear();
                defRet.setError(std::string("Failed to call constructor '" + classPath + "' with signature '" + signature + "'."),
                                Jni::kErrorJniCallCreate, Jni::kErrorTagJniObject);
            }
            else
            {
                defRet = JniObject(classPath, obj, classId);
            }
        }
        return defRet;
    }

    /**
     * Calls an object method
     */
    template <typename Return, typename... Args>
    Return call(const std::string& name, const Return& defRet, Args&&... args)
    {
        std::string signature(createSignature(defRet, args...));
        return callSigned(name, signature, defRet, args...);
    }

    template <typename Return, typename... Args>
    Return callSigned(const std::string& name, const std::string& signature, const Return& defRet, Args&&... args)
    {
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return defRet;
        }
        jclass classId = getClass();
        if(!classId)
        {
            return defRet;
        }
        jobject objId = getInstance();
        if(!objId)
        {
            assert(false && "Calling method without java object instance");
            return defRet;
        }
        jmethodID methodId = env->GetMethodID(classId, name.c_str(), signature.c_str());
        if(!methodId || env->ExceptionCheck())
        {
            env->ExceptionClear();
            setError(std::string("Failed to find method '") + name + "' with signature '" + signature + "'.", Jni::kErrorJniFindMethod,
                     Jni::kErrorTagJniObject);
            return defRet;
        }
        else
        {
            jvalue* jargs = createArguments(args...);
            Return result;
            callJavaMethod(env, objId, methodId, jargs, result);

            std::vector<jobject*> jobjs;
            findJobjectArguments(jobjs, jargs, args...);

            for(jobject* jobj : jobjs)
            {
                env->DeleteLocalRef(*jobj);
            }

            if(env->ExceptionCheck())
            {
                env->ExceptionClear();
                setError(std::string("Failed to call method '") + name + " with signature '" + signature + "'.", Jni::kErrorJniCallMethod,
                         Jni::kErrorTagJniObject);
                return defRet;
            }
            else
            {
                return result;
            }
        }
    }

    /**
     * Calls an object method
     */
    template <typename Return, typename... Args>
    std::vector<Return> callVector(const std::string& name, Args&&... args)
    {
        std::string signature(createObjectSignature(std::vector<Return>(), args...));
        return callSignedVector<Return, Args...>(name, signature, args...);
    }

    template <typename Return, typename... Args>
    std::vector<Return> callSignedVector(const std::string& name, const std::string& signature, Args&&... args)
    {
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return std::vector<Return>();
        }
        jclass classId = getClass();
        if(!classId)
        {
            return std::vector<Return>();
        }
        jobject objId = getInstance();
        if(!objId)
        {
            assert(false && "Calling method without java object instance");
            return std::vector<Return>();
        }
        jmethodID methodId = env->GetMethodID(classId, name.c_str(), signature.c_str());
        if(!methodId || env->ExceptionCheck())
        {
            env->ExceptionClear();
            setError(std::string("Failed to find method '") + name + "' with signature '" + signature + "'.", Jni::kErrorJniFindMethod,
                     Jni::kErrorTagJniObject);
            return std::vector<Return>();
        }
        else
        {
            jvalue* jargs = createArguments(args...);
            std::vector<Return> result;
            result = callJavaMethodVector<Return>(env, objId, methodId, jargs);

            std::vector<jobject*> jobjs;
            findJobjectArguments(jobjs, jargs, args...);

            for(jobject* jobj : jobjs)
            {
                env->DeleteLocalRef(*jobj);
            }

            if(env->ExceptionCheck())
            {
                env->ExceptionClear();
                setError(std::string("Failed to call method '") + name + " with signature '" + signature + "'.", Jni::kErrorJniCallMethod,
                         Jni::kErrorTagJniObject);
                return std::vector<Return>();
            }
            else
            {
                return result;
            }
        }
    }
    /**
     * Calls an object method
     */
    template <typename KeyReturn, typename ValueReturn, typename... Args>
    std::map<KeyReturn, ValueReturn> callMap(const std::string& name, Args&&... args)
    {
        std::string signature(createObjectSignature(std::map<KeyReturn, ValueReturn>(), args...));
        return callSignedMap<KeyReturn, ValueReturn, Args...>(name, signature, args...);
    }

    template <typename KeyReturn, typename ValueReturn, typename... Args>
    std::map<KeyReturn, ValueReturn> callSignedMap(const std::string& name, const std::string& signature, Args&&... args)
    {
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return std::map<KeyReturn, ValueReturn>();
        }
        jclass classId = getClass();
        if(!classId)
        {
            return std::map<KeyReturn, ValueReturn>();
        }
        jobject objId = getInstance();
        if(!objId)
        {
            assert(false && "Calling method without java object instance");
            return std::map<KeyReturn, ValueReturn>();
        }
        jmethodID methodId = env->GetMethodID(classId, name.c_str(), signature.c_str());
        if(!methodId || env->ExceptionCheck())
        {
            env->ExceptionClear();
            setError(std::string("Failed to find method '") + name + "' with signature '" + signature + "'.", Jni::kErrorJniFindMethod,
                     Jni::kErrorTagJniObject);
            return std::map<KeyReturn, ValueReturn>();
        }
        else
        {
            jvalue* jargs = createArguments(args...);
            std::map<KeyReturn, ValueReturn> result;
            result = callJavaMethodMap<KeyReturn, ValueReturn>(env, objId, methodId, jargs);

            std::vector<jobject*> jobjs;
            findJobjectArguments(jobjs, jargs, args...);

            for(jobject* jobj : jobjs)
            {
                env->DeleteLocalRef(*jobj);
            }

            if(env->ExceptionCheck())
            {
                env->ExceptionClear();
                setError(std::string("Failed to call method '") + name + " with signature '" + signature + "'.", Jni::kErrorJniCallMethod,
                         Jni::kErrorTagJniObject);
                return std::map<KeyReturn, ValueReturn>();
            }
            else
            {
                return result;
            }
        }
    }

    /**
     * Calls an object void method
     */
    template <typename... Args>
    void callVoid(const std::string& name, Args&&... args)
    {
        std::string signature(createVoidSignature(args...));
        return callSignedVoid(name, signature, args...);
    }

    template <typename... Args>
    void callSignedVoid(const std::string& name, const std::string& signature, Args&&... args)
    {
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return;
        }
        jclass classId = getClass();
        if(!classId)
        {
            return;
        }
        jobject objId = getInstance();
        if(!objId)
        {
            assert(false && "Calling method without java object instance");
            return;
        }
        jmethodID methodId = env->GetMethodID(classId, name.c_str(), signature.c_str());
        if(!methodId || env->ExceptionCheck())
        {
            env->ExceptionClear();
            setError(std::string("Failed to find method '") + name + "' with signature '" + signature + "'.", Jni::kErrorJniFindMethod,
                     Jni::kErrorTagJniObject);
        }
        else
        {
            jvalue* jargs = createArguments(args...);
            callJavaVoidMethod(env, objId, methodId, jargs);

            std::vector<jobject*> jobjs;
            findJobjectArguments(jobjs, jargs, args...);

            for(jobject* jobj : jobjs)
            {
                env->DeleteLocalRef(*jobj);
            }

            if(env->ExceptionCheck())
            {
                env->ExceptionClear();
                setError(std::string("Failed to call method '") + name + "' with signature '" + signature + "'.", Jni::kErrorJniCallMethod,
                         Jni::kErrorTagJniObject);
            }
        }
    }

    /**
     * Calls a class method
     */
    template <typename Return, typename... Args>
    Return staticCall(const std::string& name, const Return& defRet, Args&&... args)
    {
        std::string signature(createSignature(defRet, args...));
        return staticCallSigned(name, signature, defRet, args...);
    }

    template <typename Return, typename... Args>
    Return staticCallSigned(const std::string& name, const std::string& signature, const Return& defRet, Args&&... args)
    {
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return defRet;
        }
        jclass classId = getClass();
        if(!classId)
        {
            return defRet;
        }
        jmethodID methodId = env->GetStaticMethodID(classId, name.c_str(), signature.c_str());
        if(!methodId || env->ExceptionCheck())
        {
            env->ExceptionClear();
            setError(std::string("Failed to find static method '") + name + "'.", Jni::kErrorJniFindMethod, Jni::kErrorTagJniObject);
            return defRet;
        }
        else
        {
            jvalue* jargs = createArguments(args...);
            Return result = callStaticJavaMethod<Return>(env, classId, methodId, jargs);

            std::vector<jobject*> jobjs;
            findJobjectArguments(jobjs, jargs, args...);

            for(jobject* jobj : jobjs)
            {
                env->DeleteLocalRef(*jobj);
            }

            if(env->ExceptionCheck())
            {
                env->ExceptionClear();
                setError(std::string("Failed to call static method '") + name + "'.", Jni::kErrorJniCallMethod, Jni::kErrorTagJniObject);
                return defRet;
            }
            else
            {
                return result;
            }
        }
    }

    /**
     * Calls a class method
     */
    template <typename Return, typename... Args>
    std::vector<Return> staticCallVector(const std::string& name, Args&&... args)
    {
        std::string signature(createObjectSignature(std::vector<Return>(), args...));
        return staticCallSignedVector<Return, Args...>(name, signature, args...);
    }

    template <typename Return, typename... Args>
    std::vector<Return> staticCallSignedVector(const std::string& name, const std::string& signature, Args&&... args)
    {
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return std::vector<Return>();
        }
        jclass classId = getClass();
        if(!classId)
        {
            return std::vector<Return>();
        }
        jmethodID methodId = env->GetStaticMethodID(classId, name.c_str(), signature.c_str());
        if(!methodId || env->ExceptionCheck())
        {
            env->ExceptionClear();
            setError(std::string("Failed to find static method '") + name + "'.", Jni::kErrorJniFindMethod, Jni::kErrorTagJniObject);
            return std::vector<Return>();
        }
        else
        {
            jvalue* jargs = createArguments(args...);
            std::vector<Return> result = callStaticJavaMethodVector<Return>(env, classId, methodId, jargs);

            std::vector<jobject*> jobjs;
            findJobjectArguments(jobjs, jargs, args...);

            for(jobject* jobj : jobjs)
            {
                env->DeleteLocalRef(*jobj);
            }

            if(env->ExceptionCheck())
            {
                env->ExceptionClear();
                setError(std::string("Failed to call static method '") + name + "'.", Jni::kErrorJniCallMethod, Jni::kErrorTagJniObject);
                return std::vector<Return>();
            }
            else
            {
                return result;
            }
        }
    }

    /**
     * Calls a class method
     */
    template <typename KeyReturn, typename ValueReturn, typename... Args>
    std::map<KeyReturn, ValueReturn> staticCallMap(const std::string& name, Args&&... args)
    {
        std::string signature(createObjectSignature(std::map<KeyReturn, ValueReturn>(), args...));
        return staticCallSignedMap<KeyReturn, ValueReturn, Args...>(name, signature, args...);
    }

    template <typename KeyReturn, typename ValueReturn, typename... Args>
    std::map<KeyReturn, ValueReturn> staticCallSignedMap(const std::string& name, const std::string& signature, Args&&... args)
    {
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return std::map<KeyReturn, ValueReturn>();
        }
        jclass classId = getClass();
        if(!classId)
        {
            return std::map<KeyReturn, ValueReturn>();
        }
        jmethodID methodId = env->GetStaticMethodID(classId, name.c_str(), signature.c_str());
        if(!methodId || env->ExceptionCheck())
        {
            env->ExceptionClear();
            setError(std::string("Failed to find static method '") + name + "'.", Jni::kErrorJniFindMethod, Jni::kErrorTagJniObject);
            return std::map<KeyReturn, ValueReturn>();
        }
        else
        {
            jvalue* jargs = createArguments(args...);
            std::map<KeyReturn, ValueReturn> result = callStaticJavaMethodMap<KeyReturn, ValueReturn>(env, classId, methodId, jargs);

            std::vector<jobject*> jobjs;
            findJobjectArguments(jobjs, jargs, args...);

            for(jobject* jobj : jobjs)
            {
                env->DeleteLocalRef(*jobj);
            }

            if(env->ExceptionCheck())
            {
                env->ExceptionClear();
                setError(std::string("Failed to call static method '") + name + "'.", Jni::kErrorJniCallMethod, Jni::kErrorTagJniObject);
                return std::map<KeyReturn, ValueReturn>();
            }
            else
            {
                return result;
            }
        }
    }


    /**
     * Calls a class void method
     */
    template <typename... Args>
    void staticCallVoid(const std::string& name, Args&&... args)
    {
        std::string signature(createVoidSignature(args...));
        return staticCallSignedVoid(name, signature, args...);
    }

    template <typename... Args>
    void staticCallSignedVoid(const std::string& name, const std::string& signature, Args&&... args)
    {
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return;
        }
        jclass classId = getClass();
        if(!classId)
        {
            return;
        }
        jmethodID methodId = env->GetStaticMethodID(classId, name.c_str(), signature.c_str());
        if(!methodId || env->ExceptionCheck())
        {
            env->ExceptionClear();
            setError(std::string("Failed to find static method '") + name + "'.", Jni::kErrorJniFindMethod, Jni::kErrorTagJniObject);
            return;
        }
        else
        {
            jvalue* jargs = createArguments(args...);
            callStaticJavaMethod<void>(env, classId, methodId, jargs);

            std::vector<jobject*> jobjs;
            findJobjectArguments(jobjs, jargs, args...);

            for(jobject* jobj : jobjs)
            {
                env->DeleteLocalRef(*jobj);
            }

            if(env->ExceptionCheck())
            {
                env->ExceptionClear();
                setError(std::string("Failed to call static method '") + name + "'.", Jni::kErrorJniCallMethod, Jni::kErrorTagJniObject);
            }
        }
    }

    /**
     * Get a static class field
     * @param name the field name
     */
    template <typename Return>
    Return staticField(const std::string& name, const Return& defRet)
    {
        std::string signature(getSignaturePart<Return>(defRet));
        return staticFieldSigned(name, signature, defRet);
    }

    template <typename Return>
    Return staticFieldSigned(const std::string& name, const std::string& signature, const Return& defRet)
    {
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return defRet;
        }

        jclass classId = getClass();
        if(!classId)
        {
            return defRet;
        }

        jfieldID fieldId = env->GetStaticFieldID(classId, name.c_str(), signature.c_str());
        if(!fieldId || env->ExceptionCheck())
        {
            env->ExceptionClear();
            setError(std::string("Failed to find static field '") + name + "' with signature '" + signature + "'.", Jni::kErrorJniFindField,
                     Jni::kErrorTagJniObject);
            return defRet;
        }
        else
        {
            Return result = getJavaStaticField<Return>(env, classId, fieldId);
            if(env->ExceptionCheck())
            {
                env->ExceptionClear();
                setError(std::string("Failed to read static field '") + name + "' with signature '" + signature + "'.", Jni::kErrorJniCallField,
                         Jni::kErrorTagJniObject);
                return defRet;
            }
            else
            {
                return result;
            }
        }
    }

    /**
     * Get a object field
     * @param name the field name
     */
    template <typename Return>
    Return field(const std::string& name, const Return& defRet)
    {
        std::string signature(getSignaturePart<Return>(defRet));
        return fieldSigned(name, signature, defRet);
    }

    template <typename Return>
    Return fieldSigned(const std::string& name, const std::string& signature, const Return& defRet)
    {
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return defRet;
        }

        jclass classId = getClass();
        if(!classId)
        {
            return defRet;
        }

        jfieldID fieldId = env->GetFieldID(classId, name.c_str(), signature.c_str());
        if(!fieldId || env->ExceptionCheck())
        {
            env->ExceptionClear();
            setError(std::string("Failed to find field '") + name + "' with signature '" + signature + "'.", Jni::kErrorJniFindField,
                     Jni::kErrorTagJniObject);
            return defRet;
        }
        else
        {
            Return result = getJavaField<Return>(env, classId, fieldId);
            if(env->ExceptionCheck())
            {
                env->ExceptionClear();
                setError(std::string("Failed to read field '") + name + "' with signature '" + signature + "'.", Jni::kErrorJniCallField,
                         Jni::kErrorTagJniObject);
                return defRet;
            }
            else
            {
                return result;
            }
        }
    }

    /**
     * Return the signature for the object
     */
    std::string getSignature() const;

    /**
     * Return error string
     */
    const std::string& getError() const;

    /**
     * Return true of there is an error
     */
    bool hasError() const;

    /**
     * create an java array of the given type
     */
    template <typename Type>
    static jarray createJavaArray(JNIEnv* env, const Type& element, size_t size);

    /**
     * Convert a jobject array to a container
     */
    template <typename Type>
    static bool convertFromJavaArray(JNIEnv* env, jarray arr, Type& container)
    {
        if(!arr)
        {
            return false;
        }
        jsize arraySize = env->GetArrayLength(arr);
        for(size_t i = 0; i < arraySize; i++)
        {
            container.insert(container.end(), getJavaArrayElement<typename Type::value_type>(env, arr, i));
        }
        return true;
    }
    template <typename Type>
    static bool convertFromJavaArray(jarray arr, Type& container)
    {
        JNIEnv* env = getEnvironment();
        assert(env);
        return convertFromJavaArray<Type>(env, arr, container);
    }

    /**
     * Get an element of a java array
     */
    template <typename Type>
    static Type getJavaArrayElement(JNIEnv* env, jarray arr, size_t position);

    /**
    * Set an element of a java array
    */
    template <typename Type>
    static void setJavaArrayElement(JNIEnv* env, jarray arr, size_t position, const Type& elm);

    template <typename Type>
    static void objectCast(JNIEnv* env, jobject obj, Type& out);

    template <typename Type>
    static void objectCast(JNIEnv* env, jobject obj, std::vector<Type>& out)
    {
        convertFromJavaObject(env, obj, out);
    }

    template <typename Type>
    static bool convertFromJavaCollection(JNIEnv* env, jobject obj, Type& out)
    {
        if(!obj)
        {
            return false;
        }
        JniObject jcontainer(obj);
        if(!jcontainer.isInstanceOf("java/util/Collection"))
        {
            return false;
        }
        jobject jobjArray;
        jobjArray = jcontainer.callSigned<jobject>("toArray", "()[Ljava/lang/Object;", jobjArray);
        jarray jarr = (jarray)jobjArray;
        jsize arrSize = env->GetArrayLength(jarr);
        out.reserve(arrSize);
        for(size_t i = 0; i < arrSize; ++i)
        {
            jobject elem = getJavaArrayElement<jobject>(env, jarr, i);
            typename Type::value_type value;
            objectCast(env, elem, value);
            env->DeleteLocalRef(elem);
            out.insert(out.end(), value);
        }
        return true;
    }

    template <typename Key, typename Value>
    static bool convertFromJavaMap(JNIEnv* env, jobject obj, std::map<Key, Value>& out)
    {
        if(!obj)
        {
            return false;
        }
        JniObject jmap(obj);
        if(!jmap.isInstanceOf("java/util/Map"))
        {
            return false;
        }
        JniObject jkeys = jmap.call<JniObject>("keySet", JniObject("java/util/Set"));
        if(jkeys.hasError())
        {
            return false;
        }
        jobject jobjarrayKeys;
        jobjarrayKeys = jkeys.callSigned<jobject>("toArray", "()[Ljava/lang/Object;", jobjarrayKeys);
        jarray jarrayKeys = (jarray)jobjarrayKeys;
        jsize arrSize = env->GetArrayLength(jarrayKeys);

        jobject defaultResult = nullptr;
        std::vector<Key> keys;
        for(size_t i = 0; i < arrSize; ++i)
        {
            jobject keyElement = getJavaArrayElement<jobject>(env, jarrayKeys, i);
            jobject valueElement = jmap.callSigned<jobject>("get", "(Ljava/lang/Object;)Ljava/lang/Object;", defaultResult, keyElement);

            Key key;
            objectCast<Key>(env, keyElement, key);
            Value value;
            objectCast<Value>(env, valueElement, value);

            env->DeleteLocalRef(keyElement);
            env->DeleteLocalRef(valueElement);
            out[key] = value;
        }
        return true;
    }

    template <typename Key, typename Value>
    static bool convertToMapFromJavaArray(JNIEnv* env, jarray arr, std::map<Key, Value>& out)
    {
        if(!arr)
        {
            return false;
        }
        jsize mapSize = env->GetArrayLength(arr) / 2;
        for(size_t i = 0; i < mapSize; ++i)
        {
            Key k = getJavaArrayElement<Key>(env, arr, i * 2);
            Value v = getJavaArrayElement<Value>(env, arr, i * 2 + 1);
            out[k] = v;
        }
        return true;
    }

    /**
     * Convert a jobject to the c++ representation
     */
    template <typename Type>
    static bool convertFromJavaObject(JNIEnv* env, jobject obj, Type& out);

    // template specialization for containers
    template <typename Type>
    static bool convertFromJavaObject(JNIEnv* env, jobject obj, std::vector<Type>& out)
    {
        if(convertFromJavaCollection(env, obj, out))
        {
            return true;
        }
        if(convertFromJavaArray(env, (jarray)obj, out))
        {
            return true;
        }
        return false;
    }
    template <typename Type>
    static bool convertFromJavaObject(JNIEnv* env, jobject obj, std::set<Type>& out)
    {
        if(convertFromJavaCollection(env, obj, out))
        {
            return true;
        }
        if(convertFromJavaArray(env, (jarray)obj, out))
        {
            return true;
        }
        return false;
    }
    template <typename Type, int Size>
    static bool convertFromJavaObject(JNIEnv* env, jobject obj, std::array<Type, Size>& out)
    {
        if(convertFromJavaCollection(env, obj, out))
        {
            return true;
        }
        if(convertFromJavaArray(env, (jarray)obj, out))
        {
            return true;
        }
        return false;
    }
    template <typename Type>
    static bool convertFromJavaObject(JNIEnv* env, jobject obj, std::list<Type>& out)
    {
        if(convertFromJavaCollection(env, obj, out))
        {
            return true;
        }
        if(convertFromJavaArray(env, (jarray)obj, out))
        {
            return true;
        }
        return false;
    }

    template <typename Key, typename Type>
    static bool convertFromJavaObject(JNIEnv* env, jobject obj, std::map<Key, Type>& out)
    {
        if(convertFromJavaMap(env, obj, out))
        {
            return true;
        }
        if(convertToMapFromJavaArray(env, (jarray)obj, out))
        {
            return true;
        }
        return false;
    }


    // utility methods that return the object
    template <typename Type>
    static Type convertFromJavaObject(JNIEnv* env, jobject obj)
    {
        Type out;
        bool result = convertFromJavaObject(env, obj, out);
        assert(result);
        (void)result;
        return out;
    }
    template <typename Type>
    static Type convertFromJavaObject(jobject obj)
    {
        JNIEnv* env = getEnvironment();
        assert(env);
        return convertFromJavaObject<Type>(env, obj);
    }
    template <typename Type>
    static bool convertFromJavaObject(jobject obj, Type& out)
    {
        JNIEnv* env = getEnvironment();
        assert(env);
        return convertFromJavaObject(env, obj, out);
    }

    template <typename Type>
    static std::vector<Type> convertFromJavaVectorOfObjects(JNIEnv* env, jobject& obj)
    {
        std::vector<Type> out;
        bool result = convertFromJavaObject(env, obj, out);
        assert(result);
        return out;
    }
    template <typename Key, typename Value>
    static std::map<Key, Value> convertFromJavaMapOfObjects(JNIEnv* env, jobject& obj)
    {
        std::map<Key, Value> out;
        bool result = convertFromJavaObject(env, obj, out);
        assert(result);
        return out;
    }

    /**
     * Convert a c++ list container to a jarray
     */
    template <typename Type>
    static jarray createJavaArray(const Type& obj)
    {
        JNIEnv* env = getEnvironment();
        if(!env)
        {
            return nullptr;
        }
        jarray arr = nullptr;
        if(obj.empty())
        {
            arr = createJavaArray(env, typename Type::value_type(), 0);
        }
        else
        {
            arr = createJavaArray(env, *obj.begin(), obj.size());
        }
        size_t i = 0;
        for(typename Type::const_iterator itr = obj.begin(); itr != obj.end(); ++itr)
        {
            setJavaArrayElement(env, arr, i, *itr);
            i++;
        }
        return arr;
    }

    template <typename Key, typename Value>
    static JniObject createJavaMap(const std::map<Key, Value>& obj, const std::string& classPath = "java/util/HashMap")
    {
        JniObject jmap(JniObject::createNew(classPath));
        for(typename std::map<Key, Value>::const_iterator itr = obj.begin(); itr != obj.end(); ++itr)
        {
            jmap.callSigned("put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;", itr->second, itr->first, itr->second);
        }
        return jmap;
    }

    template <typename Type>
    static JniObject createJavaList(const Type& obj, const std::string& classPath = "java/util/ArrayList")
    {
        JniObject jlist(JniObject::createNew(classPath));
        for(typename Type::const_iterator itr = obj.begin(); itr != obj.end(); ++itr)
        {
            jlist.callSignedVoid("add", "(Ljava/lang/Object;)Z", *itr);
        }
        return jlist;
    }

    template <typename Type>
    static JniObject createJavaSet(const Type& obj, const std::string& classPath = "java/util/HashSet")
    {
        return createJavaList(obj, classPath);
    }

    /**
     * Convert a c++ type to the jvalue representation
     * This is called on all jni arguments
     */
    template <typename Type>
    static jvalue convertToJavaValue(const Type& obj);

    template <typename Type>
    static bool isJobjectArgument(const Type& obj);

    // template specialization for pointers
    template <typename Type>
    static jvalue convertToJavaValue(Type* obj)
    {
        return convertToJavaValue((jlong)obj);
    }
    template <typename Type>
    static bool isJobjectArgument(Type* obj)
    {
        return false;
    }

    // template specialization for containers
    template <typename Type>
    static jvalue convertToJavaValue(const std::vector<Type>& obj)
    {
        return convertToJavaValue(createJavaArray(obj));
    }
    template <typename Type>
    static bool isJobjectArgument(const std::vector<Type>& obj)
    {
        return true;
    }

    template <typename Type>
    static jvalue convertToJavaValue(const std::set<Type>& obj)
    {
        return convertToJavaValue(createJavaArray(obj));
    }
    template <typename Type>
    static bool isJobjectArgument(const std::set<Type>& obj)
    {
        return true;
    }

    template <typename Type, int Size>
    static jvalue convertToJavaValue(const std::array<Type, Size>& obj)
    {
        return convertToJavaValue(createJavaArray(obj));
    }
    template <typename Type, int Size>
    static bool isJobjectArgument(const std::array<Type, Size>& obj)
    {
        return true;
    }

    template <typename Type>
    static jvalue convertToJavaValue(const std::list<Type>& obj)
    {
        return convertToJavaValue(createJavaArray(obj));
    }
    template <typename Type>
    static bool isJobjectArgument(const std::list<Type>& obj)
    {
        return true;
    }

    template <typename Key, typename Value>
    static jvalue convertToJavaValue(const std::map<Key, Value>& obj)
    {
        return convertToJavaValue(createJavaMap(obj).getNewLocalInstance());
    }
    template <typename Key, typename Value>
    static bool isJobjectArgument(const std::map<Key, Value>& obj)
    {
        return true;
    }

    /**
     * Returns the class reference. This is a global ref that will be removed
     * when the JniObject is destroyed
     */
    jclass getClass() const;

    /**
     * Returns the class path. If it is not there it tries to call
     * `getClass().getName()` on the object to get the class
     */
    const std::string& getClassPath() const;

    /**
     * Returns the jobject reference. This is a global ref that will be removed
     * when the JniObject is destroyed
     */
    jobject getInstance() const;

    /**
    * Returns the jobject reference. This is a new local ref
    */
    jobject getNewLocalInstance() const;

    /**
     * Return true if class path and class ref match
     */
    bool isInstanceOf(const std::string& classPath) const;

    /**
     * Returns the environment pointer
     */
    static JNIEnv* getEnvironment();

    /**
     * Returns true if there is an object instance
     */
    operator bool() const;

    /**
     * Copy a jni object
     */
    JniObject& operator=(const JniObject& other);

    /**
     * Compare two jni objects
     */
    bool operator==(const JniObject& other) const;
};


#endif
