#!/bin/sh

BUILDPATH=$(pwd)/bin/Release/netcoreapp3.0/linux-x64/publish

dotnet publish -r linux-x64 -c Release /p:DefineConstants=PRODUCTION_BUILD /p:Mode=CoreRT-Moderate /p:PublishSingleFile=true --self-contained true

if [ ! -d $BUILDPATH/plugins ]; then
    mkdir $BUILDPATH/plugins
    echo "\033[0;32mplugins copied."
fi

cp -rf plugins $BUILDPATH
echo "\033[0;32mplugins copied."

cp -rf config $BUILDPATH
echo "\033[0;32mconfig copied."

cp -rf nlog.config $BUILDPATH
echo "\033[0;32mnlog.config copied."

echo "\033[0;32mProduction build successful."
