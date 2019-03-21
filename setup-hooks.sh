if ! [ -d ".git" ]; then
    echo "Not in a git repository" >&2
    exit 1
fi

mkdir -p .git/hooks

# lfs sets pre-push, post-commit, post-checkout and post-merge scripts
# This resets these scripts

# git lfs update [--manual | --force]

# Updates the Git hooks used by Git LFS. Silently upgrades known hook contents.
# If you have your own custom hooks you may need to use one of the extended
# options below.

# Options:

# * --manual -m
#     Print instructions for manually updating your hooks to include git-lfs
#     functionality. Use this option if git lfs update fails because of existing
#     hooks and you want to retain their functionality.

# * --force -f
#     Forcibly overwrite any existing hooks with git-lfs hooks. Use this option
#     if git lfs update fails because of existing hooks but you don't care
#     about their current contents.
git lfs update --force 


SCRIPTS_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )/scripts/hooks" && pwd )"
GIT_HOOKS_DIR=$(cd ".git/hooks" && pwd)

echo "SCRIPTS_DIR: " $SCRIPTS_DIR
echo "GIT_HOOKS_DIR: " $GIT_HOOKS_DIR

SCRIPTS=(pre-commit post-checkout post-merge)

for SCRIPT in "${SCRIPTS[@]}"; do
	echo "SCRIPT: " $SCRIPT
	if [ "$SCRIPT" == "pre-push" ] || [ "$SCRIPT" == "post-commit" ] || [ "$SCRIPT" == "post-checkout" ] || [ "$SCRIPT" == "post-merge" ]; then
		cat "$SCRIPTS_DIR/$SCRIPT" >> "$GIT_HOOKS_DIR/$SCRIPT"
	else
    	ln -snf "$SCRIPTS_DIR/$SCRIPT" "$GIT_HOOKS_DIR/$SCRIPT"
	fi
done

echo "Installed ${SCRIPTS[*]}"
