/* include/curl/curlbuild.h.  Generated from curlbuild.h.in by configure.  */
#ifndef __CURL_CURLBUILD_H
#define __CURL_CURLBUILD_H

/***************************************************************************
*                                  _   _ ____  _
*  Project                     ___| | | |  _ \| |
*                             / __| | | | |_) | |
*                            | (__| |_| |  _ <| |___
*                             \___|\___/|_| \_\_____|
*
* Copyright (C) 1998 - 2012, Daniel Stenberg, <daniel@haxx.se>, et al.
*
* This software is licensed as described in the file COPYING, which
* you should have received as part of this distribution. The terms
* are also available at http://curl.haxx.se/docs/copyright.html.
*
* You may opt to use, copy, modify, merge, publish, distribute and/or sell
* copies of the Software, and permit persons to whom the Software is
* furnished to do so, under the terms of the COPYING file.
*
* This software is distributed on an "AS IS" basis, WITHOUT WARRANTY OF ANY
* KIND, either express or implied.
*
***************************************************************************/

/* ================================================================ */
/*               NOTES FOR CONFIGURE CAPABLE SYSTEMS                */
/* ================================================================ */

/*
* NOTE 1:
* -------
*
* Nothing in this file is intended to be modified or adjusted by the
* curl library user nor by the curl library builder.
*
* If you think that something actually needs to be changed, adjusted
* or fixed in this file, then, report it on the libcurl development
* mailing list: http://cool.haxx.se/mailman/listinfo/curl-library/
*
* This header file shall only export symbols which are 'curl' or 'CURL'
* prefixed, otherwise public name space would be polluted.
*
* NOTE 2:
* -------
*
* Right now you might be staring at file include/curl/curlbuild.h.in or
* at file include/curl/curlbuild.h, this is due to the following reason:
*
* On systems capable of running the configure script, the configure process
* will overwrite the distributed include/curl/curlbuild.h file with one that
* is suitable and specific to the library being configured and built, which
* is generated from the include/curl/curlbuild.h.in template file.
*
*/

/* ================================================================ */
/*  DEFINITION OF THESE SYMBOLS SHALL NOT TAKE PLACE ANYWHERE ELSE  */
/* ================================================================ */



#ifdef _WIN32

#ifdef CURL_SIZEOF_LONG
#  error "CURL_SIZEOF_LONG shall not be defined except in curlbuild.h"
Error Compilation_aborted_CURL_SIZEOF_LONG_already_defined
#endif

#ifdef CURL_TYPEOF_CURL_SOCKLEN_T
#  error "CURL_TYPEOF_CURL_SOCKLEN_T shall not be defined except in curlbuild.h"
Error Compilation_aborted_CURL_TYPEOF_CURL_SOCKLEN_T_already_defined
#endif

#ifdef CURL_SIZEOF_CURL_SOCKLEN_T
#  error "CURL_SIZEOF_CURL_SOCKLEN_T shall not be defined except in curlbuild.h"
Error Compilation_aborted_CURL_SIZEOF_CURL_SOCKLEN_T_already_defined
#endif

#ifdef CURL_TYPEOF_CURL_OFF_T
#  error "CURL_TYPEOF_CURL_OFF_T shall not be defined except in curlbuild.h"
Error Compilation_aborted_CURL_TYPEOF_CURL_OFF_T_already_defined
#endif

#ifdef CURL_FORMAT_CURL_OFF_T
#  error "CURL_FORMAT_CURL_OFF_T shall not be defined except in curlbuild.h"
Error Compilation_aborted_CURL_FORMAT_CURL_OFF_T_already_defined
#endif

#ifdef CURL_FORMAT_CURL_OFF_TU
#  error "CURL_FORMAT_CURL_OFF_TU shall not be defined except in curlbuild.h"
Error Compilation_aborted_CURL_FORMAT_CURL_OFF_TU_already_defined
#endif

