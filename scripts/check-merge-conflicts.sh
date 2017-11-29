#!/bin/sh

if [ -z "$1" ]
then
    files=$(git diff --cached --name-only)
else
    files=$(find . -iname "${1}" -type f)
fi

if [[ -z "$files" ]]
then
    exit 0
fi

files=$(echo "$files" | sed 's| |\\ |g')
count=$(echo "$files" | wc -l | xargs)
echo "checking ${count} files ..."

echo "$files" | xargs egrep '[><]{7}' -H -I --line-number

if [ $? == 0 ]
then
    echo "\n\nWARNING: You have merge markers in the above files, lines. Fix them before committing.\n\n"
    exit 1
fi
