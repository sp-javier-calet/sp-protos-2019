#!/usr/bin/env python

import sys
import os.path
import re
import subprocess

from check_conflict_settings import *

def main():
    files_str = get_staged_files_str()
    if files_str == "" or files_str is None:
        sys.exit(0)
    # end if

    files = get_added_or_mod_source_files(files_str)

    conflicted = get_conflicted_files(files)

    if len(conflicted) > 0:
        print("You have merge markers in the following files:")
        for path in conflicted:
            print(path)
        sys.exit(1)
    else:
        sys.exit(0)
# end def main


def get_conflicted_files(paths):
    conflicted = []
    for rpath in paths:
        path = rpath
        if os.path.isfile(path):
            with open(path, 'rb') as file:
                found = False
                for line in file:
                    if line.startswith(b'>>>>>>> ') or line.startswith(b'<<<<<<< '):
                        conflicted.append(rpath)
                        break;
                    # end if
                #end for
            #end with
        #end if
    #end for
    return conflicted
# end def get_conflicted_files


def should_skip_file(filepath):
    for path_to_skip in SKIP_PATHS:
        if path_to_skip in filepath:
            for path_to_include in INCLUDE_PATHS:
                if path_to_include in filepath:
                    return False
            # end for
            return True
        # end if
    # end for
    return False
# end def should_skip_file


def restage_file(file):
    cmd_args = ["git", "add", file]
    execute_cmd(cmd_args)
# end def restage_file


def get_added_or_mod_source_files(fileStr):
    '''
    string to parse is in the format:
    M       install_reqs.cpp
    A       example.h
    D       upload_install.hpp
    M       web/launch_auditing_dragon_city.sh
    '''
    res = []

    for line in fileStr.split('\n'):
        results = re.compile(r'^[MA]\s+(' + FILE_MATCH_RE + ')', re.M).finditer(line)
        while True:
            try:
                result = results.next()
            except StopIteration:
                break
            # end try

            file_path = result.group(1)
            if not should_skip_file(file_path):
                res.append(file_path)
            # end if
        # end while

    return res
# end def get_added_or_mod_source_files


def get_staged_files_str():
    '''
    executes git diff --cached --name-status and returns the result as a string
    '''
    cmd_args = ["git", "diff", "--cached", "--name-status"]
    result = execute_cmd(cmd_args)

    if result[0] != 0:
        sys.exit("Could not run get staged files for commit")
    # end if

    return result[1]
# end def get_staged_files


def execute_cmd(cmd):
    '''
    executes shell command and returns a tuple with return code, standard out and standard error
    '''
    _subprocess = subprocess.Popen( cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE )
    stdout, stderr = _subprocess.communicate()
    returncode = _subprocess.wait()

    return returncode, stdout, stderr
# end def execute_cmd


# make the module executable
if __name__ == "__main__":
    main()
    os._exit(0)
# end if