#ifdef CURL_FORMAT_OFF_T
#  error "CURL_FORMAT_OFF_T shall not be defined except in curlbuild.h"
Error Compilation_aborted_CURL_FORMAT_OFF_T_already_defined
#endif

#ifdef CURL_SIZEOF_CURL_OFF_T
#  error "CURL_SIZEOF_CURL_OFF_T shall not be defined except in curlbuild.h"
Error Compilation_aborted_CURL_SIZEOF_CURL_OFF_T_already_defined
#endif

#ifdef CURL_SUFFIX_CURL_OFF_T
#  error "CURL_SUFFIX_CURL_OFF_T shall not be defined except in curlbuild.h"
Error Compilation_aborted_CURL_SUFFIX_CURL_OFF_T_already_defined
#endif

#ifdef CURL_SUFFIX_CURL_OFF_TU
#  error "CURL_SUFFIX_CURL_OFF_TU shall not be defined except in curlbuild.h"
Error Compilation_aborted_CURL_SUFFIX_CURL_OFF_TU_already_defined
#endif

/* ================================================================ */
/*    EXTERNAL INTERFACE SETTINGS FOR NON-CONFIGURE SYSTEMS ONLY    */
/* ================================================================ */

#if defined(__DJGPP__) || defined(__GO32__)
#  if defined(__DJGPP__) && (__DJGPP__ > 1)
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long long
#    define CURL_FORMAT_CURL_OFF_T     "lld"
#    define CURL_FORMAT_CURL_OFF_TU    "llu"
#    define CURL_FORMAT_OFF_T          "%lld"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     LL
#    define CURL_SUFFIX_CURL_OFF_TU    ULL
#  else
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long
#    define CURL_FORMAT_CURL_OFF_T     "ld"
#    define CURL_FORMAT_CURL_OFF_TU    "lu"
#    define CURL_FORMAT_OFF_T          "%ld"
#    define CURL_SIZEOF_CURL_OFF_T     4
#    define CURL_SUFFIX_CURL_OFF_T     L
#    define CURL_SUFFIX_CURL_OFF_TU    UL
#  endif
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(__SALFORDC__)
#  define CURL_SIZEOF_LONG           4
#  define CURL_TYPEOF_CURL_OFF_T     long
#  define CURL_FORMAT_CURL_OFF_T     "ld"
#  define CURL_FORMAT_CURL_OFF_TU    "lu"
#  define CURL_FORMAT_OFF_T          "%ld"
#  define CURL_SIZEOF_CURL_OFF_T     4
#  define CURL_SUFFIX_CURL_OFF_T     L
#  define CURL_SUFFIX_CURL_OFF_TU    UL
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(__BORLANDC__)
#  if (__BORLANDC__ < 0x520)
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long
#    define CURL_FORMAT_CURL_OFF_T     "ld"
#    define CURL_FORMAT_CURL_OFF_TU    "lu"
#    define CURL_FORMAT_OFF_T          "%ld"
#    define CURL_SIZEOF_CURL_OFF_T     4
#    define CURL_SUFFIX_CURL_OFF_T     L
#    define CURL_SUFFIX_CURL_OFF_TU    UL
#  else
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     __int64
#    define CURL_FORMAT_CURL_OFF_T     "I64d"
#    define CURL_FORMAT_CURL_OFF_TU    "I64u"
#    define CURL_FORMAT_OFF_T          "%I64d"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     i64
#    define CURL_SUFFIX_CURL_OFF_TU    ui64
#  endif
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(__TURBOC__)
#  define CURL_SIZEOF_LONG           4
#  define CURL_TYPEOF_CURL_OFF_T     long
#  define CURL_FORMAT_CURL_OFF_T     "ld"
#  define CURL_FORMAT_CURL_OFF_TU    "lu"
#  define CURL_FORMAT_OFF_T          "%ld"
#  define CURL_SIZEOF_CURL_OFF_T     4
#  define CURL_SUFFIX_CURL_OFF_T     L
#  define CURL_SUFFIX_CURL_OFF_TU    UL
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(__WATCOMC__)
#  if defined(__386__)
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     __int64
#    define CURL_FORMAT_CURL_OFF_T     "I64d"
#    define CURL_FORMAT_CURL_OFF_TU    "I64u"
#    define CURL_FORMAT_OFF_T          "%I64d"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     i64
#    define CURL_SUFFIX_CURL_OFF_TU    ui64
#  else
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long
#    define CURL_FORMAT_CURL_OFF_T     "ld"
#    define CURL_FORMAT_CURL_OFF_TU    "lu"
#    define CURL_FORMAT_OFF_T          "%ld"
#    define CURL_SIZEOF_CURL_OFF_T     4
#    define CURL_SUFFIX_CURL_OFF_T     L
#    define CURL_SUFFIX_CURL_OFF_TU    UL
#  endif
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(__POCC__)
#  if (__POCC__ < 280)
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long
#    define CURL_FORMAT_CURL_OFF_T     "ld"
#    define CURL_FORMAT_CURL_OFF_TU    "lu"
#    define CURL_FORMAT_OFF_T          "%ld"
#    define CURL_SIZEOF_CURL_OFF_T     4
#    define CURL_SUFFIX_CURL_OFF_T     L
#    define CURL_SUFFIX_CURL_OFF_TU    UL
#  elif defined(_MSC_VER)
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     __int64
#    define CURL_FORMAT_CURL_OFF_T     "I64d"
#    define CURL_FORMAT_CURL_OFF_TU    "I64u"
#    define CURL_FORMAT_OFF_T          "%I64d"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     i64
#    define CURL_SUFFIX_CURL_OFF_TU    ui64
#  else
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long long
#    define CURL_FORMAT_CURL_OFF_T     "lld"
#    define CURL_FORMAT_CURL_OFF_TU    "llu"
#    define CURL_FORMAT_OFF_T          "%lld"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     LL
#    define CURL_SUFFIX_CURL_OFF_TU    ULL
#  endif
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(__LCC__)
#  define CURL_SIZEOF_LONG           4
#  define CURL_TYPEOF_CURL_OFF_T     long
#  define CURL_FORMAT_CURL_OFF_T     "ld"
#  define CURL_FORMAT_CURL_OFF_TU    "lu"
#  define CURL_FORMAT_OFF_T          "%ld"
#  define CURL_SIZEOF_CURL_OFF_T     4
#  define CURL_SUFFIX_CURL_OFF_T     L
#  define CURL_SUFFIX_CURL_OFF_TU    UL
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(__SYMBIAN32__)
#  if defined(__EABI__)  /* Treat all ARM compilers equally */
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long long
#    define CURL_FORMAT_CURL_OFF_T     "lld"
#    define CURL_FORMAT_CURL_OFF_TU    "llu"
#    define CURL_FORMAT_OFF_T          "%lld"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     LL
#    define CURL_SUFFIX_CURL_OFF_TU    ULL
#  elif defined(__CW32__)
#    pragma longlong on
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long long
#    define CURL_FORMAT_CURL_OFF_T     "lld"
#    define CURL_FORMAT_CURL_OFF_TU    "llu"
#    define CURL_FORMAT_OFF_T          "%lld"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     LL
#    define CURL_SUFFIX_CURL_OFF_TU    ULL
#  elif defined(__VC32__)
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     __int64
#    define CURL_FORMAT_CURL_OFF_T     "lld"
#    define CURL_FORMAT_CURL_OFF_TU    "llu"
#    define CURL_FORMAT_OFF_T          "%lld"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     LL
#    define CURL_SUFFIX_CURL_OFF_TU    ULL
#  endif
#  define CURL_TYPEOF_CURL_SOCKLEN_T unsigned int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(__MWERKS__)
#  define CURL_SIZEOF_LONG           4
#  define CURL_TYPEOF_CURL_OFF_T     long long
#  define CURL_FORMAT_CURL_OFF_T     "lld"
#  define CURL_FORMAT_CURL_OFF_TU    "llu"
#  define CURL_FORMAT_OFF_T          "%lld"
#  define CURL_SIZEOF_CURL_OFF_T     8
#  define CURL_SUFFIX_CURL_OFF_T     LL
#  define CURL_SUFFIX_CURL_OFF_TU    ULL
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(_WIN32_WCE)
#  define CURL_SIZEOF_LONG           4
#  define CURL_TYPEOF_CURL_OFF_T     __int64
#  define CURL_FORMAT_CURL_OFF_T     "I64d"
#  define CURL_FORMAT_CURL_OFF_TU    "I64u"
#  define CURL_FORMAT_OFF_T          "%I64d"
#  define CURL_SIZEOF_CURL_OFF_T     8
#  define CURL_SUFFIX_CURL_OFF_T     i64
#  define CURL_SUFFIX_CURL_OFF_TU    ui64
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(__MINGW32__)
#  define CURL_SIZEOF_LONG           4
#  define CURL_TYPEOF_CURL_OFF_T     long long
#  define CURL_FORMAT_CURL_OFF_T     "I64d"
#  define CURL_FORMAT_CURL_OFF_TU    "I64u"
#  define CURL_FORMAT_OFF_T          "%I64d"
#  define CURL_SIZEOF_CURL_OFF_T     8
#  define CURL_SUFFIX_CURL_OFF_T     LL
#  define CURL_SUFFIX_CURL_OFF_TU    ULL
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(__VMS)
#  if defined(__VAX)
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long
#    define CURL_FORMAT_CURL_OFF_T     "ld"
#    define CURL_FORMAT_CURL_OFF_TU    "lu"
#    define CURL_FORMAT_OFF_T          "%ld"
#    define CURL_SIZEOF_CURL_OFF_T     4
#    define CURL_SUFFIX_CURL_OFF_T     L
#    define CURL_SUFFIX_CURL_OFF_TU    UL
#  else
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long long
#    define CURL_FORMAT_CURL_OFF_T     "lld"
#    define CURL_FORMAT_CURL_OFF_TU    "llu"
#    define CURL_FORMAT_OFF_T          "%lld"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     LL
#    define CURL_SUFFIX_CURL_OFF_TU    ULL
#  endif
#  define CURL_TYPEOF_CURL_SOCKLEN_T unsigned int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

