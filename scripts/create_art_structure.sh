#!/bin/bash
PLACEHOLDER=".gitkeep"
BASE_DIR="$1/External"

function create_folder
{
	DIR="$BASE_DIR/$1"
	mkdir -p "$DIR"
	touch "$DIR/$PLACEHOLDER"
}

function create_3d_asset_type_folders
{
	create_folder "Gfx3D/$1/Materials"
	create_folder "Gfx3D/$1/Meshes"
	create_folder "Gfx3D/$1/Textures"
	create_folder "Gfx3D/$1/Animations"
	create_folder "Gfx3D/$1/Prefabs"
}

create_folder "Audio/Music"
create_folder "Audio/Effects"
create_3d_asset_type_folders "Characters"
create_3d_asset_type_folders "Engine"
create_3d_asset_type_folders "Fx"
create_3d_asset_type_folders "Environment"
create_3d_asset_type_folders "Props"
create_folder "Gfx2D/Fonts"
create_folder "Gfx2D/Sprites"
create_folder "Media"
create_folder "Shaders"
