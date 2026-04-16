#!/bin/bash

echo "Getting credentials..."
source ./cred.sh

echo "Publishing build script steamcmd_linux.vdf..."
steamcmd +login "$STEAMCMD_USERNAME" +run_app_build $HOME/NVGDev/NVGMonoGame/steamcmd_linux.vdf +quit