#elif defined(__OS400__)
#  if defined(__ILEC400__)
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long long
#    define CURL_FORMAT_CURL_OFF_T     "lld"
#    define CURL_FORMAT_CURL_OFF_TU    "llu"
#    define CURL_FORMAT_OFF_T          "%lld"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     LL
#    define CURL_SUFFIX_CURL_OFF_TU    ULL
#    define CURL_TYPEOF_CURL_SOCKLEN_T socklen_t
#    define CURL_SIZEOF_CURL_SOCKLEN_T 4
#    define CURL_PULL_SYS_TYPES_H      1
#    define CURL_PULL_SYS_SOCKET_H     1
#  endif

#elif defined(__MVS__)
#  if defined(__IBMC__) || defined(__IBMCPP__)
#    if defined(_ILP32)
#      define CURL_SIZEOF_LONG           4
#    elif defined(_LP64)
#      define CURL_SIZEOF_LONG           8
#    endif
#    if defined(_LONG_LONG)
#      define CURL_TYPEOF_CURL_OFF_T     long long
#      define CURL_FORMAT_CURL_OFF_T     "lld"
#      define CURL_FORMAT_CURL_OFF_TU    "llu"
#      define CURL_FORMAT_OFF_T          "%lld"
#      define CURL_SIZEOF_CURL_OFF_T     8
#      define CURL_SUFFIX_CURL_OFF_T     LL
#      define CURL_SUFFIX_CURL_OFF_TU    ULL
#    elif defined(_LP64)
#      define CURL_TYPEOF_CURL_OFF_T     long
#      define CURL_FORMAT_CURL_OFF_T     "ld"
#      define CURL_FORMAT_CURL_OFF_TU    "lu"
#      define CURL_FORMAT_OFF_T          "%ld"
#      define CURL_SIZEOF_CURL_OFF_T     8
#      define CURL_SUFFIX_CURL_OFF_T     L
#      define CURL_SUFFIX_CURL_OFF_TU    UL
#    else
#      define CURL_TYPEOF_CURL_OFF_T     long
#      define CURL_FORMAT_CURL_OFF_T     "ld"
#      define CURL_FORMAT_CURL_OFF_TU    "lu"
#      define CURL_FORMAT_OFF_T          "%ld"
#      define CURL_SIZEOF_CURL_OFF_T     4
#      define CURL_SUFFIX_CURL_OFF_T     L
#      define CURL_SUFFIX_CURL_OFF_TU    UL
#    endif
#    define CURL_TYPEOF_CURL_SOCKLEN_T socklen_t
#    define CURL_SIZEOF_CURL_SOCKLEN_T 4
#    define CURL_PULL_SYS_TYPES_H      1
#    define CURL_PULL_SYS_SOCKET_H     1
#  endif

