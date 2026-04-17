#!/bin/bash
# NVG-focused Proton script
# Install GE-Proton10-34 to ~/.local/share/Steam/compatibilitytools.d/
export STEAM_COMPAT_CLIENT_INSTALL_PATH="$HOME/.local/share/Steam"
export PROTON=$STEAM_COMPAT_CLIENT_INSTALL_PATH/compatibilitytools.d/GE-Proton10-34/proton
export STEAM_COMPAT_DATA_PATH="$STEAM_COMPAT_CLIENT_INSTALL_PATH/steamapps/compatdata/2516360"
export NVGDEV="$STEAM_COMPAT_DATA_PATH/pfx/drive_c/NVGDev"
ln -sf $HOME/NVGDev $NVGDEV
cd $NVGDEV/NVGMonoGame/$1
$PROTON run $2 $3
