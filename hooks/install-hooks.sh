#!/bin/bash

cd ..

if ! [ -d ".git" ]; then
    echo "Not in a git repository" >&2
    exit 1
fi

mkdir -p .git/hooks

SCRIPTS_DIR=$(cd "hooks" && pwd)
GIT_HOOKS_DIR=$(cd ".git/hooks" && pwd)

SCRIPTS=(pre-commit post-checkout post-merge)

for SCRIPT in "${SCRIPTS[@]}"; do
    ln -snf "$SCRIPTS_DIR/$SCRIPT" "$GIT_HOOKS_DIR/$SCRIPT"
done

echo "Installed ${SCRIPTS[*]}"