#elif defined(__370__)
#  if defined(__IBMC__) || defined(__IBMCPP__)
#    if defined(_ILP32)
#      define CURL_SIZEOF_LONG           4
#    elif defined(_LP64)
#      define CURL_SIZEOF_LONG           8
#    endif
#    if defined(_LONG_LONG)
#      define CURL_TYPEOF_CURL_OFF_T     long long
#      define CURL_FORMAT_CURL_OFF_T     "lld"
#      define CURL_FORMAT_CURL_OFF_TU    "llu"
#      define CURL_FORMAT_OFF_T          "%lld"
#      define CURL_SIZEOF_CURL_OFF_T     8
#      define CURL_SUFFIX_CURL_OFF_T     LL
#      define CURL_SUFFIX_CURL_OFF_TU    ULL
#    elif defined(_LP64)
#      define CURL_TYPEOF_CURL_OFF_T     long
#      define CURL_FORMAT_CURL_OFF_T     "ld"
#      define CURL_FORMAT_CURL_OFF_TU    "lu"
#      define CURL_FORMAT_OFF_T          "%ld"
#      define CURL_SIZEOF_CURL_OFF_T     8
#      define CURL_SUFFIX_CURL_OFF_T     L
#      define CURL_SUFFIX_CURL_OFF_TU    UL
#    else
#      define CURL_TYPEOF_CURL_OFF_T     long
#      define CURL_FORMAT_CURL_OFF_T     "ld"
#      define CURL_FORMAT_CURL_OFF_TU    "lu"
#      define CURL_FORMAT_OFF_T          "%ld"
#      define CURL_SIZEOF_CURL_OFF_T     4
#      define CURL_SUFFIX_CURL_OFF_T     L
#      define CURL_SUFFIX_CURL_OFF_TU    UL
#    endif
#    define CURL_TYPEOF_CURL_SOCKLEN_T socklen_t
#    define CURL_SIZEOF_CURL_SOCKLEN_T 4
#    define CURL_PULL_SYS_TYPES_H      1
#    define CURL_PULL_SYS_SOCKET_H     1
#  endif

