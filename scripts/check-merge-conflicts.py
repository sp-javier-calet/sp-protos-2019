#!/usr/bin/env python

import sys
import git
import os.path
import argparse
import fnmatch
import glob

parser = argparse.ArgumentParser(description='Detect conflict markers in repo files.')
parser.add_argument("--src", help="git origin ref to check for changed files", default=None)
parser.add_argument("--dst", help="git destination ref to check for changed files", default='HEAD')
parser.add_argument("--path", help="root path of the repository", default='.')
parser.add_argument("--pattern", help="file pattern to match", default=None)

args = parser.parse_args()
base_path = args.path
path_pattern = args.pattern

def get_tree_paths(tree, paths):
    for blob in tree.blobs:
        paths.append(blob.path)
    for subtree in tree.trees:
        get_tree_paths(subtree, paths)

repo = git.Repo(base_path)
if args.src is None:
    paths = []
    get_tree_paths(repo.tree(args.dst), paths)
    print args
    print 'found %d files changed in %s' % (len(paths), args.dst)
else:
    span = '%s..%s' % (args.src, args.dst)
    paths = repo.git.diff(span, name_only=True).split()
    print 'found %d files changed from %s to %s' % (len(paths), args.src, args.dst)
conflicted = []
for rpath in paths:
    path = os.path.join(base_path, rpath)
    if os.path.isfile(path):
        if path_pattern is None or fnmatch.fnmatch(path, path_pattern):
            with open(path,'r') as file:
                found = False
                for line in file:
                    if line.startswith('>>>>>>>') or line.startswith('<<<<<<<'):
                        found = True
                        break;
                if found:
                    conflicted.append(rpath)
                    break

if len(conflicted) > 0:
    print >> sys.stderr, ("You have merge markers in the following files:")
    for path in conflicted:
        print >> sys.stderr, path
    exit(1)
