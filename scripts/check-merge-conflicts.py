#!/usr/bin/env python

import sys
import git
import os.path
import argparse

def get_conflicted_files(git_src, git_dst, base_path, path_pattern):
    def get_tree_paths(tree, paths):
        for blob in tree.blobs:
            paths.append(blob.path)
        for subtree in tree.trees:
            get_tree_paths(subtree, paths)

    repo = git.Repo(base_path)
    if git_src is None:
        paths = []
        get_tree_paths(repo.tree(git_dst), paths)
        print('found %d files in %s' % (len(paths), args.dst))
    else:
        span = '%s..%s' % (git_src, git_dst)
        paths = repo.git.diff(span, name_only=True, diff_filter='AMCX').split()
        print('found %d files changed from %s to %s' % (len(paths), args.src, args.dst))
    conflicted = []
    for rpath in paths:
        path = os.path.join(base_path, rpath)
        if os.path.isfile(path):
            if path_pattern is None or fnmatch.fnmatch(path, path_pattern):
                with open(path, 'rb') as file:
                    found = False
                    for line in file:
                        if line.startswith(b'>>>>>>>') or line.startswith(b'<<<<<<<'):
                            found = True
                            break;
                    if found:
                        conflicted.append(rpath)
                        break
    return conflicted

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Detect conflict markers in repo files.')
    parser.add_argument("--src", help="git origin ref to check for changed files", default=None)
    parser.add_argument("--dst", help="git destination ref to check for changed files", default='HEAD')
    parser.add_argument("--path", help="root path of the repository", default='.')
    parser.add_argument("--pattern", help="file pattern to match", default=None)

    args = parser.parse_args()
    conflicted = get_conflicted_files(args.src, args.dst, args.path, args.pattern)

    if len(conflicted) > 0:
        print("You have merge markers in the following files:")
        for path in conflicted:
            print(path)
        exit(1)