#elif defined(TPF)
#  define CURL_SIZEOF_LONG           8
#  define CURL_TYPEOF_CURL_OFF_T     long
#  define CURL_FORMAT_CURL_OFF_T     "ld"
#  define CURL_FORMAT_CURL_OFF_TU    "lu"
#  define CURL_FORMAT_OFF_T          "%ld"
#  define CURL_SIZEOF_CURL_OFF_T     8
#  define CURL_SUFFIX_CURL_OFF_T     L
#  define CURL_SUFFIX_CURL_OFF_TU    UL
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

/* ===================================== */
/*    KEEP MSVC THE PENULTIMATE ENTRY    */
/* ===================================== */

#elif defined(_MSC_VER)
#  if (_MSC_VER >= 900) && (_INTEGRAL_MAX_BITS >= 64)
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     __int64
#    define CURL_FORMAT_CURL_OFF_T     "I64d"
#    define CURL_FORMAT_CURL_OFF_TU    "I64u"
#    define CURL_FORMAT_OFF_T          "%I64d"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     i64
#    define CURL_SUFFIX_CURL_OFF_TU    ui64
#  else
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long
#    define CURL_FORMAT_CURL_OFF_T     "ld"
#    define CURL_FORMAT_CURL_OFF_TU    "lu"
#    define CURL_FORMAT_OFF_T          "%ld"
#    define CURL_SIZEOF_CURL_OFF_T     4
#    define CURL_SUFFIX_CURL_OFF_T     L
#    define CURL_SUFFIX_CURL_OFF_TU    UL
#  endif
#  define CURL_TYPEOF_CURL_SOCKLEN_T int
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4

