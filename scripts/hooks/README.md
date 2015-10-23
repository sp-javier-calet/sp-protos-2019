# Unity Git Hooks

https://github.com/3pjgames/unity-git-hooks
install-hooks.sh and this README has been modified

Why use this hooks?
http://altprog.com/en/2014/10/28/empty-folders-meta-files-unity-3d-and-git/

Git hooks for Unity project.

To manage a Unity project, the Assets meta file should also been added to
repository. But sometimes the meta files and the corresponding asset files
and directories are inconsistent, then meta files will lead to conflicts when
a team is collaborating on the same code base.

## Features

- Stop committing if meta files and asset files and directories are
  inconsistent. If an asset file is added, its meta file and meta files of all
  its containing directories should also been added. If a asset file is
  deleted, its meta file and meta files of all its empty containing
  directories should also been deleted. When meta files are added/deleted,
  asset files and directories should also been consistent.
- Delete empty asset directories after checkout and merge. Unity keep
  generating meta file for empty asset directory, but git does not trace
  directory.

## Usage

Copy files `post-checkout` `post-merge` and `pre-commit` to .git/hooks in your
git repository. If you also have hooks defined in these files, append them to
existing files.
