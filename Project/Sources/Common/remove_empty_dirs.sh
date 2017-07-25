#!/bin/sh

REPO_PATH=$(git rev-parse --show-toplevel)

# Remove merge conflict files
find $REPO_PATH -name '*~HEAD.*' -type f -delete
find $REPO_PATH -name '*.orig*' -type f -delete

# Remove empty directories
find $REPO_PATH -name '*.DS_Store' -type f -delete
find $REPO_PATH -type d -depth -empty -exec echo "Removing "{} \; -exec rmdir {} \; -exec rm {}.meta \;