/* ===================================== */
/*    KEEP GENERIC GCC THE LAST ENTRY    */
/* ===================================== */

#elif defined(__GNUC__)
#  if defined(__ILP32__) || \
      defined(__i386__) || defined(__ppc__) || defined(__arm__) || defined(__sparc__)
#    define CURL_SIZEOF_LONG           4
#    define CURL_TYPEOF_CURL_OFF_T     long long
#    define CURL_FORMAT_CURL_OFF_T     "lld"
#    define CURL_FORMAT_CURL_OFF_TU    "llu"
#    define CURL_FORMAT_OFF_T          "%lld"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     LL
#    define CURL_SUFFIX_CURL_OFF_TU    ULL
#  elif defined(__LP64__) || \
        defined(__x86_64__) || defined(__ppc64__) || defined(__sparc64__)
#    define CURL_SIZEOF_LONG           8
#    define CURL_TYPEOF_CURL_OFF_T     long
#    define CURL_FORMAT_CURL_OFF_T     "ld"
#    define CURL_FORMAT_CURL_OFF_TU    "lu"
#    define CURL_FORMAT_OFF_T          "%ld"
#    define CURL_SIZEOF_CURL_OFF_T     8
#    define CURL_SUFFIX_CURL_OFF_T     L
#    define CURL_SUFFIX_CURL_OFF_TU    UL
#  endif
#  define CURL_TYPEOF_CURL_SOCKLEN_T socklen_t
#  define CURL_SIZEOF_CURL_SOCKLEN_T 4
#  define CURL_PULL_SYS_TYPES_H      1
#  define CURL_PULL_SYS_SOCKET_H     1

#else
#  error "Unknown non-configure build target!"
	Error Compilation_aborted_Unknown_non_configure_build_target
#endif

	/* CURL_PULL_SYS_TYPES_H is defined above when inclusion of header file  */
	/* sys/types.h is required here to properly make type definitions below. */
#ifdef CURL_PULL_SYS_TYPES_H
#  include <sys/types.h>
#endif

	/* CURL_PULL_SYS_SOCKET_H is defined above when inclusion of header file  */
	/* sys/socket.h is required here to properly make type definitions below. */
#ifdef CURL_PULL_SYS_SOCKET_H
#  include <sys/socket.h>
#endif

	/* Data type definition of curl_socklen_t. */

#ifdef CURL_TYPEOF_CURL_SOCKLEN_T
	typedef CURL_TYPEOF_CURL_SOCKLEN_T curl_socklen_t;
#endif

/* Data type definition of curl_off_t. */

#ifdef CURL_TYPEOF_CURL_OFF_T
typedef CURL_TYPEOF_CURL_OFF_T curl_off_t;
#endif


