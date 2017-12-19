#! /usr/bin/python -B

import re
import os
import subprocess
import sys

SEARCH_PATTERN = '*Sources/Apple/'

PROJECT_ROOT_NAME = "sp_unity_plugins"

XCODEPROJ_NAME = PROJECT_ROOT_NAME + ".xcodeproj/"
PBXPROJ_NAME = XCODEPROJ_NAME + "project.pbxproj"

SCRIPT_DIR = "tools/sort-Xcode-project-file.pl"

EXTENSIONS_REGEXP = r'.+(\.pbxproj)$'

def main():

    files_str = get_staged_files()

    if files_str == "" or files_str is None:
        sys.exit(0)
    # end if

    files = get_added_or_mod_proj_files(files_str)

    # reindent staged modified or added Hydra Xcode project file
    for file in files:
        if file.endswith(PBXPROJ_NAME):
            reorder_xcode_project()
            restage_file(file)
        # end if
    # end for

    sys.exit(0)

# end def main

def get_added_or_mod_proj_files(files):
    '''
    string to parse is in the format:
    M       install_reqs.cpp
    A       example.h
    D       upload_install.hpp
    M       web/launch_auditing_dragon_city.sh
    '''
    res = []

    results = re.compile(r'\n?[MA]\s+(' + EXTENSIONS_REGEXP + ')', re.M).finditer(files);

    while True:
        try:
            result = results.next()
        except StopIteration:
            break
        # end try

        res.append(result.group(1))

    # end while

    return res

# end def get_added_or_mod_proj_files

def restage_file(file):

    cmd_args = ["git", "add", file]
    execute_cmd(cmd_args)

# end def restage_file

def reorder_xcode_project():

    project_dir = get_project_dir()
    script_path = os.path.join(project_dir, SCRIPT_DIR)
    cmd_args = ["/usr/bin/perl", "-w", script_path, os.path.join(project_dir, XCODEPROJ_NAME)]
    execute_cmd(cmd_args)

# end def reorder_xcode_project

def get_staged_files():
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

def get_project_dir():
    result = find(SEARCH_PATTERN + PROJECT_ROOT_NAME, os.getcwd())
    if len(result) > 0:
        return result[0]
    else:
        sys.exit("Project dir '" + SEARCH_PATTERN + PROJECT_ROOT_NAME + "' not found")

# end def get_project_dir

def find(patern, path):
    return [line[2:] for line in subprocess.check_output("find . -type d -wholename " + patern, shell=True).splitlines()]

# end find(patern, path)

# make the module executable
if __name__ == "__main__":
    main()
    os._exit(0)
# end if
