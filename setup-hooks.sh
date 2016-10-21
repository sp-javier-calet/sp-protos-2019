# setup game's pre-commit hook
mkdir -p .git/hooks
ln -s -f ../../scripts/hooks/pre-commit.sh .git/hooks/pre-commit
ln -s -f ../../scripts/hooks/post-commit.sh .git/hooks/post-commit
ln -s -f ../../scripts/hooks/post-checkout.sh .git/hooks/post-checkout