#else


#ifdef CURL_SIZEOF_LONG
#error "CURL_SIZEOF_LONG shall not be defined except in curlbuild.h"
   Error Compilation_aborted_CURL_SIZEOF_LONG_already_defined
#endif

#ifdef CURL_TYPEOF_CURL_SOCKLEN_T
#error "CURL_TYPEOF_CURL_SOCKLEN_T shall not be defined except in curlbuild.h"
   Error Compilation_aborted_CURL_TYPEOF_CURL_SOCKLEN_T_already_defined
#endif

#ifdef CURL_SIZEOF_CURL_SOCKLEN_T
#error "CURL_SIZEOF_CURL_SOCKLEN_T shall not be defined except in curlbuild.h"
   Error Compilation_aborted_CURL_SIZEOF_CURL_SOCKLEN_T_already_defined
#endif

#ifdef CURL_TYPEOF_CURL_OFF_T
#error "CURL_TYPEOF_CURL_OFF_T shall not be defined except in curlbuild.h"
   Error Compilation_aborted_CURL_TYPEOF_CURL_OFF_T_already_defined
#endif

#ifdef CURL_FORMAT_CURL_OFF_T
#error "CURL_FORMAT_CURL_OFF_T shall not be defined except in curlbuild.h"
   Error Compilation_aborted_CURL_FORMAT_CURL_OFF_T_already_defined
#endif

#ifdef CURL_FORMAT_CURL_OFF_TU
#error "CURL_FORMAT_CURL_OFF_TU shall not be defined except in curlbuild.h"
   Error Compilation_aborted_CURL_FORMAT_CURL_OFF_TU_already_defined
#endif

#ifdef CURL_FORMAT_OFF_T
#error "CURL_FORMAT_OFF_T shall not be defined except in curlbuild.h"
   Error Compilation_aborted_CURL_FORMAT_OFF_T_already_defined
#endif

#ifdef CURL_SIZEOF_CURL_OFF_T
#error "CURL_SIZEOF_CURL_OFF_T shall not be defined except in curlbuild.h"
   Error Compilation_aborted_CURL_SIZEOF_CURL_OFF_T_already_defined
#endif

#ifdef CURL_SUFFIX_CURL_OFF_T
#error "CURL_SUFFIX_CURL_OFF_T shall not be defined except in curlbuild.h"
   Error Compilation_aborted_CURL_SUFFIX_CURL_OFF_T_already_defined
#endif

#ifdef CURL_SUFFIX_CURL_OFF_TU
#error "CURL_SUFFIX_CURL_OFF_TU shall not be defined except in curlbuild.h"
   Error Compilation_aborted_CURL_SUFFIX_CURL_OFF_TU_already_defined
#endif

/* ================================================================ */
/*  EXTERNAL INTERFACE SETTINGS FOR CONFIGURE CAPABLE SYSTEMS ONLY  */
/* ================================================================ */

/* Configure process defines this to 1 when it finds out that system  */
/* header file ws2tcpip.h must be included by the external interface. */
/* #undef CURL_PULL_WS2TCPIP_H */
#ifdef CURL_PULL_WS2TCPIP_H
#  ifndef WIN32_LEAN_AND_MEAN
#    define WIN32_LEAN_AND_MEAN
#  endif
#  include <windows.h>
#  include <winsock2.h>
#  include <ws2tcpip.h>
#endif

/* Configure process defines this to 1 when it finds out that system   */
/* header file sys/types.h must be included by the external interface. */
#define CURL_PULL_SYS_TYPES_H 1
#ifdef CURL_PULL_SYS_TYPES_H
#  include <sys/types.h>
#endif

/* Configure process defines this to 1 when it finds out that system */
/* header file stdint.h must be included by the external interface.  */
/* #undef CURL_PULL_STDINT_H */
#ifdef CURL_PULL_STDINT_H
#  include <stdint.h>
#endif

