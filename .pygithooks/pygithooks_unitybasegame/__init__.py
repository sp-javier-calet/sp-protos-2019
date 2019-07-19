#-----------------------------------------------------------------------
# __init__.py
#
# Copyright 2019 Social Point SL. All rights reserved.
#
#-----------------------------------------------------------------------

import os
import pygithook
from pygithook.generic import PreCommitStagedFilter, DetectUnmergedConflicts, RunGitLfs, RemoveBadBoms, \
    IncludeFilter, ExcludeFilter, RemoveTrailingSpace, AddEndLineBreak, IndentWithSpaces
from pygithook.coding import CommentHeader, ApplyDotnetFormat, UnityHooks, SourceCodeMatch
from pygithook.strmatch import AndMatch, OrMatch, BaseNameMatch, NotMatch


def load_hooks(repo):
    filters = __filters(repo).values()
    hooks = (
        RunGitLfs(),
        UnityHooks(repo, "Project/Assets"),
        PreCommitStagedFilter(repo, filters)
    )
    return hooks


COPYRIGHT_HEADER = u"""{{c}}-----------------------------------------------------------------------
{{c}} {{filename}}
{{c}}
{{c}} Copyright {{now.strftime('%Y')}} Social Point SL. All rights reserved.
{{c}}
{{c}}-----------------------------------------------------------------------
"""

def __srcfilter(fltr):
    match = AndMatch(
        SourceCodeMatch(),
        "Project/Assets/*",
        NotMatch(OrMatch(
            "Project/Assets/GPGSConstants.cs",
            "Project/Assets/Plugins/External/*"
        ))
    )
    return IncludeFilter(match, fltr)

DOTNET_DEFINES = "ADMIN_PANEL;UNITY;UNITY_EDITOR;UNITY_IOS;UNITY_ANDROID;UNITY_STANDALONE;UNITY_2018_3_OR_NEWER"

def __filters(repo):
    return {
        'conflicts: detect unmerged files': DetectUnmergedConflicts(),
        'copyright: add copyright header': __srcfilter(CommentHeader(COPYRIGHT_HEADER)),
        'dotnet-format: apply source code format': __srcfilter(ApplyDotnetFormat(DOTNET_DEFINES)),
        'trailing-space: remove end of line whitespace': __srcfilter(RemoveTrailingSpace()),
        'end-linebreak: add linebreak at the end of files': __srcfilter(AddEndLineBreak()),
        'indent-spaces: replace indent tabs with spaces': __srcfilter(IndentWithSpaces()),
    }

def load_filters(repo):
    filters = __filters(repo)
    filters['bad-boms: remove boms in the middle of text files'] = RemoveBadBoms()
    return filters
