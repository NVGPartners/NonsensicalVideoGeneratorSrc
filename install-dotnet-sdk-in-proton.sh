#!/bin/bash
export DOTNET_SDK_VERSION="8.0.411"
export DOTNET_SDK_INSTALLER="dotnet-sdk-$DOTNET_SDK_VERSION-win-x64.exe"
export DOTNET_SDK_OUT=".temp/$DOTNET_SDK_INSTALLER"
if [ ! -f $DOTNET_SDK_OUT ]; then
    wget -4 https://builds.dotnet.microsoft.com/dotnet/Sdk/$DOTNET_SDK_VERSION/$DOTNET_SDK_INSTALLER $DOTNET_SDK_OUT
fi
./proton.sh .temp C:\\NVGDev\\NVGMonoGame\\$DOTNET_SDK_OUT