/* Configure process defines this to 1 when it finds out that system  */
/* header file inttypes.h must be included by the external interface. */
/* #undef CURL_PULL_INTTYPES_H */
#ifdef CURL_PULL_INTTYPES_H
#  include <inttypes.h>
#endif

/* Configure process defines this to 1 when it finds out that system    */
/* header file sys/socket.h must be included by the external interface. */
#define CURL_PULL_SYS_SOCKET_H 1
#ifdef CURL_PULL_SYS_SOCKET_H
#  include <sys/socket.h>
#endif

/* Configure process defines this to 1 when it finds out that system  */
/* header file sys/poll.h must be included by the external interface. */
/* #undef CURL_PULL_SYS_POLL_H */
#ifdef CURL_PULL_SYS_POLL_H
#  include <sys/poll.h>
#endif

#ifdef __LP64__

/* The size of `long', as computed by sizeof. */
#define CURL_SIZEOF_LONG 8

/* Integral data type used for curl_socklen_t. */
#define CURL_TYPEOF_CURL_SOCKLEN_T socklen_t

/* The size of `curl_socklen_t', as computed by sizeof. */
#define CURL_SIZEOF_CURL_SOCKLEN_T 4

/* Data type definition of curl_socklen_t. */
typedef CURL_TYPEOF_CURL_SOCKLEN_T curl_socklen_t;

/* Signed integral data type used for curl_off_t. */
#define CURL_TYPEOF_CURL_OFF_T long

/* Data type definition of curl_off_t. */
typedef CURL_TYPEOF_CURL_OFF_T curl_off_t;

/* curl_off_t formatting string directive without "%" conversion specifier. */
#define CURL_FORMAT_CURL_OFF_T "ld"

/* unsigned curl_off_t formatting string without "%" conversion specifier. */
#define CURL_FORMAT_CURL_OFF_TU "lu"

/* curl_off_t formatting string directive with "%" conversion specifier. */
#define CURL_FORMAT_OFF_T "%ld"

/* The size of `curl_off_t', as computed by sizeof. */
#define CURL_SIZEOF_CURL_OFF_T 8

/* curl_off_t constant suffix. */
#define CURL_SUFFIX_CURL_OFF_T L

/* unsigned curl_off_t constant suffix. */
#define CURL_SUFFIX_CURL_OFF_TU UL

#else

/* The size of `long', as computed by sizeof. */
#define CURL_SIZEOF_LONG 4

/* Integral data type used for curl_socklen_t. */
#define CURL_TYPEOF_CURL_SOCKLEN_T socklen_t

/* The size of `curl_socklen_t', as computed by sizeof. */
#define CURL_SIZEOF_CURL_SOCKLEN_T 4

/* Data type definition of curl_socklen_t. */
typedef CURL_TYPEOF_CURL_SOCKLEN_T curl_socklen_t;

/* Signed integral data type used for curl_off_t. */
#define CURL_TYPEOF_CURL_OFF_T long long

/* Data type definition of curl_off_t. */
typedef CURL_TYPEOF_CURL_OFF_T curl_off_t;

/* curl_off_t formatting string directive without "%" conversion specifier. */
#define CURL_FORMAT_CURL_OFF_T "lld"

/* unsigned curl_off_t formatting string without "%" conversion specifier. */
#define CURL_FORMAT_CURL_OFF_TU "llu"

/* curl_off_t formatting string directive with "%" conversion specifier. */
#define CURL_FORMAT_OFF_T "%lld"

/* The size of `curl_off_t', as computed by sizeof. */
#define CURL_SIZEOF_CURL_OFF_T 8

/* curl_off_t constant suffix. */
#define CURL_SUFFIX_CURL_OFF_T LL

/* unsigned curl_off_t constant suffix. */
#define CURL_SUFFIX_CURL_OFF_TU ULL

#endif

#endif /* __CURL_CURLBUILD_H */
#endif